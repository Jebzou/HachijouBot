using Discord.Net;
using Discord.WebSocket;
using HachijouBot.Commands.Danbooru;
using HachijouBot.Commands.KancolleNews;
using HachijouBot.Commands.ManageDatabase;
using HachijouBot.Commands.MapInfo;
using HachijouBot.Commands.Reminder;
using HachijouBot.Commands.Roles;
using HachijouBot.Common;
using HachijouBot.ElectronicObserverReport;
using HachijouBot.KancolleNews;
using Microsoft.Extensions.Configuration;

namespace HachijouBot.Commands
{
    public class SlashCommandManager
    {
        private List<Command> Commands = new List<Command>();

        private Hachijou Hachijou { get; }
        private IConfiguration Config { get; }
        private ElectronicObserverApiService ApiService { get; }
        private EoDataService EoDataService { get; }

        public SlashCommandManager(Hachijou hachijou, IConfiguration config, ElectronicObserverApiService apiService, EoDataService eoDataService)
        {
            Hachijou = hachijou;
            Config = config;
            ApiService = apiService;
            EoDataService = eoDataService;

            // Initialize all commands
            InitializeAllCommands();
        }

        public Task ExecuteSlashCommand(SocketSlashCommand command)
        {
            try
            {
                Command? commandFound = Commands.Find(co => co.Name == command.CommandName && (co.GuildId is null || co.GuildId == command.GuildId));

                if (commandFound != null) return commandFound.CommandHandler(command);
                return command.RespondAsync("Command not found");
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.FirstOrDefault() is HttpException discordEx)
                {
                    return command.RespondAsync(discordEx.Reason);
                }

                Hachijou.HandleError(ex);
                return command.RespondAsync("An error occured");
            }
            catch (Exception ex)
            {
                Hachijou.HandleError(ex);
                return command.RespondAsync("An error occured");
            }
        }

        public void InitializeAllCommands()
        {
            Hachijou.ClearCommands();

            Commands.Clear();

            AddCommand(new GetUpgradeCostFromIssuesCommand(Config, ApiService, EoDataService));

            AddCommand(new AddDanbooruWatcherCommand());
            AddCommand(new ManageDanbooruWatcherCommand());
            AddCommand(new GetRandomPicture());

            AddCommand(new AddCommandCommand());
            AddCommand(new DeleteCommandCommand());

            AddCommand(new AddRoleCommandCommand());

            AddCommand(new ScrapCommand());

            AddCommand(new AddMapInfoCommand());
            AddCommand(new GetMapInfoCommand());
            AddCommand(new EditMapInfoCommand());

            AddCommand(new InviteCommand());

            AddCommand(new ManageDatabaseCommand());

            AddCommand(new AddReminderCommand());

            AddCommand(new AddKancolleNewsSubscriptionCommand());
            AddCommand(new RemoveKancolleNewsSubscriptionCommand());

            CustomCommandDatabase.OnCommandAdd += (_, command) => AddCommand(command);
            CustomCommandDatabase.OnCommandDelete += RemoveCommand;
            CustomCommandDatabase.LoadCommands();

            foreach (CustomCommand command in CustomCommandDatabase.CommandsLoaded)
            {
                AddCommand(command);
            }

            RoleCommandDatabase.OnCommandAdd += (_, command) => AddCommand(command);
            RoleCommandDatabase.OnCommandDelete += RemoveCommand;
            RoleCommandDatabase.LoadCommands();

            foreach (RoleCommand command in RoleCommandDatabase.CommandsLoaded)
            {
                AddCommand(command);
            }
        }

        private async void RemoveCommand(object? sender, Command command)
        {
            Commands.Remove(command);

            Hachijou.RemoveSlashCommand(command);
        }

        public async void AddCommand(Command command)
        {
            Commands.Add(command);

            await Hachijou.AddSlashCommand(command);
            Console.WriteLine($"Added new command : {command.Name}");
        }
    }
}
