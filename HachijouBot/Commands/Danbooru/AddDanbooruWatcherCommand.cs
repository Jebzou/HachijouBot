using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager;
using HachijouBot.BooruManager.Models;
using HachijouBot.Common;
using HachijouBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands.MapInfo
{
    public class AddDanbooruWatcherCommand : Command
    {
        public override string Name => "adddanbooruwatcher";
        public override string Description => "Add a new danbooru watcher";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public AddDanbooruWatcherCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("tags")
                .WithDescription("Tags to watch (seprated by ,)"));

        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            DanbooruWatcherChannelModel model = new DanbooruWatcherChannelModel();
            string[] tags = new string[0];

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "tags") tags = (option.Value?.ToString() ?? "").Split(",");
            }

            model.ChannelId = command.ChannelId;
            model.Tags = new List<DanbooruWatcherTagModel>()
            {
                new DanbooruWatcherTagModel()
                {
                    Tags = tags.ToList(),
                    LastId = null,
                }
            };

            DanbooruImageWatcherDataBase.AddWatcher(model);

            return command.RespondAsync("Watcher added !");
        }
    }
}
