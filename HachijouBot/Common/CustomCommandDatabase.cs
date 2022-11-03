using HachijouBot.Commands;
using HachijouBot.Commands.Roles;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.Common
{
    public class CustomCommandDatabase : IDataBase
    {
        private const string CommandPath = "CustomCommands.json";

        public static List<CustomCommand> CommandsLoaded = new List<CustomCommand>();

        public static event EventHandler<CustomCommand>? OnCommandAdd;
        public static event EventHandler<CustomCommand>? OnCommandDelete;

        public static void DeleteCommand(CustomCommand customCommandToDelete)
        {
            bool deleted = CommandsLoaded.Remove(customCommandToDelete);

            if (!deleted)
            {
                throw new Exception("Command not found");
            }

            JsonHelper.WriteJson(CommandPath, CommandsLoaded);

            OnCommandDelete?.Invoke(null, customCommandToDelete);
        }

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

        public DataTable GetData(ulong guildId)
        {
            return GetData(CommandsLoaded.Where(c => c.GuildId == guildId).ToList());
        }

        public DataTable GetData()
        {
            return GetData(CommandsLoaded);
        }

        private DataTable GetData(List<CustomCommand> commands)
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Name");
            results.Columns.Add("Description");
            results.Columns.Add("Text to return");

            foreach (CustomCommand command in commands)
            {
                string guildName = "Global";

                if (command.GuildId != null) guildName = Hachijou.GetInstance().Client.GetGuild((ulong)command.GuildId).Name;

                results.Rows.Add(guildName, command.Name, command.Description, command.TextToReturn);
            }

            return results;
        }
    }
}
