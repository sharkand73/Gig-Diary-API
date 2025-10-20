using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using ApiLambda.Controllers;
using ApiLambda.Repositories;
using ApiLambda.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

// Configure services
var dynamoDbContext = new DynamoDBContextBuilder()
    .WithDynamoDBClient(() => new AmazonDynamoDBClient())
    .Build();

await LambdaBootstrapBuilder.Create(async (Stream stream, ILambdaContext context) =>
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var request = await JsonSerializer.DeserializeAsync<APIGatewayProxyRequest>(stream, options)
            ?? throw new ArgumentNullException(nameof(stream));
        return await Handler(request, context);
    }, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();

return;

async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
{
    var logger = context.Logger;
    var repository = new GigRepository(dynamoDbContext);
    var calendarService = new GigCalendarService(await GetGoogleClient());
    var gigService = new GigService(repository, calendarService, logger);
    var gigController = new GigController(gigService, logger);
    
    try
    {
        if (request.Path.StartsWith("/mappings"))
        {
            return await gigController.GetMappingsAsync();
        }
        if (!request.Path.StartsWith("/gigs"))
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "Not found" };
        }
        
        // Extract ID from path manually
        var pathParts = request.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var hasId = pathParts.Length > 1;
        var id = hasId ? pathParts[1] : null;

        return request.HttpMethod.ToUpper() switch
        {
            "OPTIONS" => gigController.CreateCorsResponse(200, ""),
            "GET" when hasId => await gigController.GetGigAsync(id!),
            "GET" => await gigController.GetAllGigsAsync(),
            "POST" => await gigController.CreateGigAsync(request.Body),
            "PUT" when hasId => await gigController.UpdateGigAsync(id!, request.Body),
            "DELETE" when hasId => await gigController.DeleteGigAsync(id!),
            _ => gigController.CreateCorsResponse(405, "Method not allowed")
        };
    }
    catch (Exception ex)
    {
        logger.LogError($"Error: {ex}");
        return gigController.CreateCorsResponse(500, "Internal server error");
    }
}

async Task<CalendarService> GetGoogleClient()
{
    var credentialJson = await GetCredentialJson();
    var credential = GoogleCredential
        .FromJson(credentialJson)
        .CreateScoped(CalendarService.Scope.Calendar);

    return new CalendarService(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential
    });
}

async Task<string> GetCredentialJson()
{
    var request = new GetParameterRequest()
    {
        Name = "GigDiaryGoogleConfig",
        WithDecryption = true
    };
    using var client = new AmazonSimpleSystemsManagementClient();
    var response = await client.GetParameterAsync(request);
    return response.Parameter.Value
        ?? throw new FormatException("GigDiaryGoogleConfig was null");
}

