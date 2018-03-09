# Client Usage

## 1. Add Service Dependencies

Use the IServiceCollection extension methods to add the client dependencies into your project:

e.g.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json")
                .Build();

    services.AddRecruitStorageClient(config)
}
```  

## 2. Add Configuration

The client library expects a ConnectionString called _**MongoDb**_ which will point to your Mongo/CosmosDb resource. E.g.

In appSettings.json:
```json
{
    "ConnectionStrings": {
        "MongoDb": "<replace with mongo connection string>"
    }
}
```

## 3. Instantiate Client

The client is most easily instantiated by using DI by just passing `IVacancyClient` as a dependency into a class.

```csharp
public class TestClass
{
    private IVacancyClient _client;

    public TestClass(IVacancyClient client)
    {
        _client = client;
    }

    public async Task DoSomething()
    {
        var dashboard = await _client.GetDashboardAsync("TheIdentifier");

        // Other stuff
    }
}
```