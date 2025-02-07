using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace HachijouBot.ElectronicObserverReport
{
    public class GetDataVersionService
    {
        private HttpClient Client { get; }

        private DateTime? LastCheckDate { get; set; }

        private int LatestDataVersion { get; set; } = 0;

        private string Url =>
            "https://raw.githubusercontent.com/ElectronicObserverEN/Data/refs/heads/master/update.json";

        public GetDataVersionService()
        {
            Client = new HttpClient();
        }

        public async Task<int> GetDataVersionOfTheDay()
        {
            if (DateTime.Today == LastCheckDate) return LatestDataVersion;

            JsonObject? updateValues = await Client.GetFromJsonAsync<JsonObject>(Url);

            if (updateValues is null) return 0;
            if (!updateValues.ContainsKey("EquipmentUpgrades")) return 0;
            if (updateValues["EquipmentUpgrades"] is not {} upgradeNode) return 0;

            LatestDataVersion = upgradeNode.GetValue<int>();
            LastCheckDate = DateTime.Today;

            Console.WriteLine("Upgrade data version updated to " + LatestDataVersion);

            return LatestDataVersion;
        }
    }
}
