//https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-getting-started?tabs=dotnet
//https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
//Microsoft.AspNetCore.JsonPatch
//IETF RFC 6902 JSON Patch specification
//https://tools.ietf.org/html/rfc6902
//From Microsoft:
//The Partial document update operation is based on the JSON Patch RFC. Property names in paths need to escape the ~ and / characters as ~0 and ~1, respectively.
//Not to confused with Azure.JsonPatchDocument, this uses System.Text.Json for serialization and not Newtonsoft.Json

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CosmosDBPartialUpdateTypeConverter;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using ConsoleApp;
using Microsoft.Azure.Cosmos.Linq;
using OneOf;

const string connectionStringSecret = "MyCosmosDBConnectionString";
const string databaseIdSecret = "MyCosmosDBDatabaseId";

Console.WriteLine("Hello from CosmosDB Partial Update!");

//Azure Key Vault
//https://docs.microsoft.com/en-us/azure/key-vault/general/basic-concepts
//https://docs.microsoft.com/en-us/azure/key-vault/secrets/quick-create-net
//https://docs.microsoft.com/en-us/azure/key-vault/secrets/quick-create-python
//https://learn.microsoft.com/en-us/azure/key-vault/secrets/quick-create-node?tabs=azure-cli%2Cwindows

Console.WriteLine("Please enter your Azure Key Vault endpoint:");
string? azureKeyVaultEndpoint = Console.ReadLine();

if (string.IsNullOrWhiteSpace(azureKeyVaultEndpoint))
{
    Console.WriteLine("Azure Key Vault endpoint is required.");
    return;
}

SecretClient secretClient = new SecretClient(new Uri(azureKeyVaultEndpoint), new DefaultAzureCredential(includeInteractiveCredentials: true));
Azure.Response<KeyVaultSecret> connectionStringSecretResponse = await secretClient.GetSecretAsync(connectionStringSecret);
Azure.Response<KeyVaultSecret> databaseIdSecretResponse = await secretClient.GetSecretAsync(databaseIdSecret);

//place holders
//should fetch the key from the Azure Key Vault
string connectionString = connectionStringSecretResponse.Value.Value;
string databaseId = databaseIdSecretResponse.Value.Value;

# region sample
{
    dynamic sampleItem = new { id = "myId", name = "myName" };

    // sample codes
    // still need more testing
    // need to add more error handling
    // need to not forget to add the "/" to the path
    // not thread safe
    // need Unit tests
    // How does IEnumerable with Add Method truly works with collection initialization?
    // Does looking at IL helps to understand how it works?
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
    patchOperationList.Add("path", new object());

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

    List<PatchOperation> values = [
        PatchOperation.Move("from", "to"),
        PatchOperation.Move("here", "there")
    ];


    //Be careful with this primary constructor
    PatchOperationList patchOperationList3 = new(values);
    values.Add(PatchOperation.Move("from", "to")); // this will be added to patchOperationList3
    patchOperationList3.Add(new { Label = "label", Number = 44 }); // this will be added to original list values

    List<PatchOperation> values2 = [
        PatchOperation.Move("from", "to"),
        PatchOperation.Move("here", "there")
    ];
    PatchOperationList patchOperationList4 = new([.. values2]);
    values2.Add(PatchOperation.Move("from", "to"));
    patchOperationList4.Add(new { Label = "label", Number = 44 });

    patchOperationList4 = new([.. values2]);
    values2.Add(PatchOperation.Move("from", "to"));
    patchOperationList4.Add(new { Label = "label", Number = 44 });

    PatchOperationList patchOperationList5 = PatchOperation.Move("/from", "/to");
    PatchOperationList patchOperationList6 = [PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there")];
    PatchOperationList patchOperationList7 = new PatchOperationList { PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there") };
    PatchOperationList patchOperationList8 = new PatchOperationList(new List<PatchOperation> { PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there") });

    patchOperationList.AddIncrement("age", 1);
    patchOperationList.AddIncrement("age", 2);
    patchOperationList.AddIncrement("age", 2);
    patchOperationList.AddIncrement("age", 2);
    patchOperationList.AddIncrement("age", 2);
    patchOperationList.AddIncrement("age", 2);
    patchOperationList.AddMove("test", "test2");
    patchOperationList.AddRemove("test2");
    patchOperationList.AddReplace("age", 55);
    //patchOperationList.AddSet<object>((object)null); //throws ArgumentNullException

    patchOperationList3.Add(PatchOperation.Move("/from", "/to"), PatchOperation.Move("/here", "/there"));
    IList<PatchOperation> patchOperations2 = new PatchOperationList();
    IReadOnlyList<PatchOperation> patchOperations3 = patchOperationList;

    PatchOperationList patchOperationList9 = new PatchOperationListBuilder()
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

    patchOperationList9 = new PatchOperationList()
    {
        values.ToList()
    };

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

            if (patchOperationList.Count <= 10)
            {
                ItemResponse<dynamic> patchResponse = await container.PatchItemAsync<dynamic>(item.id, partitionKey, patchOperationList);

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
            Console.WriteLine($"patchResponse2.Resource is {nameof(JObject)}: {patchResponse2.Resource is JObject}");
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
                                     ex.StatusCode == HttpStatusCode.BadRequest ||
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
}
#endregion  sample codes

# region sample2
{
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
            await foreach (OneOf<Book,Category> result in GetResultsAsync())
            {
                result.Switch(book =>
                {
                    Console.WriteLine($"Book: {book.Title}");
                },
                category =>
                {
                    Console.WriteLine($"Category: {category.Name}");
                });
            }
        }
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound ||
                                     ex.StatusCode == HttpStatusCode.BadRequest ||
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

    //hasn't put books in the container yet
    //for self-entertainment and learning purposes only
    //union type is interesting in this scenario imo
    async IAsyncEnumerable<OneOf<Book,Category>> GetResultsAsync()
    {
        CosmosClient client = new(connectionString);
        Database database = client.GetDatabase(databaseId);
        Container container = database.GetContainer("myTestingContainer");
        var iterator = container.GetItemLinqQueryable<Book>().ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (Book book in await iterator.ReadNextAsync())
            {
                yield return book;
            }
        }

        var iterator2 = container.GetItemLinqQueryable<Book>().Select(b => new Book { Id = b.Id, Title = b.Title }).ToFeedIterator();
        while (iterator2.HasMoreResults)
        {
            foreach (Book book in await iterator2.ReadNextAsync())
            {
                yield return book;
            }
        }

        var iterator3 = container.GetItemLinqQueryable<Book>().SelectMany(b => b.Categories.Where(c => c.Name.StartsWith("Cosmos"))).ToFeedIterator();
        while (iterator3.HasMoreResults)
        {
            foreach (Category category in await iterator3.ReadNextAsync())
            {
                yield return category;
            }
        }
    }
}
#endregion  sample2