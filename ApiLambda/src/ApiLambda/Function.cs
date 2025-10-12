using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ApiLambda.Repositories;
using System.Text.Json;
using ApiLambda.Api;

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
    var repository = new GigRepository(dynamoDbContext, logger);
    

    
    try
    {
        // Check if path starts with /gigs
        if (!request.Path.StartsWith("/gigs"))
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "Not found" };
        }

        return request.HttpMethod.ToUpper() switch
        {
            "GET" when request.PathParameters?.ContainsKey("id") == true =>
                await GetGig(repository, request.PathParameters["id"]),
            "GET" => await GetAllGigs(repository),
            "POST" => await CreateGig(repository, request.Body),
            "PUT" when request.PathParameters?.ContainsKey("id") == true =>
                await UpdateGig(repository, request.PathParameters["id"], request.Body),
            "DELETE" when request.PathParameters?.ContainsKey("id") == true =>
                await DeleteGig(repository, request.PathParameters["id"]),
            _ => new APIGatewayProxyResponse { StatusCode = 405, Body = "Method not allowed" }
        };
    }
    catch (Exception ex)
    {
        context.Logger.LogError($"Error: {ex}");
        return new APIGatewayProxyResponse { StatusCode = 500, Body = "Internal server error" };
    }
}

async Task<APIGatewayProxyResponse> GetGig(IGigRepository repository, string id)
{
    var gig = await repository.GetByIdAsync(id);
    return gig == null 
        ? new APIGatewayProxyResponse { StatusCode = 404, Body = "Not found" }
        : new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(gig) };
}

async Task<APIGatewayProxyResponse> GetAllGigs(IGigRepository repository)
{
    var gigs = await repository.GetAllAsync();
    return new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(gigs) };
}

async Task<APIGatewayProxyResponse> CreateGig(IGigRepository repository, string body)
{
    var gig = JsonSerializer.Deserialize<Gig>(body);
    var created = await repository.CreateAsync(gig);
    return new APIGatewayProxyResponse { StatusCode = 201, Body = JsonSerializer.Serialize(created) };
}

async Task<APIGatewayProxyResponse> UpdateGig(IGigRepository repository, string id, string body)
{
    var gig = JsonSerializer.Deserialize<Gig>(body);
    var updated = await repository.UpdateAsync(id, gig);
    return updated == null
        ? new APIGatewayProxyResponse { StatusCode = 404, Body = "Not found" }
        : new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(updated) };
}

async Task<APIGatewayProxyResponse> DeleteGig(IGigRepository repository, string id)
{
    var deleted = await repository.DeleteAsync(id);
    return new APIGatewayProxyResponse { StatusCode = deleted ? 204 : 404 };
}
