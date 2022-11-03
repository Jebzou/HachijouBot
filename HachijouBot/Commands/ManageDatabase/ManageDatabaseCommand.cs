using Discord;
using Discord.WebSocket;
using HachijouBot.Extensions;

namespace HachijouBot.Commands.ManageDatabase
{
    public class ManageDatabaseCommand : Command
    {
        public override string Name => "managedatabase";
        public override string Description => "Manage the databases of the bot";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public override Task CommandHandler(SocketSlashCommand command)
        {
            ManageDataBaseEmbedManager databaseManagment = new ManageDataBaseEmbedManager();

            databaseManagment.IsOwner = command.User.IsBotOwner();
            databaseManagment.UserId = command.User.Id;

            return command.RespondAsync("Select a database to view", components: databaseManagment.Build());

        }
    }
}
