using ApiLambda.Api;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace ApiLambda.Services;

public class GigCalendarService(CalendarService client) : IGigCalendarService
{
    private const string CalendarName = Constants.CalendarName;
    public async Task<string> CreatEvent(Gig gig)
    {
        var newEvent = new Event()
        {
            Summary = gig.Act,
            Location = gig.Venue,
            Description = gig.Description,
            Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.LeaveDate) },
            End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.ReturnDate) }
        };
        var returnEvent = await client.Events.Insert(newEvent, CalendarName).ExecuteAsync();
        return returnEvent?.Id
            ?? throw new NotSupportedException("Google API returned null");
    }

    public async Task UpdateEvent(Gig gig)
    {
        var newEvent = new Event()
        {
            Summary = gig.Act,
            Location = gig.Venue,
            Description = gig.Description,
            Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.LeaveDate) },
            End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.ReturnDate) }
        };
        await client.Events.Update(newEvent, CalendarName, gig.CalendarId).ExecuteAsync();
    }

    public async Task DeleteEvent(string? calendarId) => await client.Events.Delete(CalendarName, calendarId).ExecuteAsync();
}