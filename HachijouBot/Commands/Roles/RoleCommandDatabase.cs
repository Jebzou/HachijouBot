﻿using HachijouBot.Common;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.Commands.Roles
{
    public class RoleCommandDatabase : IDataBase
    {
        private const string CommandPath = "data/RoleCommands.json";

        public static List<RoleCommand> CommandsLoaded = new List<RoleCommand>();

        public static event EventHandler<RoleCommand>? OnCommandAdd;
        public static event EventHandler<RoleCommand>? OnCommandDelete;

        public static void DeleteCommand(RoleCommand roleCommandToDelete)
        {
            bool deleted = CommandsLoaded.Remove(roleCommandToDelete);

            if (!deleted)
            {
                throw new Exception("Command not found");
            }

            JsonHelper.WriteJson(CommandPath, CommandsLoaded);

            OnCommandDelete?.Invoke(null, roleCommandToDelete);
        }

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

        public DataTable GetData(ulong guildId)
        {
            return GetData(CommandsLoaded.Where(c => c.GuildId == guildId).ToList());
        }

        public DataTable GetData()
        {
            return GetData(CommandsLoaded);
        }

        private DataTable GetData(List<RoleCommand> commands)
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Name");
            results.Columns.Add("Description");
            results.Columns.Add("Role");

            foreach (RoleCommand command in commands)
            {
                string guildName = "Global";

                if (command.GuildId != null) guildName = Hachijou.GetInstance().Client.GetGuild((ulong)command.GuildId).Name;

                results.Rows.Add(guildName, command.Name, command.Description, command.RoleName);
            }

            return results;
        }
    }
}
