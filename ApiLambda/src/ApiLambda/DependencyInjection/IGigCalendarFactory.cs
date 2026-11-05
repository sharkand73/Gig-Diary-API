using ApiLambda.Services;

namespace ApiLambda.DependencyInjection;

public interface IGigCalendarFactory
{
    Task<IGigCalendarService> CreateGigCalendarAsync();
}