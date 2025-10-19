using ApiLambda.Models;

namespace ApiLambda.Repositories;

public interface IGigRepository
{
    Task<Gig?> GetByIdAsync(string id);
    Task<IEnumerable<Gig>> GetAllAsync();
    Task<Gig> CreateAsync(Gig gig);
    Task<Gig?> UpdateAsync(string id, Gig gig);
    Task<bool> DeleteAsync(string id);
}
