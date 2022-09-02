using Discord;
using Discord.WebSocket;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public class AddCommandCommand : Command
    {
        public override string Name { get => "addcommand"; }
        public override string Description { get => "Add a new custom command"; }

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public AddCommandCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("commandname")
                .WithDescription("Command name"));


            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("commandreply")
                .WithDescription("The reply to the command"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("commanddescription")
                .WithDescription("The description of the command"));
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            string commandName = "";
            string commandDescription = "";
            string commandReply = "";

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "commandname") commandName = option.Value?.ToString() ?? "";
                if (option.Name == "commanddescription") commandDescription = option.Value?.ToString() ?? "";
                if (option.Name == "commandreply") commandReply = option.Value?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(commandName) || string.IsNullOrEmpty(commandReply)) return command.RespondAsync("Invalid parameters");

            CustomCommandDatabase.AddCommand(new CustomCommand(commandName, commandDescription, commandReply));

            return command.RespondAsync("Command added !");
        }
    }
}
