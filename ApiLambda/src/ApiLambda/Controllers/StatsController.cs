using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using ApiLambda.Services;
using Microsoft.Extensions.Logging;

namespace ApiLambda.Controllers;

public class StatsController(IGigStatsService statsService, 
    ILogger<StatsController> logger) : ControllerBase, IStatsController
{
    public async Task<APIGatewayProxyResponse> GetStatsAsync(string? pathVariable)
    {
        logger.LogInformation("GET");
        var pathVariableInt = ConvertPathVariableToInt(pathVariable);
        var stats = await statsService.GetGigStats(pathVariableInt);
        return CreateCorsResponse(200, JsonSerializer.Serialize(stats));
    }

    private static int? ConvertPathVariableToInt(string? pathVariable)
    {
        if (pathVariable == null || !int.TryParse(pathVariable, out var pathVariableInt)
            || pathVariableInt <= 2025) 
        {
            return null;
        }
        return pathVariableInt;
    }
}