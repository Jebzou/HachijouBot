using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands.Roles
{
    public class RoleCommand : Command
    {
        public override string Name { get => CommandAlias; }
        public override string Description { get => $"Give/take the role {RoleName}"; }

        public string CommandAlias { get; set; }
        public string RoleName { get; set; }
        public ulong RoleId { get; set; }

        public RoleCommand(string alias, ulong roleId, ulong guildId, string roleName)
        {
            CommandAlias = alias;
            RoleId = roleId;
            RoleName = roleName;
            GuildId = guildId;
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            if (command.GuildId is null) return command.RespondAsync("Can only execute this command in a server");
            if (command.GuildId != GuildId) return command.RespondAsync("Command not found");

            SocketGuildChannel? channel = (command.Channel as SocketGuildChannel);
            if (channel is null) return command.RespondAsync("Can only execute this command in a server");

            IGuildUser? user = command.User as IGuildUser;
            if (user is null) return command.RespondAsync("Can only execute this command in a server");

            if (user.RoleIds.Contains(RoleId))
            {
                user.RemoveRoleAsync(RoleId).Wait();

                return command.RespondAsync("Role has been removed");
            }
            else
            {
                user.AddRoleAsync(RoleId).Wait();

                return command.RespondAsync("Role has been added");
            }
        }
    }
}
