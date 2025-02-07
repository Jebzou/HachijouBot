using System;
using System.Text;
using System.Text.Json;
using Discord;
using HachijouBot.Common;
using HachijouBot.KancolleNews;
using HachijouBot.Models;
using Microsoft.Extensions.Configuration;

namespace HachijouBot.ElectronicObserverReport;

public class ElectronicObserverReportService
{
    private ElectronicObserverApiService ElectronicObserverApiService { get; set; }

    private ElectronicOberverReportConfigModel? LatestModel { get; set; }

    private string ReportChannelId { get; }

    private EoDataService EoDataService { get; }
    private IConfiguration Config { get; }

    private GetDataVersionService GetDataVersionService { get; }

    public ElectronicObserverReportService(ElectronicObserverApiService api, IConfiguration configuration, EoDataService dataService)
    {
        Config = configuration;

        ElectronicObserverApiService = api;
        ReportChannelId = Config["ReportChannelId"] ?? "";
        EoDataService = dataService;
        GetDataVersionService = new();

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
            if (LatestModel is null)
            {
                if (File.Exists("data/EoReportConfig.json"))
                {
                    LatestModel = JsonSerializer.Deserialize<ElectronicOberverReportConfigModel>(await File.ReadAllTextAsync("data/EoReportConfig.json")) ?? new();
                }
                else
                {
                    LatestModel = new();
                }
            }

            if (LatestModel.UpgradeIssueLastId == 0)
            {

                string url = "EquipmentUpgradeIssues/latest";
                List<EquipmentUpgradeIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeIssueModel>>(url);

                if (issues is { Count: > 0 } issuesNotNull)
                {
                    LatestModel.UpgradeIssueLastId = issuesNotNull[0].Id;
                }
            }

            if (LatestModel.UpgradeCostIssueLastId == 0)
            {
                string url = "EquipmentUpgradeCostIssues/latest";
                List<EquipmentUpgradeCostIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeCostIssueModel>>(url);

                if (issues is { Count: > 0 } issuesNotNull)
                {
                    LatestModel.UpgradeCostIssueLastId = issuesNotNull[0].Id;
                }
            }

            await CheckUpgradeIssues();
            await CheckUpgradeCostIssues();

            await UpdateConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
        }
    }

    private async Task CheckUpgradeIssues()
    {
        if (LatestModel is null) return;

        int dataVersion = await GetDataVersionService.GetDataVersionOfTheDay();

        string url = $"EquipmentUpgradeIssues?startId={LatestModel.UpgradeIssueLastId}&minimumDataVersion={dataVersion}";
        List<EquipmentUpgradeIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeIssueModel>>(url);
        
        if (issues is null) return;
        if (!issues.Any()) return;

        StringBuilder message = new();
        message.AppendLine("Detected upgrade issues : ");

        foreach (EquipmentUpgradeIssueModel issue in issues)
        {
            await ParseIssues(issue, message);

            if (message.Length > 1000)
            {
                await PostMessage(message.ToString());
                message.Clear();
            }
        }

        if (message.Length > 0)
        {
            await PostMessage(message.ToString());
        }

        LatestModel.UpgradeIssueLastId = issues.Max(issue => issue.Id);
    }

    private async Task CheckUpgradeCostIssues()
    {
        if (LatestModel is null) return;

        int dataVersion = await GetDataVersionService.GetDataVersionOfTheDay();
        string url = $"EquipmentUpgradeCostIssues?startId={LatestModel.UpgradeCostIssueLastId}&minimumDataVersion={dataVersion}";
        List<EquipmentUpgradeCostIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeCostIssueModel>>(url);

        if (issues is null) return;
        if (!issues.Any()) return;

        StringBuilder message = new();
        message.AppendLine("Detected upgrade cost issues : ");

        foreach (EquipmentUpgradeCostIssueModel issue in issues)
        {
            await ParseIssues(issue, message);

            if (message.Length > 1000)
            {
                await PostMessage(message.ToString());
                message.Clear();
            }
        }

        if (message.Length > 0)
        {
            await PostMessage(message.ToString());
        }

        LatestModel.UpgradeCostIssueLastId = issues.Max(issue => issue.Id);
    }

    private async Task UpdateConfig()
    {
        await File.WriteAllTextAsync("data/EoReportConfig.json", JsonSerializer.Serialize(LatestModel));
    }

    private async Task ParseIssues(EquipmentUpgradeCostIssueModel issue, StringBuilder message)
    {
        ShipModel? shipModel = await EoDataService.GetShip(issue.HelperId);
        EquipmentModel? eq = await EoDataService.GetEquipment(issue.EquipmentId);

        string upgradeStage = issue.UpgradeStage switch
        {
            UpgradeStage.From0To5 => "0\uff5e5",
            UpgradeStage.From6To9 => "6\uff5e9",
            _ => issue.UpgradeStage.ToString(),
        };

        message.AppendLine($"## {eq?.NameEN ?? $"#{issue.EquipmentId}"} with {shipModel?.NameEN ?? $"#{issue.HelperId}"} ({upgradeStage})");
        message.AppendLine($"- {issue.Actual.Fuel} {EmoteDataBase.Fuel} {issue.Actual.Ammo} {EmoteDataBase.Ammo} {issue.Actual.Steel} {EmoteDataBase.Steel} {issue.Actual.Bauxite} {EmoteDataBase.Bauxite}");
        message.AppendLine($"- {issue.Actual.DevmatCost} {EmoteDataBase.DevMats} (slider: {issue.Actual.SliderDevmatCost} {EmoteDataBase.DevMats}) {issue.Actual.ImproveMatCost} {EmoteDataBase.Screws} (slider: {issue.Actual.SliderImproveMatCost} {EmoteDataBase.Screws})");

        foreach (EquipmentUpgradeCostItemModel consumedEquipmentReq in issue.Actual.EquipmentDetail)
        {
            EquipmentModel? requiredEquipment = await EoDataService.GetEquipment(consumedEquipmentReq.Id);

            message.AppendLine($"- {requiredEquipment?.NameEN ?? $"#{consumedEquipmentReq.Id}"} x{consumedEquipmentReq.Count}");
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
                EquipmentModel? eq = await EoDataService.GetEquipment(actualEquipmentId);

                message.AppendLine($"{shipModel.NameEN} ({issue.HelperId}) is missing an upgrade : {eq?.NameEN ?? $"#{actualEquipmentId}"} ({Enum.GetName(issue.Day)})");
            }
        }

        foreach (int expectedEquipmentId in issue.ExpectedUpgrades)
        {
            if (!issue.ActualUpgrades.Contains(expectedEquipmentId))
            {
                ShipModel shipModel = await EoDataService.GetShip(issue.HelperId) ?? new();
                EquipmentModel? eq = await EoDataService.GetEquipment(expectedEquipmentId);

                message.AppendLine($"{shipModel.NameEN} ({issue.HelperId}) can't upgrade : {eq?.NameEN ?? $"#{expectedEquipmentId}"} ({Enum.GetName(issue.Day)})");
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
