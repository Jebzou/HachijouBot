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

    private int LastId { get; set; }

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
            if (LastId == 0)
            {
                string url = $"EquipmentUpgradeIssues/latest";
                List<EquipmentUpgradeIssueModel>? issues = await ElectronicObserverApiService.GetJson<List<EquipmentUpgradeIssueModel>>(url);

                if (issues is { Count: > 0 } issuesNotNull)
                {
                    LastId = issuesNotNull[0].Id;
                }
            }

            await CheckUpgradeIssues();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
        }
    }

    private async Task CheckUpgradeIssues()
    {
        string url = $"EquipmentUpgradeIssues?startId={LastId}";
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

        LastId = issues.Max(issue => issue.Id);
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
