using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager;
using HachijouBot.Commands;
using HachijouBot.Commands.Reminder;
using HachijouBot.Common;
using HachijouBot.Extensions;
using Microsoft.Extensions.Configuration;
using System.Timers;
using HachijouBot.ElectronicObserverReport;
using HachijouBot.KancolleNews;

namespace HachijouBot
{
    public class Hachijou
    {
        public static Hachijou GetInstance()
        {
            if (HachijouChan is null) HachijouChan = new Hachijou();
            return HachijouChan;
        }

        private static Hachijou? HachijouChan { get; set; }

        private Hachijou() { }

        public HachijouDiscordSocketClient Client { get; private set; }

        private SlashCommandManager CustomCommandManager;

        private BooruManager.BooruManager BooruManager { get; set; }

        private IConfiguration Configuration { get; set; }

        private ReminderManager ReminderManager;

        public KancolleNewsService KancolleNewsService { get; private set; }

        public ElectronicObserverReportService? ElectronicObserverReportService { get; private set; }
        public ElectronicObserverApiService ElectronicObserverApiService { get; private set; }

        public async Task Initialize()
        {
            Client = new HachijouDiscordSocketClient();

            Client.JoinedGuild += Client_JoinedGuild;

            //Hook into log event and write it out to the console
            Client.Log += Log;

            //Hook into the client ready event
            Client.Ready += Ready;

            //Create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory + "/data")
                .AddJsonFile(path: "config.json");

            Configuration = _builder.Build();

            Client.BotOwnerId = ulong.Parse(Configuration["OwnerId"] ?? "0");

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

            if (command.GuildId is ulong guildId)
            {
                // With global commands we don't need the guild.
                await Client.GetGuild(guildId).CreateApplicationCommandAsync(botCommand.Build());
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

        public void RemoveSlashCommand(Command command)
        {
            // it doesn't seem to work, 

            /*if (command.SocketCommand is null) return;
            if (command.GuildId is null)
            {
                command.SocketCommand = await Client.GetGlobalApplicationCommandAsync(command.SocketCommand.Id);
                await command.SocketCommand.DeleteAsync();
            }
            else
            {
                // With global commands we don't need the guild.
                command.SocketCommand = await Client.GetGuild((ulong)command.GuildId).GetApplicationCommandAsync(command.SocketCommand.Id);
                await command.SocketCommand.DeleteAsync();
            }*/
                
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

            //CustomCommandManager = new SlashCommandManager(this);
            //Client.SlashCommandExecuted += CustomCommandManager.ExecuteSlashCommand;

            Console.WriteLine($"Done loading Discord");

            // Load danbooru
            Console.WriteLine($"Loading Danbooru");
            //LoadDanbooru();

            Console.WriteLine($"Done loading Danbooru");

            Console.WriteLine($"Load reminders");
            //ReminderManager = new ReminderManager();
            Console.WriteLine($"Done loading reminders");
            
            Console.WriteLine($"Load news service");
            //KancolleNewsService = new KancolleNewsService();
            Console.WriteLine($"Done loading news service");
            
            LoadEoReportService();

            return Task.CompletedTask;
        }

        private void LoadEoReportService()
        {
            if (string.IsNullOrEmpty(Configuration["EoApiUrl"])) return;
            if (ElectronicObserverReportService is not null) return;

            Console.WriteLine("Load EO issue report service");
            ElectronicObserverApiService = new(Configuration["EoApiUrl"], Configuration["EoApiSecret"]);
            EoDataService data = new EoDataService();
            ElectronicObserverReportService = new(ElectronicObserverApiService, Configuration["ReportChannelId"], data);
            Console.WriteLine("Done loading EO issue report service");
        }

        private void LoadDanbooru()
        {
            BooruManager = new BooruManager.BooruManager(this);
            HachijouDanbooru.Login = Configuration["DanbooruLogin"];
            HachijouDanbooru.ApiKey = Configuration["DanbooruApiKey"];
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
