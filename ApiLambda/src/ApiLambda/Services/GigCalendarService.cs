using ApiLambda.Models;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace ApiLambda.Services;

public class GigCalendarService(CalendarService client) : IGigCalendarService
{
    private const string CalendarName = Constants.CalendarName;
    
    public async Task<string> CreatEvent(Gig gig)
    {
        // Calculate minutes from event start to 9pm the night before
        var nightBefore9Pm = gig.LeaveDate.Date.AddDays(-1).AddHours(21);
        var minutesToNightBefore = (int)(gig.LeaveDate - nightBefore9Pm).TotalMinutes;
        
        Console.WriteLine($"Event time: {gig.LeaveDate}, 9pm night before: {nightBefore9Pm}, Minutes: {minutesToNightBefore}");

        var newEvent = new Event()
        {
            Summary = gig.Act,
            Location = gig.Venue,
            Description = gig.Description,
            Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.LeaveDate, TimeSpan.Zero) },
            End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.ReturnDate, TimeSpan.Zero) },
            Reminders = new Event.RemindersData
            {
                UseDefault = false,
                Overrides = new List<EventReminder>
                {
                    new EventReminder { Method = "email", Minutes = 60 }
                }
            }
        };
        var returnEvent = await client.Events.Insert(newEvent, CalendarName).ExecuteAsync();
        return returnEvent?.Id
            ?? throw new NotSupportedException("Google API returned null");
    }

    public async Task UpdateEvent(Gig gig)
    {
        var nightBefore9Pm = gig.LeaveDate.Date.AddDays(-1).AddHours(21);
        var minutesToNightBefore = (int)(gig.LeaveDate - nightBefore9Pm).TotalMinutes;

        var newEvent = new Event()
        {
            Summary = gig.Act,
            Location = gig.Venue,
            Description = gig.Description,
            Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.LeaveDate, TimeSpan.Zero) },
            End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(gig.ReturnDate, TimeSpan.Zero) },
            Reminders = new Event.RemindersData
            {
                UseDefault = false,
                Overrides = new List<EventReminder>
                {
                    new EventReminder { Method = "popup", Minutes = 60 },
                    new EventReminder { Method = "popup", Minutes = minutesToNightBefore }
                }
            }
        };
        await client.Events.Update(newEvent, CalendarName, gig.CalendarId).ExecuteAsync();
    }

    public async Task DeleteEvent(string? calendarId) => await client.Events.Delete(CalendarName, calendarId).ExecuteAsync();
}