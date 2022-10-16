using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.BooruManager.Models
{
    public class DanbooruWatcherChannelModel
    {
        public ulong? ChannelId { get; set; } = null;

        public List<DanbooruWatcherTagModel> Tags { get; set; } = new List<DanbooruWatcherTagModel>();
    }
}
