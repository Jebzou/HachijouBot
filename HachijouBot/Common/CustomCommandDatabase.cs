using HachijouBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Common
{
    public static class CustomCommandDatabase
    {
        private const string CommandPath = "CustomCommands.json";

        public static List<CustomCommand> CommandsLoaded = new List<CustomCommand>();

        public static event EventHandler<CustomCommand>? OnCommandAdd;

        public static void AddCommand(CustomCommand customCommandToAdd)
        {
            CommandsLoaded.Add(customCommandToAdd);
            JsonHelper.WriteJson(CommandPath, CommandsLoaded);

            OnCommandAdd?.Invoke(null, customCommandToAdd);
        }

        public static void LoadCommands()
        {
            CommandsLoaded.Clear();

            if (!File.Exists(CommandPath)) return;

            CommandsLoaded = JsonHelper.ReadJson<List<CustomCommand>>(CommandPath);
        }
    }
}
