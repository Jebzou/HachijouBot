using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager;
using HachijouBot.BooruManager.Models;

namespace HachijouBot.Commands.Danbooru
{
    public class GetRandomPicture : Command
    {
        public override string Name => "getpicture";
        public override string Description => "Get a random picture from danbooru";

        public GetRandomPicture()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("tags")
                .WithDescription("Tags (separated by ,)"));

        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            string[] tags = new string[0];

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "tags") tags = (option.Value?.ToString() ?? "").Split(",");
            }

            DanbooruWatcherTagModel model = new DanbooruWatcherTagModel()
            {
                Tags = tags.ToList(),
                LastId = null,
            };

            Task<string> getImage = new DanbooruImageWatcher(model).GetRandomImage();

            try
            {
                getImage.Wait();
                return command.RespondAsync(getImage.Result);
            }
            catch (AggregateException ex)
            {
                return command.RespondAsync(ex.InnerExceptions.First().Message);
            }
        }
    }
}
