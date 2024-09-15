using System.Text.Json.Serialization;

namespace HachijouBot.ElectronicObserverReport;

public class EquipmentUpgradeCostIssueModel
{
    [JsonPropertyName("software_version")] public string SoftwareVersion { get; set; } = "";

    [JsonPropertyName("data_version")] public int DataVersion { get; set; }
    
    [JsonPropertyName("expected")] public EquipmentUpgradeCostModel Expected { get; set; } = new();

    [JsonPropertyName("actual")] public EquipmentUpgradeCostModel Actual { get; set; } = new();

    [JsonPropertyName("helperId")] public int HelperId { get; set; }

    [JsonPropertyName("equipmentId")] public int EquipmentId { get; set; }

    [JsonPropertyName("upgradeStage")] public UpgradeStage UpgradeStage { get; set; }

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonIgnore] public DateTime AddedOn { get; set; }


    // override object.Equals
    public override bool Equals(object? obj)
    {
        //       
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237  
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //

        if (obj is not EquipmentUpgradeCostIssueModel other)
        {
            return false;
        }

        if (other.DataVersion != DataVersion) return false;
        if (other.HelperId != HelperId) return false;
        if (other.EquipmentId != EquipmentId) return false;
        if (other.UpgradeStage != UpgradeStage) return false;

        return true;
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(DataVersion);
        hashCode.Add(HelperId);
        hashCode.Add(EquipmentId);
        hashCode.Add(UpgradeStage);

        return hashCode.ToHashCode();
    }
}