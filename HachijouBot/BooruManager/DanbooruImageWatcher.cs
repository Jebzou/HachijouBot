using BooruDex.Booru.Client;
using BooruDex.Models;
using HachijouBot.BooruManager.Models;

namespace HachijouBot.BooruManager
{

    public class DanbooruImageWatcher
    {
        private DanbooruWatcherTagModel WatcherInfos { get; set; }

        public DanbooruImageWatcher(DanbooruWatcherTagModel watcherInfos)
        {
            WatcherInfos = watcherInfos;
        }

        public async Task<List<string>> GetNewImagesAsync()
        {
            var client = new DanbooruDonmai(new HttpClient());

            Post[] posts = await client.PostListAsync(50, WatcherInfos.Tags.ToArray());

            if (posts.Length == 0) return new List<string>();

            if (WatcherInfos.LastId is null)
            {
                // --- Only return last post
                posts = new Post[1] { posts.First() };
                WatcherInfos.LastId = 0;
            }

            // Only get new posts 
            posts = posts.Where(post => post.ID > WatcherInfos.LastId).ToArray();

            if (posts.Length == 0) return new List<string>();

            // Update last ID
            WatcherInfos.LastId = posts.First().ID;

            return posts.Select(post => post.PostUrl).ToList();
        }
    }

}
