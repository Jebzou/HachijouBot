using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager.Models;
using HachijouBot.Common;
using HachijouBot.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.BooruManager
{
    public class DanbooruImageWatcherDataBase : IDataBase
    {
        private const string Path = "DanbooruWatchers.json";

        public static List<DanbooruWatcherChannelModel> Watchers = new List<DanbooruWatcherChannelModel>();

        public static event EventHandler<DanbooruWatcherChannelModel>? OnWatcherAdd;

        public static void DeleteWatcherTag(DanbooruWatcherChannelModel watcherChannelModel, DanbooruWatcherTagModel watcherTagModel)
        {
            DanbooruWatcherChannelModel? channelWatcher = Watchers.Find(watcherPerChannel => watcherPerChannel.Id == watcherChannelModel.Id);
            if (channelWatcher is null) return;

            DanbooruWatcherTagModel? tagToDelete = channelWatcher.Tags.Find(tag => tag.Id == watcherTagModel.Id);
            if (tagToDelete is null) return;

            watcherChannelModel.Tags.Remove(watcherTagModel);

            if (!watcherChannelModel.Tags.Any())
            {
                Watchers.Remove(watcherChannelModel);
            }

            SaveWatchers();
        }

        public static void AddWatcher(DanbooruWatcherChannelModel watcher)
        {
            DanbooruWatcherChannelModel? channelWatcher = Watchers.Find(watcherPerChannel => watcherPerChannel.ChannelId == watcher.ChannelId);

            if (channelWatcher != null)
            {
                channelWatcher.Tags.AddRange(watcher.Tags);
                watcher = channelWatcher;
            }
            else
            {
                Watchers.Add(watcher);
            }

            SaveWatchers();

            OnWatcherAdd?.Invoke(null, watcher);
        }

        public static void LoadWatchers()
        {
            Watchers.Clear();

            if (!File.Exists(Path)) return;

            Watchers = JsonHelper.ReadJson<List<DanbooruWatcherChannelModel>>(Path);
        }

        public static void SaveWatchers()
        {
            JsonHelper.WriteJson(Path, Watchers);
        }

        public DataTable GetData(ulong guildId)
        {
            return GetData(Watchers.Where(c => ((ITextChannel)Hachijou.GetInstance().Client.GetChannel(c.ChannelId ?? 0)).GuildId == guildId).ToList());
        }

        public DataTable GetData()
        {
            return GetData(Watchers);
        }

        private DataTable GetData(List<DanbooruWatcherChannelModel> commands)
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Channel");
            results.Columns.Add("Tags");
            results.Columns.Add("Last Id");

            foreach (DanbooruWatcherChannelModel watcher in commands)
            {
                string guildName = "";
                string channelName = "";

                if (watcher.ChannelId != null)
                {
                    ITextChannel channel = (ITextChannel)Hachijou.GetInstance().Client.GetChannel((ulong)watcher.ChannelId);
                    channelName = channel.Name;
                    guildName = channel.Guild.Name;
                }

                foreach (DanbooruWatcherTagModel channel in watcher.Tags)
                {
                    results.Rows.Add(guildName, channelName, channel.Tags.Aggregate("", (tagA, tagB) => tagA + "," + tagB, tag => tag), channel.LastId);
                }
            }

            return results;
        }
    }
}
