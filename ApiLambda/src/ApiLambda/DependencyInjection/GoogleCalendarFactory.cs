using ApiLambda.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace ApiLambda.DependencyInjection;

public class GoogleCalendarFactory(ISecretStore secretStore) : IGigCalendarFactory
{
    private const string GoogleSecretName = "GigDiaryGoogleConfig";

    public async Task<IGigCalendarService> CreateGigCalendarAsync() => new GigCalendarService(await GetGoogleClient());
    
    private async Task<CalendarService> GetGoogleClient()
    {
        var credentialJson = await secretStore.GetSecretAsync(GoogleSecretName);
        var credential = GoogleCredential
            .FromJson(credentialJson)
            .CreateScoped(CalendarService.Scope.Calendar);

        return new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });
    }
}