using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public class RepeatCommand : Command
    {
        public override string Name { get => "repeat"; }
        public override string Description { get => "Reply with what you send"; }

        public RepeatCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("message1")
                .WithDescription("First message to repeat"));


            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("message2")
                .WithDescription("Second message to repeat"));
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            string response = "";

            foreach (SocketSlashCommandDataOption message in command.Data.Options)
            {
                response += message.Value.ToString() + " ";
            }

            return command.RespondAsync(string.IsNullOrEmpty(response) ? "No parameter provided" : response);
        }
    }
}
