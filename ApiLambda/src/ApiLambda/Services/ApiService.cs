using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApiLambda.Api;
using ApiLambda.Repositories;

namespace ApiLambda.Services;

public class ApiService(IGigRepository repository, 
    IGigCalendarService calendarService, ILambdaLogger logger) : IApiService
{
    public async Task<APIGatewayProxyResponse> GetGigAsync(string id)
    {
        logger.LogInformation("GET");
        var gig = await repository.GetByIdAsync(id);
        return gig == null 
            ? CreateCorsResponse(404, "Not found")
            : CreateCorsResponse(200, JsonSerializer.Serialize(gig));
    }
    
    public async Task<APIGatewayProxyResponse> GetAllGigsAsync()
    {
        logger.LogInformation("GET ALL");
        var gigs = await repository.GetAllAsync();
        return CreateCorsResponse(200, JsonSerializer.Serialize(gigs));
    }
    
    public async Task<APIGatewayProxyResponse> CreateGigAsync(string body)
    {
        logger.LogInformation("CREATE");
        var gig = JsonSerializer.Deserialize<Gig>(body)
            ?? throw new FormatException("Invalid JSON");
        var createdGig = await repository.CreateAsync(gig);
        if (!gig.CalendarSync) return CreateCorsResponse(201, JsonSerializer.Serialize(createdGig));;
        try
        {
            logger.LogInformation("Creating new Gig in calendar");
            createdGig.CalendarId = await calendarService.CreatEvent(gig);
            await repository.UpdateAsync(createdGig.Id!, createdGig);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error creating calendar event: {ex}");
        }
        return CreateCorsResponse(201, JsonSerializer.Serialize(createdGig));;
    }
    
    public async Task<APIGatewayProxyResponse> UpdateGigAsync(string id, string body)
    {
        logger.LogInformation("UPDATE");
        var gig = JsonSerializer.Deserialize<Gig>(body)
            ?? throw new FormatException("Invalid JSON");
        var updated = await repository.UpdateAsync(id, gig);
        if (updated == null) return CreateCorsResponse(404, "Not found");
        if (!gig.CalendarSync) return CreateCorsResponse(200, JsonSerializer.Serialize(updated));
        try
        {
            logger.LogInformation("Updating Gig in calendar");
            if (gig.CalendarId == null)
            {
                gig.CalendarId = await calendarService.CreatEvent(gig);
                updated = await repository.UpdateAsync(id, gig);
                return CreateCorsResponse(200, JsonSerializer.Serialize(updated));
            }
            await calendarService.UpdateEvent(gig);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error updating calendar event with body {body}: {ex}");
        }
        return CreateCorsResponse(200, JsonSerializer.Serialize(updated));
    }
    
    public async Task<APIGatewayProxyResponse> DeleteGigAsync(string id)
    {
        logger.LogInformation("DELETE");
        var gig = await repository.GetByIdAsync(id);
        logger.LogInformation($"Gig: {gig}");

        var calendarId = gig?.CalendarId;

        logger.LogInformation($"CalendarId: {calendarId}");
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogInformation($"Cannot delete Gig {id} in calendar.  Nothing to delete.");
            return CreateCorsResponse(404, "Not found");
        }
        if (calendarId == null) return CreateCorsResponse(204, "");
        try
        {
            logger.LogInformation($"Deleting calendar event {calendarId}");
            await calendarService.DeleteEvent(calendarId);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error deleting calendar event: {ex}");
        }
        return CreateCorsResponse(204, "");
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