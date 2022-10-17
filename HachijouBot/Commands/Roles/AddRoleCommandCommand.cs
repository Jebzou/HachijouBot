using Discord;
using Discord.WebSocket;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace HachijouBot.Commands.Roles
{
    internal class AddRoleCommandCommand  : Command
    {
        public override string Name { get => "addrolecommand"; }
        public override string Description { get => "Add a command that gives/takes a role"; }

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public AddRoleCommandCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("commandalias")
                .WithDescription("Command alias"));


            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("rolehandle")
                .WithDescription("The @ of the role (basically ping the role here)"));
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            string commandAlias = "";
            string roleHandle = "";

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "commandalias") commandAlias = option.Value?.ToString() ?? "";
                if (option.Name == "rolehandle") roleHandle = option.Value?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(commandAlias) || string.IsNullOrEmpty(roleHandle)) return command.RespondAsync("Invalid parameters");

            if (command.GuildId is null) return command.RespondAsync("Can't execute this command outside of a server");

            // get the role from params
            SocketGuildChannel? channel = (command.Channel as SocketGuildChannel);
            if (channel is null) return command.RespondAsync("Can't execute this command outside of a server");

            //<@&568890624065929216>
            Regex regexHandle = new Regex("<@&([0-9]+)>");
            if (!regexHandle.IsMatch(roleHandle)) return command.RespondAsync("Handle parameter is not valid");

            GroupCollection groups = regexHandle.Match(roleHandle).Groups;
            if (groups.Count == 0) return command.RespondAsync("Handle parameter is not valid");

            string handleParsed = groups[1].Value;
            ulong roleId = ulong.Parse(handleParsed);

            // get the role from guild
            SocketRole? guildRole = channel.Guild.Roles.FirstOrDefault(r => r.Id == roleId);
            if (guildRole is null) return command.RespondAsync("Role not found");

            RoleCommandDatabase.AddCommand(new RoleCommand(commandAlias, roleId, (ulong)command.GuildId, guildRole.Name));

            return command.RespondAsync("Command added !");
        }
    }
}
