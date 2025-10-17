using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using ApiLambda.Api;
using ApiLambda.Repositories;

namespace ApiLambda.Services;

public class ApiService(IGigRepository repository) : IApiService
{
    public async Task<APIGatewayProxyResponse> GetGigAsync(string id)
    {
        var gig = await repository.GetByIdAsync(id);
        return gig == null 
            ? CreateCorsResponse(404, "Not found")
            : CreateCorsResponse(200, JsonSerializer.Serialize(gig));
    }
    
    public async Task<APIGatewayProxyResponse> GetAllGigsAsync()
    {
        var gigs = await repository.GetAllAsync();
        return CreateCorsResponse(200, JsonSerializer.Serialize(gigs));
    }
    
    public async Task<APIGatewayProxyResponse> CreateGigAsync(string body)
    {
        var gig = JsonSerializer.Deserialize<Gig>(body)
            ?? throw new FormatException("Invalid JSON");
        var created = await repository.CreateAsync(gig);
        return CreateCorsResponse(201, JsonSerializer.Serialize(created));
    }
    
    public async Task<APIGatewayProxyResponse> UpdateGigAsync(string id, string body)
    {
        var gig = JsonSerializer.Deserialize<Gig>(body)
            ?? throw new FormatException("Invalid JSON");
        var updated = await repository.UpdateAsync(id, gig);
        return updated == null
            ? CreateCorsResponse(404, "Not found")
            : CreateCorsResponse(200, JsonSerializer.Serialize(updated));
    }
    
    public async Task<APIGatewayProxyResponse> DeleteGigAsync(string id)
    {
        var deleted = await repository.DeleteAsync(id);
        return CreateCorsResponse(deleted ? 204 : 404, "");
    }
    
    public APIGatewayProxyResponse CreateCorsResponse(int statusCode, string body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = body,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" },
                { "Content-Type", "application/json" }
            }
        };
    }
}