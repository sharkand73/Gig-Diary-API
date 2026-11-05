using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace ApiLambda.DependencyInjection;

public class SecretStore : ISecretStore
{
    public async Task<string> GetSecretAsync(string secretName)
    {
        var request = new GetParameterRequest()
        {
            Name = secretName,
            WithDecryption = true
        };
        using var client = new AmazonSimpleSystemsManagementClient();
        var response = await client.GetParameterAsync(request);
        return response.Parameter.Value;
    }
}