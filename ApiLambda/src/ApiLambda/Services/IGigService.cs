using ApiLambda.Api;

namespace ApiLambda.Services;

public interface IGigService
{
    Task<List<Gig>> GetAllGigs();
    Task<Gig?> GetGigById(string id);
    Task<Gig> CreateGig(Gig gig);
    Task<Gig?> UpdateGig(string id, Gig gig);
    Task<bool> DeleteGig(string id);
}