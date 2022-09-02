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
            Command? commandFound = Commands.Find(co => co.Name == command.CommandName);

            if (commandFound != null) return commandFound.CommandHandler(command);
            return command.RespondAsync("Command not found");
        }

        public void InitializeAllCommands()
        {
            Commands.Clear();

            AddCommand(new AddCommandCommand());

            CustomCommandDatabase.OnCommandAdd += (_, command) => AddCommand(command);
            CustomCommandDatabase.LoadCommands();
        }

        public async void AddCommand(Command command)
        {
            Commands.Add(command);

            await Hachijou.AddSlashCommand(command);
        }
    }
}
