using System.Text.Json.Serialization;

namespace HachijouBot.ElectronicObserverReport;

public class EquipmentUpgradeCostRange(EquipmentUpgradeCostIssueModel baseDetail)
{
    public int HelperId => baseDetail.HelperId;

    public int EquipmentId => baseDetail.EquipmentId;

    public int Ammo => baseDetail.Actual.Ammo;
    public int Steel => baseDetail.Actual.Steel;
    public int Bauxite => baseDetail.Actual.Bauxite;
    public int Fuel => baseDetail.Actual.Fuel;

    public UpgradeLevel StartLevel { get; set; } = baseDetail.UpgradeLevel;
    public UpgradeLevel EndLevel { get; set; } = baseDetail.UpgradeLevel;

    public int DevmatCost => baseDetail.Actual.DevmatCost;

    public int SliderDevmatCost => baseDetail.Actual.SliderDevmatCost;

    public int ImproveMatCost => baseDetail.Actual.ImproveMatCost;

    public int SliderImproveMatCost => baseDetail.Actual.SliderImproveMatCost;

    public List<EquipmentUpgradeCostItemModel> EquipmentDetail => baseDetail.Actual.EquipmentDetail;

    public List<EquipmentUpgradeCostItemModel> ConsumableDetail => baseDetail.Actual.ConsumableDetail;
}