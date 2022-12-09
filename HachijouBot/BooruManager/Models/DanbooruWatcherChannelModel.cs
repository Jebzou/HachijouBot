using HachijouBot.Models;

namespace HachijouBot.BooruManager.Models
{
    public class DanbooruWatcherChannelModel : DatabaseModel
    {
        public ulong? ChannelId { get; set; } = null;

        public List<DanbooruWatcherTagModel> Tags { get; set; } = new List<DanbooruWatcherTagModel>();
    }
}
