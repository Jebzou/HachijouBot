using HachijouBot.Common;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.KancolleNews
{
    public class KancolleNewsDatabase : IDataBase
    {
        private const string Path = "KancolleNews.json";

        public static List<KancolleNewsSubscriptionModel> SubscriptionsLoaded = new List<KancolleNewsSubscriptionModel>();
        
        public static void AddSubscription(KancolleNewsSubscriptionModel subscriptionToAdd)
        {
            SubscriptionsLoaded.Add(subscriptionToAdd);
            SaveSubscriptions();
        }

        public static void LoadSubscriptions()
        {
            SubscriptionsLoaded.Clear();

            if (!File.Exists(Path)) return;

            SubscriptionsLoaded = JsonHelper.ReadJson<List<KancolleNewsSubscriptionModel>>(Path);
        }

        public static void SaveSubscriptions()
        {
            JsonHelper.WriteJson(Path, SubscriptionsLoaded);
        }

        public static KancolleNewsSubscriptionModel? GetSubscriptions(KancolleNewsSubscriptionModel baseModel)
        {
            if (SubscriptionsLoaded.Count == 0) LoadSubscriptions();

            KancolleNewsSubscriptionModel? val = SubscriptionsLoaded.Find(model => model.ChannelId == baseModel.ChannelId);

            return val;
        }

        public DataTable GetData(ulong guildId)
        {
            return GetData(SubscriptionsLoaded.Where(c => c.GuildId == guildId).ToList());
        }

        public DataTable GetData()
        {
            return GetData(SubscriptionsLoaded);
        }

        private DataTable GetData(List<KancolleNewsSubscriptionModel> commands)
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Channel");
            results.Columns.Add("Last commit id");

            foreach (KancolleNewsSubscriptionModel sub in commands)
            {
                string guildName = Hachijou.GetInstance().Client.GetGuild(sub.GuildId).Name;
                string channelName = Hachijou.GetInstance().Client.GetGuild(sub.GuildId).GetChannel(sub.ChannelId).Name;

                results.Rows.Add(guildName, channelName, sub.LastUpdateCommitId);
            }

            return results;
        }
    }
}
