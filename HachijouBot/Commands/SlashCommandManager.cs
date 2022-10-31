using Discord;
using Discord.Net;
using Discord.WebSocket;
using HachijouBot.Commands.Danbooru;
using HachijouBot.Commands.ManageDatabase;
using HachijouBot.Commands.MapInfo;
using HachijouBot.Commands.Roles;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public class SlashCommandManager
    {
        private List<Command> Commands = new List<Command>();

        private Hachijou Hachijou;

        public SlashCommandManager(Hachijou hachijou)
        {
            Hachijou = hachijou;

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

            AddCommand(new AddDanbooruWatcherCommand());
            AddCommand(new GetRandomPicture());

            AddCommand(new AddCommandCommand());

            AddCommand(new AddRoleCommandCommand());

            AddCommand(new ScrapCommand());

            AddCommand(new AddMapInfoCommand());
            AddCommand(new GetMapInfoCommand());
            AddCommand(new EditMapInfoCommand());

            AddCommand(new InviteCommand());

            AddCommand(new ManageDatabaseCommand());

            CustomCommandDatabase.OnCommandAdd += (_, command) => AddCommand(command);
            CustomCommandDatabase.LoadCommands();

            foreach (CustomCommand command in CustomCommandDatabase.CommandsLoaded)
            {
                AddCommand(command);
            }

            RoleCommandDatabase.OnCommandAdd += (_, command) => AddCommand(command);
            RoleCommandDatabase.LoadCommands();

            foreach (RoleCommand command in RoleCommandDatabase.CommandsLoaded)
            {
                AddCommand(command);
            }
        }

        public async void AddCommand(Command command)
        {
            Commands.Add(command);

            await Hachijou.AddSlashCommand(command);
        }
    }
}
