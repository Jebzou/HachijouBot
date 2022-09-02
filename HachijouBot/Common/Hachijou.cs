using Discord;
using Discord.WebSocket;
using HachijouBot.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot
{
    public class Hachijou
    {
        private DiscordSocketClient client;

        private SlashCommandManager CustomCommandManager;

        public async Task Initialize()
        {
            client = new DiscordSocketClient();

            //Hook into log event and write it out to the console
            client.Log += Log;

            //Hook into the client ready event
            client.Ready += Ready;

            //Create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            IConfiguration config = _builder.Build();

            //This is where we get the Token value from the configuration file
            await client.LoginAsync(TokenType.Bot, config["Token"]);
            await client.StartAsync();
        }


        public async Task AddSlashCommand(Command command)
        {
            SlashCommandBuilder botCommand = new SlashCommandBuilder();

            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            botCommand.WithName(command.Name);
            //botCommand.WithNameLocalizations(new Dictionary<string, string>());

            // Descriptions can have a max length of 100.
            botCommand.WithDescription(command.Description);
            //botCommand.WithDescriptionLocalizations(new Dictionary<string, string>());

            /*foreach (var option in command.Options)
            {
                if (option.DescriptionLocalizations is null) option.WithDescriptionLocalizations(new Dictionary<string, string>());
                if (option.NameLocalizations is null) option.WithNameLocalizations(new Dictionary<string, string>());
            }*/

            if (command.GuildPermission != null) botCommand.WithDefaultMemberPermissions(command.GuildPermission);

            botCommand.AddOptions(command.Options.ToArray());

            // With global commands we don't need the guild.
            await client.CreateGlobalApplicationCommandAsync(botCommand.Build());
        }

        public void HandleError(Exception ex)
        {
            Console.WriteLine(ex.ToString());

            SocketTextChannel? logChannel = (client.GetChannel(691967799957913610) as SocketTextChannel);
            logChannel?.SendMessageAsync(ex.ToString());
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task Ready()
        {
            Console.WriteLine($"Connected");

            // This basically resets commands
            client.BulkOverwriteGlobalApplicationCommandsAsync(new ApplicationCommandProperties[0]);

            CustomCommandManager = new SlashCommandManager(this);
            client.SlashCommandExecuted += CustomCommandManager.ExecuteSlashCommand;

            return Task.CompletedTask;
        }
    }
}
