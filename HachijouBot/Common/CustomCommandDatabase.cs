using HachijouBot.Commands;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.Common
{
    public class CustomCommandDatabase : IDataBase
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

        public DataTable GetData()
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Name");
            results.Columns.Add("Description");
            results.Columns.Add("Text to return");

            foreach (CustomCommand command in CommandsLoaded)
            {
                string guildName = "Global";

                if (command.GuildId != null) guildName = Hachijou.GetInstance().Client.GetGuild((ulong)command.GuildId).Name;

                results.Rows.Add(guildName, command.Name, command.Description, command.TextToReturn);
            }

            return results;
        }
    }
}
