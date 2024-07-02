using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace HachijouBot.Common;

public class ElectronicObserverApiService
{
    private HttpClient Client { get; }

    private string Url { get; }

    private string Secret { get; }

    public ElectronicObserverApiService(string url, string secret)
    {
        Client = new HttpClient();

        Url = url;
        Secret = secret;

        Initialize();
    }

    public void Initialize()
    {
        Client.BaseAddress = new Uri(Url);

        var authenticationString = $"{Secret}:";
        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
    }

    public async Task<T?> GetJson<T>(string url)
    {
        return await Client.GetFromJsonAsync<T>(url).ConfigureAwait(false);
    }
}