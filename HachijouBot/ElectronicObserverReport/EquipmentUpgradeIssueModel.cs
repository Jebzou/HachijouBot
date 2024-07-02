﻿using System.Text.Json.Serialization;

namespace HachijouBot.ElectronicObserverReport;

public record EquipmentUpgradeIssueModel
{
    [JsonPropertyName("software_version")] public string SoftwareVersion { get; set; } = "";

    [JsonPropertyName("data_version")] public int DataVersion { get; set; }

    [JsonPropertyName("expected")] public List<int> ExpectedUpgrades { get; set; } = new();

    [JsonPropertyName("actual")] public List<int> ActualUpgrades { get; set; } = new();

    [JsonPropertyName("day")] public DayOfWeek Day { get; set; }

    [JsonPropertyName("helperId")] public int HelperId { get; set; }
    
    [JsonPropertyName("id")] public int Id { get; set; }
}