using Amazon.Lambda.Core;
using ApiLambda.Api;
using ApiLambda.Repositories;

namespace ApiLambda.Services;

public class GigService(IGigRepository repository, 
    IGigCalendarService calendarService, ILambdaLogger logger) : IGigService
{
    public async Task<List<Gig>> GetAllGigs() => (await repository.GetAllAsync()).ToList();

    public async Task<Gig?> GetGigById(string id) => await repository.GetByIdAsync(id);

    public async Task<Gig> CreateGig(Gig gig)
    {
        var createdGig = await repository.CreateAsync(gig);
        if (!gig.CalendarSync) return createdGig;
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
        return createdGig;
    }

    public async Task<Gig?> UpdateGig(string id, Gig gig)
    {
        var updated = await repository.UpdateAsync(id, gig);
        if (!gig.CalendarSync) return updated;
        try
        {
            logger.LogInformation("Updating Gig in calendar");
            if (gig.CalendarId == null)
            {
                gig.CalendarId = await calendarService.CreatEvent(gig);
                updated = await repository.UpdateAsync(id, gig);
                return updated;
            }
            await calendarService.UpdateEvent(gig);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error updating calendar event with body {gig}: {ex}");
        }
        return updated;
    }

    public async Task<bool> DeleteGig(string id)
    {
        var gig = await repository.GetByIdAsync(id);
        logger.LogInformation($"Deleting gig: {gig}");
        if (gig == null) return false;
        var deleted = await repository.DeleteAsync(id);
        if (!deleted) return false;
        
        var calendarId = gig.CalendarId;
        if (!gig.CalendarSync || calendarId == null) return true;
        logger.LogInformation($"CalendarId: {calendarId}");
        try
        {
            logger.LogInformation($"Deleting calendar event {calendarId}");
            await calendarService.DeleteEvent(calendarId);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error deleting calendar event: {ex}");
        }
        return true;
    }
}