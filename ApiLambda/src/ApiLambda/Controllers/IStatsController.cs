using Amazon.Lambda.APIGatewayEvents;

namespace ApiLambda.Controllers;

public interface IStatsController
{
    Task<APIGatewayProxyResponse> GetStatsAsync();
}