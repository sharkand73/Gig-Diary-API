using ApiLambda.Models;

namespace ApiLambda.Services;

public interface IGigStatsService
{
    public Task<GigStats> GetGigStats();
}