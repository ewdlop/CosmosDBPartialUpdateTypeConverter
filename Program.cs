using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CosmosDBPartialUpdateTypeConverter;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;
using System.Reflection;

//https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-getting-started?tabs=dotnet


const string connectionStringSecret = "MyCosmosDBConnectionString";
const string databaseIdSecret = "MyCosmosDBDatabaseId";
string azureKeyVaultEndpoint = $"https://kv-ray81081506952833917.vault.azure.net/";

SecretClient secretClient = new SecretClient(new Uri(azureKeyVaultEndpoint), new DefaultAzureCredential(true));
Azure.Response<KeyVaultSecret> connectionStringSecretResponse = await secretClient.GetSecretAsync(connectionStringSecret);
Azure.Response<KeyVaultSecret> databaseIdSecretResponse = await secretClient.GetSecretAsync(databaseIdSecret);

//place holders
//should fetch the key from the Azure Key Vault
string connectionString = connectionStringSecretResponse.Value.Value;
string databaseId = databaseIdSecretResponse.Value.Value;

dynamic sampleItem = new { id = "myId", name = "myName" };

// sample codes
// still need more testing
// need to limit to 10 PatchOperations
// need to add more error handling
// need to not forget to add the "/" to the path
PatchOperationList patchOperationList = new()
{
    PatchOperation.Add("/age", 33),
    PatchOperation.Add("/address", new JObject
    {
        { "city", "Seattle" },
        { "state", "WA" },
        { "postalCode", "98052" },
        { "test", new JObject
        {
            { "test1", "123" },
            { "test2", "456" }
        }
        }
    }),
    Enumerable.Range(0, 10).Select(i => new {}), //ignored
    1, //ignored
    new { Label = "label", Number = 44 },
    ("test","123"),
    "",//ignored
    {"test","123" },
    {"test","123","456" }, //ignored
    ("","",""),//ignored
};

PatchOperationList patchOperationList2 = new()
{
    new JObject
    {
        { "city", "Seattle" },
        { "state", "WA" },
        { "postalCode", "98052" },
        { "test", new JObject
        {
            { "test1", "123" },
            { "test2", "456" }
        }
        }
    }
};

