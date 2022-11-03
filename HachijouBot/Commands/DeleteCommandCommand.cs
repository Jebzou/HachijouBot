using Discord;
using Discord.WebSocket;
using HachijouBot.Commands.Roles;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public class DeleteCommandCommand : Command
    {
        public override string Name { get => "deletecommand"; }
        public override string Description { get => "Delete a custom/role command"; }

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public DeleteCommandCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("commandname")
                .WithDescription("Command name"));
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            string commandName = "";

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "commandname") commandName = option.Value?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(commandName)) return command.RespondAsync("Invalid parameters");

            if (command.GuildId is null) return command.RespondAsync("Can't execute this command outside of a server");

            CustomCommand? commandToDelete = CustomCommandDatabase.CommandsLoaded.FirstOrDefault(c => c.Name == commandName && c.GuildId == command.GuildId);
            RoleCommand? roleCommandToDelete = RoleCommandDatabase.CommandsLoaded.FirstOrDefault(c => c.Name == commandName && c.GuildId == command.GuildId);

            if (commandToDelete is null && roleCommandToDelete is null) return command.RespondAsync("Command not found");

            if (commandToDelete != null) CustomCommandDatabase.DeleteCommand(commandToDelete);
            if (roleCommandToDelete != null) RoleCommandDatabase.DeleteCommand(roleCommandToDelete);

            return command.RespondAsync("Command removed !");
        }
    }
}
