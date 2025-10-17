using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ApiLambda.Repositories;
using System.Text.Json;
using ApiLambda.Api;
using ApiLambda.Services;

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
    var apiService = new ApiService(repository);
    
    try
    {
        // Check if path starts with /gigs
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
            "OPTIONS" => apiService.CreateCorsResponse(200, ""),
            "GET" when hasId => await apiService.GetGigAsync(id!),
            "GET" => await apiService.GetAllGigsAsync(),
            "POST" => await apiService.CreateGigAsync(request.Body),
            "PUT" when hasId => await apiService.UpdateGigAsync(id!, request.Body),
            "DELETE" when hasId => await apiService.DeleteGigAsync(id!),
            _ => apiService.CreateCorsResponse(405, "Method not allowed")
        };
    }
    catch (Exception ex)
    {
        logger.LogError($"Error: {ex}");
        return apiService.CreateCorsResponse(500, "Internal server error");
    }
}

