using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands.Roles
{
    public static class RoleCommandDatabase
    {
        private const string CommandPath = "RoleCommands.json";

        public static List<RoleCommand> CommandsLoaded = new List<RoleCommand>();

        public static event EventHandler<RoleCommand>? OnCommandAdd;

        public static void AddCommand(RoleCommand commandToAdd)
        {
            CommandsLoaded.Add(commandToAdd);
            JsonHelper.WriteJson(CommandPath, CommandsLoaded);

            OnCommandAdd?.Invoke(null, commandToAdd);
        }

        public static void LoadCommands()
        {
            CommandsLoaded.Clear();

            if (!File.Exists(CommandPath)) return;

            CommandsLoaded = JsonHelper.ReadJson<List<RoleCommand>>(CommandPath);
        }
    }
}
