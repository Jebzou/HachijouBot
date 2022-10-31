using Discord.WebSocket;
using HachijouBot.Extensions;

namespace HachijouBot.Commands.ManageDatabase
{
    public class ManageDatabaseCommand : Command
    {
        public override string Name => "managedatabase";
        public override string Description => "Manage the databases of the bot";

        public override Task CommandHandler(SocketSlashCommand command)
        {
            if (!command.User.IsBotOwner()) 
                return command.RespondAsync("Not enough permission");

            return command.RespondAsync("Select a database to view", components: new ManageDataBaseEmbedManager().Build());

        }
    }
}
