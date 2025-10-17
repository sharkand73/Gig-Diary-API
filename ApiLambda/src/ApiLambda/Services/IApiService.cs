using Amazon.Lambda.APIGatewayEvents;
using ApiLambda.Api;

namespace ApiLambda.Services;

public interface IApiService
{
    Task<APIGatewayProxyResponse> GetAllGigsAsync();
    Task<APIGatewayProxyResponse> GetGigAsync(string id);
    Task<APIGatewayProxyResponse> CreateGigAsync(string body);
    Task<APIGatewayProxyResponse> UpdateGigAsync(string id, string body);
    Task<APIGatewayProxyResponse> DeleteGigAsync(string id);
    APIGatewayProxyResponse CreateCorsResponse(int statusCode, string body);
}