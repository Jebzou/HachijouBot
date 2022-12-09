namespace HachijouBot.BooruManager.Models
{
    public class DanbooruWatcherTagModel
    {
        public List<string> Tags { get; set; } = new List<string>();

        public uint? LastId { get; set; } = null;
    }
}
