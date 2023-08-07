using System.Net.Http.Json;

namespace HachijouBot.KancolleNews
{
    public class EoUpdateService
    {
        /// <summary>
        /// Update cache per commit
        /// </summary>
        public Dictionary<string, List<EoUpdateModel>> EoUpdateCache { get; set; } = new();

        private string FetchUrl =>
            "ElectronicObserverEN/Data/{0}/Data/Updates.json";

        private HttpClient HttpClient { get; }
        
        public EoUpdateService()
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = new("https://raw.githubusercontent.com"),
            };
        }

        public async Task<List<EoUpdateModel>> GetUpdates(string commitId)
        {
            if (EoUpdateCache.TryGetValue(commitId, out List<EoUpdateModel>? updatesFound)) return updatesFound;

            List<EoUpdateModel>? updates = await HttpClient.GetFromJsonAsync<List<EoUpdateModel>?>(string.Format(FetchUrl, commitId));

            return updates ?? new List<EoUpdateModel>();
        }
    }
}