PatchOperationList patchOperationList3 = new List<PatchOperation>();
PatchOperationList patchOperationList4 = PatchOperation.Move("/from", "/to");
PatchOperationList patchOperationList5 = [PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there")];
PatchOperationList patchOperationList6 = new PatchOperationList { PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there") };
PatchOperationList patchOperationList7 = new PatchOperationList(new List<PatchOperation> { PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there") });

List<PatchOperation> patchOperations = patchOperationList;
patchOperationList.AddIncrement("age", 1);
patchOperationList.AddIncrement("age", 2);
patchOperationList.AddIncrement("age", 2);
patchOperationList.AddIncrement("age", 2);
patchOperationList.AddIncrement("age", 2);
patchOperationList.AddIncrement("age", 2);
patchOperationList.AddMove("test", "test2");
patchOperationList.AddRemove("test2");
patchOperationList.AddReplace("age", 55);

patchOperationList3.Add(PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there"));
IList<PatchOperation> patchOperations2 = new PatchOperationList();
IReadOnlyList<PatchOperation> patchOperations3 = patchOperationList;

PatchOperationList patchOperationList8 = new PatchOperationListBuilder()
    .With(PatchOperation.Move("/from", "/to"))
    .WithAdd("age", 70)
    .WithAdd("address", new JObject
    {
        { "city", "Seattle" },
        { "state", "WA" },
        { "postalCode", "98052" }
    })
    .WithAdd(new { Label = "label", Number = 41 })
    .Build();

for (int i = 0; i < patchOperationList.Count; i++)
{
    Console.WriteLine(patchOperationList[i]);
}

foreach (var patchOperation in patchOperationList)
{
    Console.WriteLine(patchOperation);
}

patchOperationList.Select(patchOperation => patchOperation.Path).ToList().ForEach(Console.WriteLine);

patchOperationList.ForEach(Console.WriteLine);

//usage of PatchOperationList
try
{
    CosmosClient client = new(connectionString);
    Database database = client.GetDatabase(databaseId);
    string partitionKeyPath = "/myPartitionKey";
    ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync("myTestingContainer", partitionKeyPath);
    if (containerResponse.StatusCode == HttpStatusCode.Created || containerResponse.StatusCode == HttpStatusCode.OK)
    {
        Console.WriteLine(containerResponse.StatusCode == HttpStatusCode.Created ? "Container created" : "Container already exists");
        Container container = containerResponse.Container;
        Guid guid = Guid.NewGuid();
        string itemPartitionKey = "itemPartitionKey";
        dynamic item = new { id = guid.ToString(), name = "myName", myPartitionKey = itemPartitionKey };
        string myPartitionKey = $"{itemPartitionKey}";

        dynamic itemResponse = await container.CreateItemAsync(item);
        Console.WriteLine($"Created item in database with id: {itemResponse.Resource.id}");


        //Partial update
        PartitionKey partitionKey = new PartitionKey(myPartitionKey);
        
        if(patchOperationList.Count <= 10)
        {
            ItemResponse<dynamic> patchResponse = await container.PatchItemAsync<dynamic>(item.id, partitionKey, patchOperations);

            if (patchResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"Patched item in database with id: {patchResponse.Resource.id}");
            }
            else
            {
                Console.WriteLine($"Failed to patch item in database with id: {patchResponse.Resource.id}");
            }
        }
        else
        {
            TransactionalBatchRequestOptions requestOptions = new TransactionalBatchRequestOptions();
            TransactionalBatchItemRequestOptions itemRequestOptions = new TransactionalBatchItemRequestOptions();
            TransactionalBatch batch = container.CreateTransactionalBatch(partitionKey);
            const int maxSize = 10;
            int batchCount = (int)Math.Ceiling((double)patchOperationList.Count / maxSize);
            Enumerable.Range(0, batchCount).ToList().ForEach(i =>
            {
                List<PatchOperation> batchPatchOperations = patchOperationList.Skip(i * maxSize).Take(maxSize).ToList();
                batch.PatchItem(item.id, batchPatchOperations);
            });
            TransactionalBatchResponse batchResponse = await batch.ExecuteAsync(requestOptions: requestOptions);
            if (batchResponse.IsSuccessStatusCode)
            {
                dynamic lastResponse = batchResponse.GetOperationResultAtIndex<dynamic>(batchResponse.Count - 1).Resource;
                Console.WriteLine(JsonConvert.SerializeObject(lastResponse, Formatting.Indented));
            }
        }
       

        ItemResponse<dynamic> patchResponse2 = await container.PatchItemAsync<dynamic>(item.id, partitionKey, patchOperationList2);

        if (patchResponse2.StatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine($"Patched item in database with id: {patchResponse2.Resource.id}");
        }
        else
        {
            Console.WriteLine($"Failed to patch item in database with id: {patchResponse2.Resource.id}");
        }
    }
}
catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound ||
                                 ex.StatusCode == HttpStatusCode.BadRequest||
                                 ex.StatusCode == HttpStatusCode.FailedDependency)
{
    Console.WriteLine($"CosmosException: {ex}");
}
catch (CosmosException ex)
{
    Console.WriteLine($"Other CosmosException: {ex}");
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex}");
}

namespace CosmosDBPartialUpdateTypeConverter
{
    /// <summary>
    ///Not done yet
    ///Need to limit to 10 PatchOperations and more testing
    ///https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-faq
    /// </summary>
    public class PatchOperationList : IList<PatchOperation>, IReadOnlyList<PatchOperation>
    {
        private readonly List<PatchOperation> _patchOperations = [];

        public static implicit operator PatchOperationList(List<PatchOperation> patchOperations) => new(patchOperations);
        public static implicit operator PatchOperationList(PatchOperation[] patchOperations) => new([.. patchOperations]);
        public static implicit operator PatchOperationList(PatchOperation patchOperation) => new(patchOperation);
        public static implicit operator List<PatchOperation>(PatchOperationList patchOperation) => patchOperation._patchOperations;
        
        public PatchOperationList() : this([]) { }
        
        public PatchOperationList(IList<PatchOperation> patchOperations) => _patchOperations = [.. patchOperations];

        public PatchOperationList(List<PatchOperation> patchOperations) => _patchOperations = [.. patchOperations];

        public PatchOperationList(IEnumerable<PatchOperation> patchOperations) : this(patchOperations.ToList()) { }
        
        public PatchOperationList(PatchOperation patchOperation) : this([patchOperation]) { }
        
        public int Count => _patchOperations.Count;
        
        public PatchOperation this[int index]
        {
            get => _patchOperations[index];
            set => _patchOperations[index] = value;
        }

        public PatchOperationListBuilder Builder => new(_patchOperations);

        public bool IsReadOnly => false;

        PatchOperation IList<PatchOperation>.this[int index] 
        {
            get => (_patchOperations as IList<PatchOperation>)[index];
            set => (_patchOperations as IList<PatchOperation>)[index] = value;
        }

        public void Add(string path, object? value) => _patchOperations.Add(path, value);

        public void Add(PatchOperation item) => _patchOperations.Add(item);

        public void Add(JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject, nameof(jObject));
            _patchOperations.Add(jObject);
        }

        public void Add<T>(T entity)
            where T : class => _patchOperations.Add(entity);

        public void Add<T>(IEnumerable<T> entities)
            where T : class => _patchOperations.Add(entities);

        public void Add<T>(params T[] entities)
            where T : class => _patchOperations.Add(entities);

        public void Add(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null) => _patchOperations.Add(value, propertyInfoFilter);

        public void AddAppend(string path, object? value) => _patchOperations.AddAppend(path, value);

        public void AddIncrement(string path, long value) => _patchOperations.AddIncrement(path, value);

        public void AddIncrement(string path, double value) => _patchOperations.AddIncrement(path, value);

        public void AddMove(string from, string path) => _patchOperations.AddMove(from, path);

        public void AddRemove(string path) => _patchOperations.AddRemove(path);

        public void AddReplace(string path, object? value) => _patchOperations.AddReplace(path, value);

        public void AddSet(string path, object? value) => _patchOperations.AddSet(path, value);

        public void AddSet(JObject value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.AddSet(value);
        }

        public void AddSet<T>(T entity)
            where T : class => _patchOperations.AddSet(entity);

        public void AddSet<T>(IEnumerable<T> entities)
            where T : class => _patchOperations.AddSet(entities);

        public void AddSet<T>(params T[] entities)
            where T : class => _patchOperations.AddSet(entities);

        public void AddSet(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null) => _patchOperations.AddSet(value, propertyInfoFilter);

        public void AddSet(IEnumerable<object> values) => _patchOperations.AddSet(values);

        public void AddSet(params object[] values) => _patchOperations.AddSet(values);

        public int IndexOf(PatchOperation item) => _patchOperations.IndexOf(item);

        public void Insert(int index, PatchOperation item) => _patchOperations.Insert(index, item);

        public void RemoveAt(int index) => _patchOperations.RemoveAt(index);

        public void Clear() => _patchOperations.Clear();

        public bool Contains(PatchOperation item) => _patchOperations.Contains(item);

        public void CopyTo(PatchOperation[] array, int arrayIndex) => _patchOperations.CopyTo(array, arrayIndex);

        public bool Remove(PatchOperation item) => _patchOperations.Remove(item);

        public IEnumerator<PatchOperation> GetEnumerator() => _patchOperations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ForEach(Action<PatchOperation> action) => _patchOperations.ForEach(action);

        public IReadOnlyList<PatchOperation> AsReadOnly() => _patchOperations.AsReadOnly();
    }

    /// <summary>
    /// Need more work
    /// </summary>
    public static class PatchOperationExtension
    {
        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, params PatchOperation[] entities)
        {
            patchOperations.AddRange(entities);
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Add(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, string path, T? value)
        {
            patchOperations.Add(PatchOperation.Add(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject, nameof(jObject));
            foreach (var property in jObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                patchOperations.Add(PatchOperation.Add(BuildPath(key), value));
            }
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, T entity)
            where T : class
        {
            patchOperations.AddRange(entity switch
            {
                string => Enumerable.Empty<PatchOperation>(),
                _ => typeof(T).GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
            where T : class
        {
            patchOperations.AddRange(entities.Where(entity => entity is not string)
                .SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, params T[] entities)
            where T : class
        {
            patchOperations.AddRange(entities switch
            {
                [string item1, string item2] => [PatchOperation.Add(BuildPath(item1), item2)],
                _ => entities.Where(entity => entity is not string).SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity))))
            });
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            patchOperations.AddRange(value switch
            {
                (string item1, object item2) => [PatchOperation.Add(BuildPath(item1), item2)],
                (object item1, object item2) => [ PatchOperation.Add(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties().Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
                    .Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(value)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, IEnumerable<object> values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Add(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Add(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(value)))
            }));
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, params object[] values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Add(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Add(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(value)))
            }));
            return patchOperations;
        }
        
        //For Appending to an json array
        //the path must exist first
        //need more testing for the corner cases
        public static List<PatchOperation> AddAppend(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Add($"{BuildPath(path)}/`", value));
            return patchOperations;
        }

        public static List<PatchOperation> AddIncrement(this List<PatchOperation> patchOperations, string path, long value)
        {
            patchOperations.Add(PatchOperation.Increment(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddIncrement(this List<PatchOperation> patchOperations, string path, double value)
        {
            patchOperations.Add(PatchOperation.Increment(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddMove(this List<PatchOperation> patchOperations, string from, string path)
        {
            patchOperations.Add(PatchOperation.Move(BuildPath(from), BuildPath(path)));
            return patchOperations;
        }

        public static List<PatchOperation> AddRemove(this List<PatchOperation> patchOperations, string path)
        {
            patchOperations.Add(PatchOperation.Remove(BuildPath(path)));
            return patchOperations;
        }

        public static List<PatchOperation> AddReplace(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Replace(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Set(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, string path, T? value)
        {
            patchOperations.Add(PatchOperation.Set(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, T entity)
            where T : class
        {
            patchOperations.AddRange(entity switch
            {
                string => Enumerable.Empty<PatchOperation>(),
                _ => typeof(T).GetProperties().Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(entity)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject, nameof(jObject));
            foreach (var property in jObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                patchOperations.Add(PatchOperation.Set(BuildPath(key), value));
            }
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
            where T : class
        {
            patchOperations.AddRange(entities.Where(entity => entity is not string)
                .SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, params T[] entities)
            where T : class
        {
            patchOperations.AddRange(entities switch
            {
                [string item1, string item2] => [PatchOperation.Set(BuildPath(item1), item2)],
                _ => entities.Where(entity => entity is not string).SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(entity))))
            });
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            patchOperations.AddRange(value switch
            {
                (string item1, object item2) => [PatchOperation.Set(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Set(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties().Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
                    .Select(property => PatchOperation.Set(property.Name, property.GetValue(value)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, IEnumerable<object> values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Set(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Set(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Set(property.Name, property.GetValue(value)))
            }));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, params object[] values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Set(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Set(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(value)))
            }));
            return patchOperations;
        }

        //Todo
        //Should use regex to check if string is of the form ^\/[A-Za-z0-9]+$ 
        //or of the form ^\/[A-Za-z0-9]+\/[A-Za-z0-9]+$, recursively for more than 2 level
        //for path such as /address/city
        public static string BuildPath(string path)
        {
            return path.StartsWith('/') ? path : $"/{path}";
        }
    }

    //Possibly need more work
    public class PatchOperationListBuilder(List<PatchOperation> _patchOperations)
    {
        public PatchOperationListBuilder() : this(new List<PatchOperation>()) { }
        public PatchOperationListBuilder(IEnumerable<PatchOperation> patchOperations) : this(patchOperations.ToList()) { }
        public PatchOperationListBuilder(PatchOperation patchOperation) : this(new List<PatchOperation> { patchOperation }) { }
        public PatchOperationListBuilder(params PatchOperation[] patchOperations) : this(patchOperations.ToList()) { }

        public PatchOperationListBuilder With(PatchOperation patchOperation)
        {
            ArgumentNullException.ThrowIfNull(patchOperation, nameof(patchOperation));
            _patchOperations.Add(patchOperation);
            return this;
        }

        public PatchOperationListBuilder With(IEnumerable<PatchOperation> entities)
        {
            _patchOperations.AddRange(entities);
            return this;
        }

        public PatchOperationListBuilder With(params PatchOperation[] patchOperation)
        {
            _patchOperations.AddRange(patchOperation);
            return this;
        }

        public PatchOperationListBuilder WithAdd(string path, object? value)
        {
            _patchOperations.Add(path, value);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(string path, T? value)
        {
            _patchOperations.Add(path, value);
            return this;
        }

        public PatchOperationListBuilder WithAdd(JObject value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.Add(value);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(T entity)
            where T : class
        {
            _patchOperations.Add(entity);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(IEnumerable<T> entities)
            where T : class
        {
            _patchOperations.Add(entities);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(params T[] entities)
            where T : class
        {
            _patchOperations.Add(entities);
            return this;
        }

        public PatchOperationListBuilder WithAdd(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            _patchOperations.Add(value, propertyInfoFilter);
            return this;
        }

        public PatchOperationListBuilder WithAddAppend(string path, object? value)
        {
            _patchOperations.AddAppend(path, value);
            return this;
        }

        public PatchOperationListBuilder WithIncrement(string path, long value)
        {
            _patchOperations.AddIncrement(path, value);
            return this;
        }

        public PatchOperationListBuilder WithMove(string from, string path)
        {
            _patchOperations.AddMove(from, path);
            return this;
        }

        public PatchOperationListBuilder WithRemove(string path)
        {
            _patchOperations.AddRemove(path);
            return this;
        }

        public PatchOperationListBuilder WithReplace(string path, object? value)
        {
            _patchOperations.AddReplace(path, value);
            return this;
        }

        public PatchOperationListBuilder WithSet(string path, object? value)
        {
            _patchOperations.AddSet(path, value);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(string path, T? value)
        {
            _patchOperations.AddSet(path, value);
            return this;
        }

        public PatchOperationListBuilder WithSet(JObject value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.AddSet(value);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(T entity)
            where T : class
        {
            _patchOperations.AddSet(entity);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(IEnumerable<T> entities)
            where T : class
        {
            _patchOperations.AddSet(entities);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(params T[] entities)
            where T : class
        {
            _patchOperations.AddSet(entities);
            return this;
        }

        public PatchOperationListBuilder WithSet(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            _patchOperations.AddSet(value, propertyInfoFilter);
            return this;
        }

        public PatchOperationList Build() => _patchOperations.Where(op=> op is not null).ToList();
    }
}