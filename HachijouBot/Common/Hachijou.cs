using Discord;
using Discord.WebSocket;
using HachijouBot.Commands;
using HachijouBot.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace HachijouBot
{
    public class Hachijou
    {
        public DiscordSocketClient Client { get; private set; }

        private SlashCommandManager CustomCommandManager;

        private BooruManager.BooruManager BooruManager { get; set; }

        private IConfiguration Configuration { get; set; }

        public async Task Initialize()
        {
            Client = new DiscordSocketClient();

            Client.JoinedGuild += Client_JoinedGuild;

            //Hook into log event and write it out to the console
            Client.Log += Log;

            //Hook into the client ready event
            Client.Ready += Ready;

            //Create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            Configuration = _builder.Build();

            //This is where we get the Token value from the configuration file
            await Client.LoginAsync(TokenType.Bot, Configuration["Token"]);
            await Client.StartAsync();
        }

        private Task Client_JoinedGuild(SocketGuild arg)
        {
            string? _ownerId = Configuration["OwnerId"];

            if (_ownerId is string _ownerIdNotNull)
            {
                Client.GetUser(ulong.Parse(_ownerIdNotNull)).SendMessageAsync($"I joined a new server : {arg.Name} monkaS");
            }  

            Console.WriteLine($"I joined a new server : {arg.Name} monkaS");

            return Task.CompletedTask;
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

            if (command is CustomCommand customCommand)
            {
                // With global commands we don't need the guild.
                await Client.GetGuild(customCommand.GuildId).CreateApplicationCommandAsync(botCommand.Build());
            }
            else
            {
                // With global commands we don't need the guild.
                await Client.CreateGlobalApplicationCommandAsync(botCommand.Build());
            }

            command.OptionsChanged += (_, _) =>
            {
                AddSlashCommand(command);
            };
        }

        public void HandleError(Exception ex)
        {
            Console.WriteLine(ex.ToString());

            SocketTextChannel? logChannel = (Client.GetChannel(691967799957913610) as SocketTextChannel);
            logChannel?.SendMessageAsync(ex.ToString());
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        public void ClearCommands()
        {
            // This basically resets commands
            Client.BulkOverwriteGlobalApplicationCommandsAsync(new ApplicationCommandProperties[0]);

            foreach (SocketGuild guild in Client.Guilds)
            {
                guild.BulkOverwriteApplicationCommandAsync(new ApplicationCommandProperties[0]);
            }
        }

        private Task Ready()
        {
            Console.WriteLine($"Connected");

            LoadEmotes();

            CustomCommandManager = new SlashCommandManager(this);
            Client.SlashCommandExecuted += CustomCommandManager.ExecuteSlashCommand;

            Console.WriteLine($"Done loading Discord");

            // Load danbooru
            Console.WriteLine($"Loading Danbooru");
            LoadDanbooru();

            Console.WriteLine($"Done loading Danbooru");

            return Task.CompletedTask;
        }

        private void LoadDanbooru()
        {
            BooruManager = new BooruManager.BooruManager(this);
            BooruManager.CheckMissingPics();

            System.Timers.Timer timer = new System.Timers.Timer(18e5);

            timer.Elapsed += OnDanbooruLoadPics;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void OnDanbooruLoadPics(object? source, ElapsedEventArgs e)
        {
            BooruManager.CheckMissingPics();
        }

        private void LoadEmotes()
        {
            IEnumerable<GuildEmote> emotes = Client.Guilds.SelectMany(g => g.Emotes);

            foreach (string key in EmoteDataBase.Emotes.Keys)
            {
                GuildEmote? emote = emotes.FirstOrDefault(e => e.Name == key);
                if (emote != null) EmoteDataBase.Emotes[key] = $"<:{emote.Name}:{emote.Id}>";
            }
        }
    }
}
