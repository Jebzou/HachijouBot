namespace HachijouBot.KancolleNews
{
    public class KancolleNewsSubscriptionModel
    {
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }

        public string? LastUpdateCommitId { get; set; }
    }
}
