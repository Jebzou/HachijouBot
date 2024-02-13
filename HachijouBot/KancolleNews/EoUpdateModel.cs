using System.Text.Json.Serialization;

namespace HachijouBot.KancolleNews;

/// <summary>
/// Kancolle update model (From Eo tools)
/// </summary>
public class EoUpdateModel
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("start_date")]
    public DateTime? UpdateDate { get; set; }

    [JsonPropertyName("start_time")]
    public TimeSpan? UpdateStartTime { get; set; } = TimeSpan.Zero;

    [JsonPropertyName("end_time")]
    public TimeSpan? UpdateEndTime { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("live_update")]
    public bool WasLiveUpdate { get; set; } = false;

    [JsonPropertyName("maint_start_tweet")]
    public string StartTweetLink { get; set; } = "";

    [JsonPropertyName("maint_end_tweet")]
    public string EndTweetLink { get; set; } = "";

    /// <summary>
    /// Returns true if update is coming
    /// </summary>
    /// <returns></returns>
    public bool UpdateIsComing()
    {
        if (UpdateDate is null) return true;
        if (UpdateStartTime is null) return true;

        DateTime dateNowJst = DateTime.UtcNow + new TimeSpan(9, 0, 0);
        DateTime start = UpdateDate.Value.Date.Add(UpdateStartTime.Value);

        // Update has started ?
        return start > dateNowJst;
    }

    /// <summary>
    /// Returns true if update is in progress
    /// </summary>
    /// <returns></returns>
    public bool UpdateInProgress()
    {
        if (UpdateDate is null) return false;
        if (UpdateStartTime is null) return false;

        DateTime dateNowJst = DateTime.UtcNow + new TimeSpan(9, 0, 0);
        DateTime start = UpdateDate.Value.Date.Add(UpdateStartTime.Value);

        // Update has started and no end time => update in progress
        if (start < dateNowJst && UpdateEndTime is null) return true;

        // Update has started and end time => update could be in progress
        if (start < dateNowJst && UpdateEndTime is { } endTime)
        {
            DateTime end = UpdateDate.Value.Date.Add(endTime);

            // End didn't happen yet => Update is in progress
            if (end > dateNowJst) return true;
        }
        return false;
    }
}
