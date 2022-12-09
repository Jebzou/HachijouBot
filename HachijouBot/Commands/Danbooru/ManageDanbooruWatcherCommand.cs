using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager;
using HachijouBot.BooruManager.Models;
using HachijouBot.Commands.ManageDatabase;
using HachijouBot.Extensions;

namespace HachijouBot.Commands.MapInfo
{
    public class ManageDanbooruWatcherCommand : Command
    {
        public override string Name => "managedanbooruwatcher";
        public override string Description => "Manage danbooru watcher for this channel";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public ManageDanbooruWatcherCommand()
        {

        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            ManageDanbooruWatcherEmbedManager databaseManagment = new()
            {
                UserId = command.User.Id,
                ChannelId = command.Channel.Id,
            };

            return command.RespondAsync("Select tags to manage", components: databaseManagment.Build());
        }
    }
}
