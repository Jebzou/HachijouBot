using System.Text.Json.Serialization;

namespace HachijouBot.Github;

public class GithubCommitModel
{
    [JsonPropertyName("commit")] public Commit Commit { get; set; } = new();
    [JsonPropertyName("sha")] public string CommitId { get; set; } = "";
}

public class Commit
{
    [JsonPropertyName("committer")] public Committer Committer { get; set; } = new();
}

public class Committer
{
    [JsonPropertyName("date")] public DateTime Date { get; set; } = new();
}

