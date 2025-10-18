using ApiLambda.Api;

namespace ApiLambda.Services;

public interface IGigCalendarService
{
    Task<string> CreatEvent(Gig gig);
    Task UpdateEvent(Gig gig);
    Task DeleteEvent(string? calendarId);
}