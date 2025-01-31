using Microsoft.Azure.Cosmos;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Identity;
using System.Text.Json;
using System.Text;

namespace YijingCosmos.Encryption
{
    public class ReadOnlyColumnEncryption
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CryptographyClient _cryptoClient;
        private readonly string _databaseId;
        private readonly string _containerId;

        public ReadOnlyColumnEncryption(
            string cosmosEndpoint,
            string keyVaultUrl,
            string keyName,
            string databaseId,
            string containerId)
        {
            var credential = new DefaultAzureCredential();
            _cosmosClient = new CosmosClient(cosmosEndpoint, credential);
            
            var keyClient = new KeyClient(new Uri(keyVaultUrl), credential);
            var key = keyClient.GetKey(keyName).Value;
            _cryptoClient = new CryptographyClient(key.Id, credential);
            
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public async Task EncryptReadOnlyColumn<T>(
            string partitionKey, 
            string id, 
            string columnPath,
            JsonPatchOperation operation)
        {
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);

            try
            {
                // Read the current document
                var response = await container.ReadItemAsync<T>(
                    id, 
                    new PartitionKey(partitionKey)
                );
                var item = response.Resource;

                // Get the value to encrypt
                var valueToEncrypt = GetValueFromPath(item, columnPath);
                if (valueToEncrypt == null)
                    throw new ArgumentException($"Column path {columnPath} not found");

                // Encrypt the value
                var encryptedValue = await EncryptValue(valueToEncrypt.ToString());

                // Create patch operation
                var patchOperations = new List<PatchOperation>
                {
                    PatchOperation.Replace(columnPath, encryptedValue),
                    PatchOperation.Add("/_metadata/encrypted", new[]
                    {
                        new
                        {
                            path = columnPath,
                            timestamp = DateTime.UtcNow,
                            type = "readonly"
                        }
                    })
                };

                // Apply patch operations
                await container.PatchItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey),
                    patchOperations
                );
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Document with id {id} not found");
            }
        }

        public async Task<string> DecryptReadOnlyColumn<T>(
            string partitionKey,
            string id,
            string columnPath)
        {
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);

            try
            {
                // Read the document
                var response = await container.ReadItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey)
                );
                var item = response.Resource;

                // Get encrypted value
                var encryptedValue = GetValueFromPath(item, columnPath)?.ToString();
                if (encryptedValue == null)
                    throw new ArgumentException($"Column path {columnPath} not found");

                // Decrypt the value
                return await DecryptValue(encryptedValue);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Document with id {id} not found");
            }
        }

        private object GetValueFromPath<T>(T item, string path)
        {
            var document = JsonSerializer.Serialize(item);
            using var jsonDoc = JsonDocument.Parse(document);
            
            var pathSegments = path.TrimStart('/').Split('/');
            var current = jsonDoc.RootElement;

            foreach (var segment in pathSegments)
            {
                if (!current.TryGetProperty(segment, out var next))
                    return null;
                current = next;
            }

            return current.GetRawText();
        }

        private async Task<string> EncryptValue(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var encryptResult = await _cryptoClient.EncryptAsync(
                EncryptionAlgorithm.RsaOaep,
                bytes
            );

            return Convert.ToBase64String(encryptResult.Ciphertext);
        }

        private async Task<string> DecryptValue(string encryptedValue)
        {
            var bytes = Convert.FromBase64String(encryptedValue);
            var decryptResult = await _cryptoClient.DecryptAsync(
                EncryptionAlgorithm.RsaOaep,
                bytes
            );

            return Encoding.UTF8.GetString(decryptResult.Plaintext);
        }
    }

    // Example document class
    public class SensitiveDocument
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string ReadOnlyData { get; set; }
        public object Metadata { get; set; }
    }

    // Example usage class
    public class DocumentEncryptionService
    {
        private readonly ReadOnlyColumnEncryption _encryption;

        public DocumentEncryptionService(
            string cosmosEndpoint,
            string keyVaultUrl,
            string keyName,
            string databaseId,
            string containerId)
        {
            _encryption = new ReadOnlyColumnEncryption(
                cosmosEndpoint,
                keyVaultUrl,
                keyName,
                databaseId,
                containerId
            );
        }

        public async Task EncryptReadOnlyColumn(string partitionKey, string documentId)
        {
            var operation = new JsonPatchOperation
            {
                OperationType = JsonPatchOperationType.Replace,
                Path = "/readOnlyData",
                Value = null // Will be set during encryption
            };

            await _encryption.EncryptReadOnlyColumn<SensitiveDocument>(
                partitionKey,
                documentId,
                "/readOnlyData",
                operation
            );
        }

        public async Task<string> DecryptReadOnlyColumn(string partitionKey, string documentId)
        {
            return await _encryption.DecryptReadOnlyColumn<SensitiveDocument>(
                partitionKey,
                documentId,
                "/readOnlyData"
            );
        }
    }
}

// Example usage
public class Program
{
    public static async Task Main()
    {
        var service = new DocumentEncryptionService(
            cosmosEndpoint: "your_cosmos_endpoint",
            keyVaultUrl: "your_keyvault_url",
            keyName: "your_key_name",
            databaseId: "your_database",
            containerId: "your_container"
        );

        // Encrypt read-only column
        await service.EncryptReadOnlyColumn("partition1", "doc1");

        // Decrypt read-only column
        var decryptedValue = await service.DecryptReadOnlyColumn("partition1", "doc1");
        Console.WriteLine($"Decrypted value: {decryptedValue}");
    }
}
