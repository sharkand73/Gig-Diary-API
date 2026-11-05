using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using ApiLambda.Controllers;
using ApiLambda.DependencyInjection;
using ApiLambda.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


// Configure services

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddLambdaLogger(Logging.Options));
services.AddApiLambda();
var sp = services.BuildServiceProvider();
var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("ApiLambda");
var gigController = sp.GetRequiredService<GigController>();
var statsController = sp.GetRequiredService<StatsController>();


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
    var path = request.Path.ToLower();
    var method = request.HttpMethod.ToUpper();
    try
    {
        if (path.StartsWith("/mappings"))
        {
            return await gigController.GetMappingsAsync();
        }
        if (path.StartsWith("/stats") && method == "GET")
        {
            var pathVariable = path.Split('/').Last();
            pathVariable = pathVariable == "stats" ? null : pathVariable;
            return await statsController.GetStatsAsync(pathVariable);
        }
        if (!path.StartsWith("/gigs"))
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "Not found" };
        }
        
        // Extract ID from path manually
        var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var hasId = pathParts.Length > 1;
        var id = hasId ? pathParts[1] : null;

        return method switch
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
        logger.LogError(ex, "Error.");
        return gigController.CreateCorsResponse(500, "Internal server error");
    }
}

