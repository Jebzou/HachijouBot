using Discord;
using HachijouBot.Github;
using System.Text;

namespace HachijouBot.KancolleNews
{
    public class KancolleNewsService
    {
        private GithubCommits GithubCommitService { get; set; } = new GithubCommits()
        {
            Username = "ElectronicObserverEN",
            Repository = "Data"
        };

        private EoUpdateService EoUpdateService { get; set; } = new EoUpdateService();

        public KancolleNewsService()
        {
            KancolleNewsDatabase.LoadSubscriptions();

            // Every 10 minutes
            System.Timers.Timer timer = new System.Timers.Timer(600000);

            timer.Elapsed += TimerTick;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private async void TimerTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            await CheckNews();
        }

        public async Task CheckNews(KancolleNewsSubscriptionModel subscription)
        {
            // Get last commit
            GithubCommitModel? commit = await GithubCommitService.GetLatestCommit();
            if (commit is null) return;
            
            try
            {
                await CheckNewsForOneChannel(subscription, commit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }

            KancolleNewsDatabase.SaveSubscriptions();
        }

        private async Task CheckNews()
        {
            // Get last commit
            GithubCommitModel? commit = await GithubCommitService.GetLatestCommit();
            if (commit is null) return;

            // For each channel check last update and run update if needed
            foreach (KancolleNewsSubscriptionModel subscription in KancolleNewsDatabase.SubscriptionsLoaded)
            {
                try
                {
                    await CheckNewsForOneChannel(subscription, commit);

                    subscription.LastUpdateCommitId = commit.CommitId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                }
            }

            KancolleNewsDatabase.SaveSubscriptions();
        }

        private async Task CheckNewsForOneChannel(KancolleNewsSubscriptionModel subscription, GithubCommitModel commit)
        {
            if (subscription.LastUpdateCommitId == commit.CommitId) return;

            // get update file
            List<EoUpdateModel> newUpdateModel = await EoUpdateService.GetUpdates(commit.CommitId);
            newUpdateModel = newUpdateModel.Where(upd => upd.WasLiveUpdate is false).ToList();


            EoUpdateModel? lastNewUpdate = newUpdateModel
                .Where(upd => upd.WasLiveUpdate is false)
                .Where(upd => upd.UpdateIsComing() || upd.UpdateInProgress())
                .MaxBy(upd => upd.UpdateDate);

            if (lastNewUpdate is null)
            {
                return;
            }

            if (subscription.LastUpdateCommitId is null)
            {
                // Get the last maint : 
                await PostNewMaintenanceInformation(subscription, lastNewUpdate);

                if (!string.IsNullOrEmpty(lastNewUpdate.EndTweetLink)) await PostMessage(subscription, $"{lastNewUpdate.EndTweetLink}");
                else if (!string.IsNullOrEmpty(lastNewUpdate.StartTweetLink)) await PostMessage(subscription, $"{lastNewUpdate.StartTweetLink}");

                return;
            }

            List<EoUpdateModel> oldUpdateModel = await EoUpdateService.GetUpdates(subscription.LastUpdateCommitId);

            EoUpdateModel? lastOldUpdate = oldUpdateModel
                .Where(upd => upd.WasLiveUpdate is false)
                .Where(upd => upd.UpdateDate <= lastNewUpdate.UpdateDate)
                .MaxBy(upd => upd.UpdateDate);

            subscription.LastUpdateCommitId = commit.CommitId;

            if (lastOldUpdate is null)
            {
                // Do nothing
                return;
            }

            // New update ?
            if (lastOldUpdate.Id != lastNewUpdate.Id)
            {
                await PostNewMaintenanceInformation(subscription, lastNewUpdate);
            }
            // They announced end time ?
            else if (lastOldUpdate.UpdateEndTime is null && lastNewUpdate.UpdateEndTime is not null)
            {
                await PostUpdateMaintenanceInformation(subscription, lastNewUpdate, false);
            }
            // They announced new end time ? = delay 
            else if (lastOldUpdate.UpdateEndTime is not null && lastNewUpdate.UpdateEndTime is not null && lastOldUpdate.UpdateEndTime != lastNewUpdate.UpdateEndTime)
            {
                await PostUpdateMaintenanceInformation(subscription, lastNewUpdate, true);
            }

            if (lastOldUpdate.EndTweetLink != lastNewUpdate.EndTweetLink) await PostMessage(subscription, $"{lastNewUpdate.EndTweetLink}");
            else if (lastOldUpdate.StartTweetLink != lastNewUpdate.StartTweetLink) await PostMessage(subscription, $"{lastNewUpdate.StartTweetLink}");
        }

        private async Task PostMessage(KancolleNewsSubscriptionModel subscription, string message)
        {
            ITextChannel? channel = await Hachijou.GetInstance().Client.GetChannelAsync(subscription.ChannelId) as ITextChannel;
            if (channel is not null) await channel.SendMessageAsync(message);
        }

        private async Task PostNewMaintenanceInformation(KancolleNewsSubscriptionModel subscription, EoUpdateModel newUpdateModel)
        {
            await PostMessage(subscription, NewMaintenanceDiscordTimeStamp(newUpdateModel));
        }

        private async Task PostUpdateMaintenanceInformation(KancolleNewsSubscriptionModel subscription, EoUpdateModel newUpdateModel, bool delay)
        {
            await PostMessage(subscription, NewMaintenanceEndDiscordTimeStamp(newUpdateModel, delay));
        }

        private string NewMaintenanceEndDiscordTimeStamp(EoUpdateModel newUpdateModel, bool isDelay)
        {
            if (newUpdateModel.UpdateEndTime is not { } updateEnd) return "";

            // Times are in JST, need to convert back to UTC
            TimeZoneInfo japaneseTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

            DateTime end = newUpdateModel.UpdateDate.Add(updateEnd);
            DateTimeOffset endUtcTime = TimeZoneInfo.ConvertTimeToUtc(end, japaneseTimeZone);

            return isDelay switch
            {
                true =>
                    $"New planned maintenance end time is <t:{endUtcTime.ToUnixTimeSeconds()}:F> (<t:{endUtcTime.ToUnixTimeSeconds()}:R>)",
                _ =>
                    $"Next maintenance should end on <t:{endUtcTime.ToUnixTimeSeconds()}:F> (<t:{endUtcTime.ToUnixTimeSeconds()}:R>)"
            };
        }

        private string NewMaintenanceDiscordTimeStamp(EoUpdateModel newUpdateModel)
        {
            StringBuilder timestamp = new StringBuilder();

            // Times are in JST, need to convert back to UTC
            DateTime start = newUpdateModel.UpdateDate.Add(newUpdateModel.UpdateStartTime);
            TimeZoneInfo japaneseTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset startUtcTime = TimeZoneInfo.ConvertTimeToUtc(start, japaneseTimeZone);

            timestamp.AppendLine($"Next maintenance starts on <t:{startUtcTime.ToUnixTimeSeconds()}:F> (<t:{startUtcTime.ToUnixTimeSeconds()}:R>)");

            if (newUpdateModel.UpdateEndTime is { } updateEnd)
            {
                DateTime end = newUpdateModel.UpdateDate.Add(updateEnd);
                DateTimeOffset endUtcTime = TimeZoneInfo.ConvertTimeToUtc(end, japaneseTimeZone);

                timestamp.AppendLine($"Next maintenance should end on <t:{endUtcTime.ToUnixTimeSeconds()}:F> (<t:{endUtcTime.ToUnixTimeSeconds()}:R>)");
            }

            return timestamp.ToString();
        }
    }
}
