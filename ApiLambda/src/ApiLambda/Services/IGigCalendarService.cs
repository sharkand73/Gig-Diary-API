using ApiLambda.Api;

namespace ApiLambda.Services;

public interface ICalendarService
{
    Task CreatEvent(Gig gig);
    Task UpdateEvent(Gig gig);
    Task DeleteEvent(Gig gig);
}