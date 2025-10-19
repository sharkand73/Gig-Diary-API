using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApiLambda.Models;
using ApiLambda.Services;

namespace ApiLambda.Controllers;

public class ApiService(IGigService gigService, 
    ILambdaLogger logger) : IApiService
{
    public async Task<APIGatewayProxyResponse> GetGigAsync(string id)
    {
        logger.LogInformation("GET");
        var gig = await gigService.GetGigById(id);
        return gig == null 
            ? CreateCorsResponse(404, "Not found")
            : CreateCorsResponse(200, JsonSerializer.Serialize(gig));
    }
    
    public async Task<APIGatewayProxyResponse> GetAllGigsAsync()
    {
        logger.LogInformation("GET ALL");
        var gigs = await gigService.GetAllGigs();
        return CreateCorsResponse(200, JsonSerializer.Serialize(gigs));
    }
    
    public async Task<APIGatewayProxyResponse> CreateGigAsync(string body)
    {
        logger.LogInformation("CREATE");
        var gig = JsonSerializer.Deserialize<Gig>(body)
            ?? throw new FormatException("Invalid JSON");
        var createdGig = await gigService.CreateGig(gig);
        return CreateCorsResponse(201, JsonSerializer.Serialize(createdGig));;
    }
    
    public async Task<APIGatewayProxyResponse> UpdateGigAsync(string id, string body)
    {
        logger.LogInformation("UPDATE");
        var gig = JsonSerializer.Deserialize<Gig>(body)
            ?? throw new FormatException("Invalid JSON");
        var updated = await gigService.UpdateGig(id, gig);
        return updated == null 
            ? CreateCorsResponse(404, "Not found") 
            : CreateCorsResponse(200, JsonSerializer.Serialize(updated));
    }
    
    public async Task<APIGatewayProxyResponse> DeleteGigAsync(string id)
    {
        logger.LogInformation("DELETE");
        var deleted = await gigService.DeleteGig(id);
        return !deleted 
            ? CreateCorsResponse(404, "Not found") 
            : CreateCorsResponse(204, "");
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