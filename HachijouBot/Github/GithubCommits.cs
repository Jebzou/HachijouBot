using System.Net.Http.Json;

namespace HachijouBot.Github;

/// <summary>
/// Class used to read commits on github
/// </summary>
public class GithubCommits
{
    private HttpClient HttpClient { get; }

    public string Username { get; init; } = "";
    public string Repository { get; init; } = "";

    private string FetchUrl => $"https://api.github.com/repos/{Username}/{Repository}/commits?per_page=1&page=1";

    public GithubCommits()
    {
        HttpClient = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "HachijouBot" }
            }
        };
    }

    public async Task<GithubCommitModel?> GetLatestCommit()
    {
        List<GithubCommitModel>? latestCommit =  await HttpClient.GetFromJsonAsync<List<GithubCommitModel>?>(FetchUrl);

        if (latestCommit is null) return null;
        return !latestCommit.Any() ? null : latestCommit[0];
    }
}
