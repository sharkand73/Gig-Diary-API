using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ApiLambda.Models;

namespace ApiLambda.Repositories;

public class GigRepository(IDynamoDBContext dynamoDbContext) : IGigRepository
{
    public async Task<Gig?> GetByIdAsync(string id)
    {
        return await dynamoDbContext.LoadAsync<Gig>(id);
    }

    public async Task<IEnumerable<Gig>> GetAllAsync()
    {
        var scan = dynamoDbContext.ScanAsync<Gig>(new List<ScanCondition>());
        var gigs = await scan.GetRemainingAsync();
        var orderedGigs = gigs.OrderByDescending(g => g.LeaveDate).ToList();
        var nextGig = orderedGigs.LastOrDefault(g => g.LeaveDate > DateTime.Now);
        if (nextGig != null)
        {
            nextGig.IsNextGig = true;
        }
        return orderedGigs;
    }
    
    public async Task<List<Gig>> GetRange(DateOnly startDate, DateOnly endDate)
    {
        var startDateString = startDate.ToString("yyyy-MM-dd");
        var endDateString = endDate.ToString("yyyy-MM-dd");

        var filter = new ScanFilter();
        filter.AddCondition("LeaveDate", ScanOperator.Between, startDateString, endDateString);

        return await dynamoDbContext.FromScanAsync<Gig>(new ScanOperationConfig
        {
            Filter = filter,
            IndexName = "LeaveDate-Index"
        }).GetRemainingAsync();
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