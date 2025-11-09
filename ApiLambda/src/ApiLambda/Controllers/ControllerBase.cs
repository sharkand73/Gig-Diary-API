using Amazon.Lambda.APIGatewayEvents;

namespace ApiLambda.Controllers;

public class ControllerBase
{
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