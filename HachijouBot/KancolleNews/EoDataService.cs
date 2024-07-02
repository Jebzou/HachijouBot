using System.Text.Json;
using HachijouBot.Models;

namespace HachijouBot.KancolleNews;

public class EoDataService
{
    public List<ShipModel> ShipCache { get; set; } = new();
    public List<EquipmentModel> EquipmentCache { get; set; } = new();

    private string FetchUrl => "https://raw.githubusercontent.com/ElectronicObserverEN/Data/master/Data/{0}.json";

    private HttpClient HttpClient { get; }
    
    public EoDataService()
    {
        HttpClient = new HttpClient();
    }

    public async Task<ShipModel?> GetShip(int apiId)
    {
        ShipModel? ship = ShipCache.Find(ship => ship.ApiId == apiId);

        if (ship is not null) return ship;

        await LoadShips();

        return ShipCache.Find(s => s.ApiId == apiId);
    }

    public async Task<EquipmentModel?> GetEquipment(int apiId)
    {
        EquipmentModel? equipment = EquipmentCache.Find(e => e.ApiId == apiId);

        if (equipment is not null) return equipment;

        await LoadEquipments();

        return EquipmentCache.Find(e => e.ApiId == apiId);
    }

    private async Task LoadEquipments()
    {

        string content = await HttpClient.GetStringAsync(string.Format(FetchUrl, "Equipments"));

        try
        {
            EquipmentCache = JsonSerializer.Deserialize<List<EquipmentModel>>(content) ?? new();
        }
        catch
        {
            Console.WriteLine(content);
            throw;
        }
    }

    private async Task LoadShips()
    {

        string content = await HttpClient.GetStringAsync(string.Format(FetchUrl, "Ships"));

        try
        {
           ShipCache = JsonSerializer.Deserialize<List<ShipModel>>(content) ?? new();
        }
        catch
        {
            Console.WriteLine(content);
            throw;
        }
    }
}