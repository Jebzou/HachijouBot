using BooruDex.Booru.Client;
using System.Text.Json;

namespace HachijouBot.BooruManager
{
    public class HachijouDanbooru : DanbooruDonmai
    {
        public static string? ApiKey { get; set; }
        public static string? Login { get; set; }

        public HachijouDanbooru() : base(new HttpClient())
        {
            HttpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("HachijouBot", "v1.0"));
        }

        protected override string CreateBaseApiUrl(string query, bool json = true)
        {
            string baseAPIUrl = base.CreateBaseApiUrl(query, json);

            if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Login))
            {
                baseAPIUrl += $"api_key={ApiKey}&login={Login}&";
            }

            return baseAPIUrl;
        }
    }
}
