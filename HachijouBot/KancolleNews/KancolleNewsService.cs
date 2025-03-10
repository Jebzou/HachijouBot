﻿using Discord;
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
            CheckNews().Wait(30000);

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
            try
            {
                // Get last commit
                GithubCommitModel? commit = await GithubCommitService.GetLatestCommit();
                if (commit is null) return;
            
                await CheckNewsForOneChannel(subscription, commit);

                subscription.LastUpdateCommitId = commit.CommitId;
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
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }

            KancolleNewsDatabase.SaveSubscriptions();
        }

        private async Task CheckNewsForOneChannel(KancolleNewsSubscriptionModel subscription, GithubCommitModel commit)
        {
            if (subscription.LastUpdateCommitId == commit.CommitId) return;

            // get update file
            List<EoUpdateModel> newUpdateModel = await EoUpdateService.GetUpdates(commit.CommitId);
            newUpdateModel = newUpdateModel.Where(upd => upd.WasLiveUpdate is false).ToList();


            EoUpdateModel? latestUpdateCurrentState = newUpdateModel
                .Where(upd => !upd.WasLiveUpdate)
                .Where(upd => upd.UpdateDate is not null)
                .Where(upd => upd.UpdateStartTime is not null)
                .Where(upd => upd.UpdateIsComing() || upd.UpdateInProgress())
                .MinBy(upd => upd.UpdateDate);

            if (latestUpdateCurrentState is null)
            {
                return;
            }

            if (subscription.LastUpdateCommitId is null)
            {
                // Get the last maint : 
                await PostNewMaintenanceInformation(subscription, latestUpdateCurrentState);

                if (!string.IsNullOrEmpty(latestUpdateCurrentState.EndTweetLink)) await PostMessage(subscription, $"{latestUpdateCurrentState.EndTweetLink}");
                else if (!string.IsNullOrEmpty(latestUpdateCurrentState.StartTweetLink)) await PostMessage(subscription, $"{latestUpdateCurrentState.StartTweetLink}");

                return;
            }

            List<EoUpdateModel> oldUpdateModel = await EoUpdateService.GetUpdates(subscription.LastUpdateCommitId);

            EoUpdateModel? latestUpdateInPreviousState = oldUpdateModel.Find(upd => upd.Id == latestUpdateCurrentState.Id);

            subscription.LastUpdateCommitId = commit.CommitId;

            // New update ?
            if (latestUpdateInPreviousState is null)
            {
                await PostNewMaintenanceInformation(subscription, latestUpdateCurrentState);
            }
            // They announced end time ?
            else if (latestUpdateInPreviousState.UpdateEndTime is null && latestUpdateCurrentState.UpdateEndTime is not null)
            {
                await PostUpdateMaintenanceInformation(subscription, latestUpdateCurrentState, false);
            }
            // They announced new end time ? = delay 
            else if (latestUpdateInPreviousState.UpdateEndTime is not null && latestUpdateCurrentState.UpdateEndTime is not null && latestUpdateInPreviousState.UpdateEndTime != latestUpdateCurrentState.UpdateEndTime)
            {
                await PostUpdateMaintenanceInformation(subscription, latestUpdateCurrentState, true);
            }
            else
            {
                return;
            }

            if (latestUpdateInPreviousState is null || latestUpdateInPreviousState.StartTweetLink != latestUpdateCurrentState.StartTweetLink) await PostMessage(subscription, $"{latestUpdateCurrentState.StartTweetLink}");
            else if (latestUpdateInPreviousState.EndTweetLink != latestUpdateCurrentState.EndTweetLink) await PostMessage(subscription, $"{latestUpdateCurrentState.EndTweetLink}");
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
            if (newUpdateModel is not { UpdateDate: {} updateStart, UpdateEndTime: {} updateEnd}) return "";

            // Times are in JST, need to convert back to UTC
            TimeZoneInfo japaneseTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

            DateTime end = updateStart.Add(updateEnd);
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
            if (newUpdateModel is not { UpdateDate: { } updateStart, UpdateStartTime: { } updateStartTime}) return "";

            StringBuilder timestamp = new StringBuilder();

            // Times are in JST, need to convert back to UTC
            DateTime start = updateStart.Add(updateStartTime);
            TimeZoneInfo japaneseTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset startUtcTime = TimeZoneInfo.ConvertTimeToUtc(start, japaneseTimeZone);

            timestamp.AppendLine($"Next maintenance starts on <t:{startUtcTime.ToUnixTimeSeconds()}:F> (<t:{startUtcTime.ToUnixTimeSeconds()}:R>)");

            if (newUpdateModel.UpdateEndTime is { } updateEnd)
            {
                DateTime end = updateStart.Add(updateEnd);
                DateTimeOffset endUtcTime = TimeZoneInfo.ConvertTimeToUtc(end, japaneseTimeZone);

                timestamp.AppendLine($"Next maintenance should end on <t:{endUtcTime.ToUnixTimeSeconds()}:F> (<t:{endUtcTime.ToUnixTimeSeconds()}:R>)");
            }

            return timestamp.ToString();
        }
    }
}
