using HachijouBot.BooruManager.Models;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.BooruManager
{
    public class DanbooruImageWatcherDataBase
    {
        private const string Path = "DanbooruWatchers.json";

        public static List<DanbooruWatcherChannelModel> Watchers = new List<DanbooruWatcherChannelModel>();

        public static event EventHandler<DanbooruWatcherChannelModel>? OnWatcherAdd;

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
    }
}
