using System.Text.Json.Serialization;

namespace HachijouBot.ElectronicObserverReport;

public class EquipmentUpgradeCostModel
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("fuel")]
    public int Fuel { get; set; }

    [JsonPropertyName("ammo")]
    public int Ammo { get; set; }

    [JsonPropertyName("steel")]
    public int Steel { get; set; }

    [JsonPropertyName("baux")]
    public int Bauxite { get; set; }

    /// <summary>
    /// Devmat cost
    /// </summary>
    [JsonPropertyName("devmats")]
    public int DevmatCost { get; set; }

    /// <summary>
    /// Devmat cost if slider is used
    /// </summary>
    [JsonPropertyName("devmats_sli")]
    public int SliderDevmatCost { get; set; }

    /// <summary>
    /// Screw cost
    /// </summary>
    [JsonPropertyName("screws")]
    public int ImproveMatCost { get; set; }

    /// <summary>
    /// Screw cost if slider is used
    /// </summary>
    [JsonPropertyName("screws_sli")]
    public int SliderImproveMatCost { get; set; }

    [JsonPropertyName("equips")]
    public List<EquipmentUpgradeCostItemModel> EquipmentDetail { get; set; } = new();

    [JsonPropertyName("consumable")]
    public List<EquipmentUpgradeCostItemModel> ConsumableDetail { get; set; } = new();
}
