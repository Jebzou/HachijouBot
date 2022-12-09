using HachijouBot.Models;

namespace HachijouBot.BooruManager.Models
{
    public class DanbooruWatcherTagModel : DatabaseModel
    {
        public List<string> Tags { get; set; } = new List<string>();

        public uint? LastId { get; set; } = null;

        public string TagDisplay => string.Join(',', Tags);
    }
}
