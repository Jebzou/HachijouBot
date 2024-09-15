using System;
using System.Text;
using Discord;
using HachijouBot.Common;
using HachijouBot.KancolleNews;
using HachijouBot.Models;

namespace HachijouBot.ElectronicObserverReport;

public class ElectronicObserverReportService
{
    private ElectronicObserverApiService ElectronicObserverApiService { get; set; }

    private int UpgradeIssueLastId { get; set; }
    private int UpgradeCostIssueLastId { get; set; }

    private string ReportChannelId { get; }

    private EoDataService EoDataService { get; }

    public ElectronicObserverReportService(ElectronicObserverApiService api, string reportChannelId, EoDataService dataService)
    {
        ElectronicObserverApiService = api;
        ReportChannelId = reportChannelId;
        EoDataService = dataService;

        CheckIssues().Wait(30000);

        // Every 10 minutes
        System.Timers.Timer timer = new System.Timers.Timer(600000);

        timer.Elapsed += TimerTick;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private async void TimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await CheckIssues();
    }
    
    private async Task CheckIssues()
    {
        try
        {
            if (UpgradeIssueLastId == 0)
            {
                string url = "EquipmentUpgradeIssues/latest";
                List<EquipmentUpgradeIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeIssueModel>>(url);

                if (issues is { Count: > 0 } issuesNotNull)
                {
                    UpgradeIssueLastId = issuesNotNull[0].Id;
                }
            }

            /*if (UpgradeCostIssueLastId == 0)
            {
                string url = "EquipmentUpgradeCostIssues/latest";
                List<EquipmentUpgradeCostIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeCostIssueModel>>(url);

                if (issues is { Count: > 0 } issuesNotNull)
                {
                    UpgradeCostIssueLastId = issuesNotNull[0].Id;
                }
            }*/

            await CheckUpgradeIssues();
            await CheckUpgradeCostIssues();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
        }
    }

    private async Task CheckUpgradeIssues()
    {
        string url = $"EquipmentUpgradeIssues?startId={UpgradeIssueLastId}";
        List<EquipmentUpgradeIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeIssueModel>>(url);
        
        if (issues is null) return;
        if (!issues.Any()) return;

        StringBuilder message = new();
        message.AppendLine("Detected upgrade issues : ");

        foreach (EquipmentUpgradeIssueModel issue in issues)
        {
            await ParseIssues(issue, message);
        }

        await PostMessage(message.ToString());

        UpgradeIssueLastId = issues.Max(issue => issue.Id);
    }

    private async Task CheckUpgradeCostIssues()
    {
        string url = $"EquipmentUpgradeCostIssues?startId={UpgradeCostIssueLastId}";
        List<EquipmentUpgradeCostIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeCostIssueModel>>(url);

        if (issues is null) return;
        if (!issues.Any()) return;

        StringBuilder message = new();
        message.AppendLine("Detected upgrade cost issues : ");

        foreach (EquipmentUpgradeCostIssueModel issue in issues)
        {
            await ParseIssues(issue, message);
        }

        await PostMessage(message.ToString());

        UpgradeCostIssueLastId = issues.Max(issue => issue.Id);
    }

    private async Task ParseIssues(EquipmentUpgradeCostIssueModel issue, StringBuilder message)
    {
        ShipModel shipModel = await EoDataService.GetShip(issue.HelperId) ?? new();
        EquipmentModel eq = await EoDataService.GetEquipment(issue.EquipmentId) ?? new();

        string upgradeStage = issue.UpgradeStage switch
        {
            UpgradeStage.From0To5 => "0\uff5e5",
            UpgradeStage.From6To9 => "6\uff5e9",
            _ => issue.UpgradeStage.ToString(),
        };

        message.AppendLine($"## {eq.NameEN} with {shipModel.NameEN} ({upgradeStage})");
        message.AppendLine($"- {issue.Actual.Fuel} {EmoteDataBase.Emotes["kcfuel"]} {issue.Actual.Ammo} {EmoteDataBase.Emotes["kcammo"]} {issue.Actual.Steel} {EmoteDataBase.Emotes["kcsteel"]} {issue.Actual.Bauxite} {EmoteDataBase.Emotes["kcbauxite"]}");

        foreach (EquipmentUpgradeCostItemModel consumedEquipmentReq in issue.Actual.EquipmentDetail)
        {
            EquipmentModel requiredEquipment = await EoDataService.GetEquipment(consumedEquipmentReq.Id) ?? new();

            message.AppendLine($"- {requiredEquipment.NameEN} x{consumedEquipmentReq.Count}");
        }

        foreach (EquipmentUpgradeCostItemModel consumedItemId in issue.Actual.ConsumableDetail)
        {
            UseItemId useItem = (UseItemId)consumedItemId.Id;
            message.AppendLine($"- {useItem} x{consumedItemId.Count}");
        }
    }

    private async Task ParseIssues(EquipmentUpgradeIssueModel issue, StringBuilder message)
    {
        foreach (int actualEquipmentId in issue.ActualUpgrades)
        {
            if (!issue.ExpectedUpgrades.Contains(actualEquipmentId) && !IsBaseUpgradeEquipment(actualEquipmentId))
            {
                ShipModel shipModel = await EoDataService.GetShip(issue.HelperId) ?? new();
                EquipmentModel eq = await EoDataService.GetEquipment(actualEquipmentId) ?? new();

                message.AppendLine($"{shipModel.NameEN} ({shipModel.ApiId}) is missing an upgrade : {eq.NameEN} ({Enum.GetName(issue.Day)})");
            }
        }

        foreach (int expectedEquipmentId in issue.ExpectedUpgrades)
        {
            if (!issue.ActualUpgrades.Contains(expectedEquipmentId))
            {
                ShipModel shipModel = await EoDataService.GetShip(issue.HelperId) ?? new();
                EquipmentModel eq = await EoDataService.GetEquipment(expectedEquipmentId) ?? new();

                message.AppendLine($"{shipModel.NameEN} ({shipModel.ApiId}) can't upgrade : {eq.NameEN} ({Enum.GetName(issue.Day)})");
            }
        }
    }

    private bool IsBaseUpgradeEquipment(int equipmentId) => equipmentId switch
    {
        2 // 12.7cm Twin Gun
            or 4 // 14cm Single Gun
            or 14 // 61cm Quadruple Torpedo
            or 44 // Type 94 Depth Charge Projector
            => true,
        _ => false
    };

    private async Task PostMessage(string message)
    {
        ITextChannel? channel = await Hachijou.GetInstance().Client.GetChannelAsync(ulong.Parse(ReportChannelId)) as ITextChannel;
        if (channel is not null) await channel.SendMessageAsync(message);
    }
}
