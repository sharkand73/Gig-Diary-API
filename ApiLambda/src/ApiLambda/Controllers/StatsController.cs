using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApiLambda.Services;

namespace ApiLambda.Controllers;

public class StatsController(IGigStatsService statsService, 
    ILambdaLogger logger) : ControllerBase, IStatsController
{
    public async Task<APIGatewayProxyResponse> GetStatsAsync()
    {
        logger.LogInformation("GET");
        var stats = await statsService.GetGigStats();
        return CreateCorsResponse(200, JsonSerializer.Serialize(stats));
    }
}