using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using ApiLambda.Api;

namespace ApiLambda.Repositories;

public class GigRepository(IDynamoDBContext dynamoDbContext, ILambdaLogger logger) : IGigRepository
{
    public async Task<Gig?> GetByIdAsync(string id)
    {
        return await dynamoDbContext.LoadAsync<Gig>(id);
    }

    public async Task<IEnumerable<Gig>> GetAllAsync()
    {
        var scan = dynamoDbContext.ScanAsync<Gig>(new List<ScanCondition>());
        var gigs = await scan.GetRemainingAsync();
        return gigs.OrderByDescending(g => g.LeaveDate);
    }

    public async Task<Gig> CreateAsync(Gig gig)
    {
        gig.Id = Guid.NewGuid().ToString();
        await dynamoDbContext.SaveAsync(gig);
        return gig;
    }

    public async Task<Gig?> UpdateAsync(string id, Gig gig)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;
        
        gig.Id = id;
        await dynamoDbContext.SaveAsync(gig);
        return gig;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return false;
        
        await dynamoDbContext.DeleteAsync<Gig>(id);
        return true;
    }
}