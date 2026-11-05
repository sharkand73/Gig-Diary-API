using ApiLambda.DependencyInjection;
using ApiLambda.Models;
using ApiLambda.Repositories;
using Microsoft.Extensions.Logging;

namespace ApiLambda.Services;

public class GigService(
    IGigRepository repository,
    IGigCalendarFactory calendarFactory,
    ILogger<GigService> logger) : IGigService
{
    private IGigCalendarService? _calendarService;

    public async Task<List<Gig>> GetAllGigs() => (await repository.GetAllAsync()).ToList();

    public async Task<Gig?> GetGigById(string id) => await repository.GetByIdAsync(id);

    public async Task<Gig> CreateGig(Gig gig)
    {
        var calendarService = await GetGigCalendarService();
        
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
            logger.LogError(ex, "Error creating calendar event.");
        }

        return createdGig;
    }

    public async Task<Gig?> UpdateGig(string id, Gig gig)
    {
        var calendarService = await GetGigCalendarService();
        
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
            logger.LogError(ex, "Error updating calendar event with body {gig}", gig);
        }

        return updated;
    }

    public async Task<bool> DeleteGig(string id)
    {
        var calendarService = await GetGigCalendarService();
        
        var gig = await repository.GetByIdAsync(id);
        logger.LogInformation("Deleting gig: {gig}", gig);
        if (gig == null) return false;
        var deleted = await repository.DeleteAsync(id);
        if (!deleted) return false;

        var calendarId = gig.CalendarId;
        if (!gig.CalendarSync || calendarId == null) return true;
        logger.LogInformation("CalendarId: {calendarId}", calendarId);
        try
        {
            logger.LogInformation("Deleting calendar event {calendarId}", calendarId);
            await calendarService.DeleteEvent(calendarId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting calendar event.");
        }

        return true;
    }

    public async Task<Mappings> GetMappings()
    {
        var allGigs = (await repository.GetAllAsync()).ToList();
        var postCodes = allGigs
            .Where(g => !string.IsNullOrEmpty(g.Venue) && !string.IsNullOrEmpty(g.Postcode))
            .GroupBy(g => g.Venue.Trim())
            .ToDictionary(
                group => group.Key,
                group => group.First().Postcode
            );

        var contacts = allGigs
            .Where(g => !string.IsNullOrEmpty(g.Act) && !string.IsNullOrEmpty(g.Contact))
            .GroupBy(g => g.Act.Trim())
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(g => g.LeaveDate)
                    .First()
                    .Contact
            );

        var instruments = allGigs
            .Where(g => !string.IsNullOrEmpty(g.Act))
            .GroupBy(g => g.Act.Trim())
            .ToDictionary(
                group => group.Key,
                group => group.First().Instrument
            );

        return new Mappings(postCodes, contacts, instruments);
    }

    private async Task<IGigCalendarService> GetGigCalendarService()
    {
        _calendarService ??= await calendarFactory.CreateGigCalendarAsync();
        return _calendarService;
    }
}