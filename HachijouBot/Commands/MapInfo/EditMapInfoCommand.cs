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
    public class EditMapInfoCommand : Command
    {
        public override string Name => "editmapinfo";
        public override string Description => "Edit existing map info";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;

        public EditMapInfoCommand()
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

            MapInfoModel? mapToEdit = MapInfoDatabase.GetMapInfos(model);

            if (mapToEdit is null) return command.RespondAsync("Map info not found");

            mapToEdit.Reply = model.Reply;
            MapInfoDatabase.SaveMapInfos();

            return command.RespondAsync("Map info edited !");
        }
    }
}
