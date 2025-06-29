using Discord.WebSocket;
using Discord;
using HachijouBot.Common;
using HachijouBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HachijouBot.Commands;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using HachijouBot.KancolleNews;

namespace HachijouBot.ElectronicObserverReport;

public class GetUpgradeCostFromIssuesCommand : Command
{
    public override string Name => "getcost";

    public override string Description => "Get upgrade cost";

    public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

    private IConfiguration Config { get; }

    private ElectronicObserverApiService ApiService { get; }

    private EoDataService EoDataService { get; }

    public GetUpgradeCostFromIssuesCommand(IConfiguration config, ElectronicObserverApiService apiService, EoDataService eoDataService)
    {
        Config = config;
        ApiService = apiService;
        EoDataService = eoDataService;

        Options.Add(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithRequired(true)
            .WithName("equipmentid")
            .WithDescription("Equipment Id"));


        Options.Add(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithRequired(false)
            .WithName("shipid")
            .WithDescription("Ship id"));
    }

    public override async Task CommandHandler(SocketSlashCommand command)
    {
        if (command.Channel.Id.ToString() != Config["ReportChannelId"])
        {
            await command.RespondAsync("Permission denied");
            return;
        }

        int equipmentId = 0;
        int? shipId = null;

        foreach (var option in command.Data.Options)
        {
            if (option.Name == "equipmentid")
            {
                equipmentId = int.Parse(option.Value?.ToString() ?? "0");
            }

            if (option.Name == "shipid")
            {
                shipId = int.Parse(option.Value?.ToString() ?? "0");
            }
        }

        string url = shipId switch
        {
            { } => $"EquipmentUpgradeCostIssues/{equipmentId}?shipId={shipId}",
            _ => $"EquipmentUpgradeCostIssues/{equipmentId}",
        };

        List<EquipmentUpgradeCostIssueModel>? issues = await ApiService.GetJson<List<EquipmentUpgradeCostIssueModel>>(url);
        List<EquipmentUpgradeCostRange> issuesRanges = GetCostPerLevelRange(issues ?? []);

        StringBuilder message = new();

        foreach (EquipmentUpgradeCostRange issue in issuesRanges)
        {
            await ParseIssues(issue, message);

            if (message.Length > 1000)
            {
                await command.RespondAsync(message.ToString());
                message.Clear();
            }
        }

        if (message.Length > 0)
        {
            await command.RespondAsync(message.ToString());
        }
    }

    private async Task ParseIssues(EquipmentUpgradeCostRange issue, StringBuilder message)
    {
        ShipModel? shipModel = await EoDataService.GetShip(issue.HelperId);
        EquipmentModel? eq = await EoDataService.GetEquipment(issue.EquipmentId);

        string upgradeStage = (issue.StartLevel, issue.EndLevel) switch
        {
            (UpgradeLevel.Max, _) => "Conversion",
            _ when (issue.StartLevel == issue.EndLevel)  => ((int)issue.StartLevel).ToString(),
            _ => $"{(int)issue.StartLevel}\uff5e{((int)issue.EndLevel)}",
        };

        message.AppendLine($"## {eq?.NameEN ?? $"#{issue.EquipmentId}"} with {shipModel?.NameEN ?? $"#{issue.HelperId}"} ({upgradeStage})");
        message.AppendLine($"- {issue.Fuel} {EmoteDataBase.Fuel} {issue.Ammo} {EmoteDataBase.Ammo} {issue.Steel} {EmoteDataBase.Steel} {issue.Bauxite} {EmoteDataBase.Bauxite}");
        message.AppendLine($"- {issue.DevmatCost} {EmoteDataBase.DevMats} (slider: {issue.SliderDevmatCost} {EmoteDataBase.DevMats}) {issue.ImproveMatCost} {EmoteDataBase.Screws} (slider: {issue.SliderImproveMatCost} {EmoteDataBase.Screws})");

        foreach (EquipmentUpgradeCostItemModel consumedEquipmentReq in issue.EquipmentDetail)
        {
            EquipmentModel? requiredEquipment = await EoDataService.GetEquipment(consumedEquipmentReq.Id);

            message.AppendLine($"- {requiredEquipment?.NameEN ?? $"#{consumedEquipmentReq.Id}"} x{consumedEquipmentReq.Count}");
        }

        foreach (EquipmentUpgradeCostItemModel consumedItemId in issue.ConsumableDetail)
        {
            UseItemId useItem = (UseItemId)consumedItemId.Id;
            message.AppendLine($"- {useItem} x{consumedItemId.Count}");
        }
    }

    public static List<EquipmentUpgradeCostRange> GetCostPerLevelRange(List<EquipmentUpgradeCostIssueModel> costs)
    {
        List<EquipmentUpgradeCostRange> result = [];

        EquipmentUpgradeCostIssueModel? previousCost = null;

        foreach (EquipmentUpgradeCostIssueModel cost in costs.OrderBy(costLvl => costLvl.UpgradeLevel))
        {
            if (!cost.Equals(previousCost) || cost.UpgradeLevel is UpgradeLevel.Conversion)
            {
                result.Add(new(cost));
            }

            previousCost = cost;
        }

        for (int index = 0; index < result.Count; index++)
        {
            EquipmentUpgradeCostRange range = result[index];

            if (index < result.Count - 1)
            {
                EquipmentUpgradeCostRange nextRange = result[index + 1];

                range.EndLevel = nextRange.StartLevel switch
                {
                    UpgradeLevel.Conversion => UpgradeLevel.Max,
                    _ => nextRange.StartLevel - 1,
                };
            }
            else
            {
                range.EndLevel = range.StartLevel switch
                {
                    UpgradeLevel.Conversion => UpgradeLevel.Conversion,
                    _ => UpgradeLevel.Max,
                };
            }
        }

        return result;
    }
}
