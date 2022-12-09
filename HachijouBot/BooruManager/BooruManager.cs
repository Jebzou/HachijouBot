using Discord;
using HachijouBot.BooruManager.Models;

namespace HachijouBot.BooruManager
{
    public class BooruManager
    {
        private Hachijou Hachijou { get; }

        public BooruManager(Hachijou client)
        {
            DanbooruImageWatcherDataBase.LoadWatchers();
            Hachijou = client;
        }

        public async void CheckMissingPics()
        {
            try
            {
                foreach (DanbooruWatcherChannelModel watcherInfos in DanbooruImageWatcherDataBase.Watchers)
                {
                    if (watcherInfos.ChannelId is null) continue;
                    IMessageChannel channel = Hachijou.Client.GetChannel((ulong)watcherInfos.ChannelId) as IMessageChannel;
                    if (channel is null) continue;

                    try
                    {
                        await CheckMissingPicsForAChannel(watcherInfos, channel);
                    }
                    catch (Exception ex)
                    {
                        Hachijou.HandleError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Hachijou.HandleError(ex);
            }
        }

        private async Task CheckMissingPicsForAChannel(DanbooruWatcherChannelModel watcherInfos, IMessageChannel channel)
        {
            foreach (DanbooruWatcherTagModel watcherTagInfo in watcherInfos.Tags)
            {
                try
                {
                    await CheckMissingPicsForOneTag(channel, watcherTagInfo);
                }
                catch (Exception ex)
                {
                    Hachijou.HandleError(ex);
                }
            }
        }

        private async Task CheckMissingPicsForOneTag(IMessageChannel channel, DanbooruWatcherTagModel watcherTagInfo)
        {
            DanbooruWatcherTagModel newWatcherTagInfos = new()
            {
                LastId = watcherTagInfo.LastId,
                Tags = watcherTagInfo.Tags,
            };

            DanbooruImageWatcher watcher = new DanbooruImageWatcher(newWatcherTagInfos);

            List<string> newPics = await watcher.GetNewImagesAsync();

            if (newPics.Count > 0)
            {
                await channel.SendMessageAsync("New pictures detected !");

                foreach (string pics in newPics)
                {
                    await channel.SendMessageAsync(pics);
                }
            }

            // Update only if no error
            watcherTagInfo.LastId = newWatcherTagInfos.LastId;
            DanbooruImageWatcherDataBase.SaveWatchers();
        }
    }
}
