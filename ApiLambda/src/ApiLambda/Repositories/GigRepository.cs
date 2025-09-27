using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using ApiLambda.Api;

namespace ApiLambda.Repositories;

public class GigRepository(IDynamoDBContext dynamoDbContext, ILambdaLogger logger) : IGigRepository
{
    public Task<Gig?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Gig>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Gig> CreateAsync(Gig gig)
    {
        throw new NotImplementedException();
    }

    public Task<Gig?> UpdateAsync(string id, Gig gig)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }
}