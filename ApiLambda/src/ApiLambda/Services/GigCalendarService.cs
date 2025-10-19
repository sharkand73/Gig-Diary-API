using ApiLambda.Models;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace ApiLambda.Services;

public class GigCalendarService(CalendarService client) : IGigCalendarService
{
    private const string CalendarName = Constants.CalendarName;
    private const string Timezone = "Europe/London";
    public async Task<string> CreatEvent(Gig gig)
    {
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var startOffset = londonTimeZone.GetUtcOffset(gig.LeaveDate);
        var endOffset = londonTimeZone.GetUtcOffset(gig.ReturnDate);
        
        var newEvent = new Event()
        {
            Summary = gig.Act,
            Location = gig.Venue,
            Description = gig.Description,
            Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.LeaveDate, startOffset) },
            End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.ReturnDate, endOffset) }
        };
        var returnEvent = await client.Events.Insert(newEvent, CalendarName).ExecuteAsync();
        return returnEvent?.Id
            ?? throw new NotSupportedException("Google API returned null");
    }

    public async Task UpdateEvent(Gig gig)
    {
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var startOffset = londonTimeZone.GetUtcOffset(gig.LeaveDate);
        var endOffset = londonTimeZone.GetUtcOffset(gig.ReturnDate);
        
        var newEvent = new Event()
        {
            Summary = gig.Act,
            Location = gig.Venue,
            Description = gig.Description,
            Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.LeaveDate, startOffset) },
            End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.ReturnDate, endOffset) }
        };
        await client.Events.Update(newEvent, CalendarName, gig.CalendarId).ExecuteAsync();
    }

    public async Task DeleteEvent(string? calendarId) => await client.Events.Delete(CalendarName, calendarId).ExecuteAsync();
}