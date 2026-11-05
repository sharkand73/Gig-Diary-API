namespace ApiLambda.DependencyInjection;

public interface ISecretStore
{
    Task<string> GetSecretAsync(string secretName);
}