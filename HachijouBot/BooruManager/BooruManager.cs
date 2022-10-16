using Discord;
using HachijouBot.BooruManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    await CheckMissingPicsForAChannel(watcherInfos, channel);
                }

                DanbooruImageWatcherDataBase.SaveWatchers();
            }
            catch (Exception ex)
            {
                Hachijou.HandleError(ex);
            }
        }

        private static async Task CheckMissingPicsForAChannel(DanbooruWatcherChannelModel watcherInfos, IMessageChannel channel)
        {
            foreach (DanbooruWatcherTagModel watcherTagInfo in watcherInfos.Tags)
            {
                DanbooruImageWatcher watcher = new DanbooruImageWatcher(watcherTagInfo);

                List<string> newPics = await watcher.GetNewImagesAsync();

                if (newPics.Count > 0)
                {
                    await channel.SendMessageAsync("New pictures detected !");

                    foreach (string pics in newPics)
                    {
                        await channel.SendMessageAsync(pics);
                    }
                }
            }
        }
    }
}
