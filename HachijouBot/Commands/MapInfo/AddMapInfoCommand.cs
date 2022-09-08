using Discord;
using Discord.WebSocket;
using HachijouBot.Common;
using HachijouBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands.MapInfo
{
    public class AddMapInfoCommand : Command
    {
        public override string Name => "addmapinfo";
        public override string Description => "Add a new map info";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public AddMapInfoCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Number)
                .WithRequired(true)
                .WithName("areaid")
                .WithDescription("The Area ID"));


            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Number)
                .WithRequired(true)
                .WithName("mapid")
                .WithDescription("The map ID"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("comps")
                .WithDescription("The recomanded comps"));
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            MapInfoModel model = new MapInfoModel();

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "areaid") model.AreaId = int.Parse(option.Value?.ToString() ?? "0");
                if (option.Name == "mapid") model.MapId = int.Parse(option.Value?.ToString() ?? "0");
                if (option.Name == "comps") model.Reply = option.Value?.ToString() ?? "";
            }

            model.GuildId = command.GuildId;

            MapInfoDatabase.AddMapInfo(model);

            return command.RespondAsync("Map info added !");
        }
    }
}
