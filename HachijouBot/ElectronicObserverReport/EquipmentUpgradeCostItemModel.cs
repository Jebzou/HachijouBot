using System.Text.Json.Serialization;

namespace HachijouBot.ElectronicObserverReport;

public class EquipmentUpgradeCostItemModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("eq_count")]
    public int Count { get; set; }
}
