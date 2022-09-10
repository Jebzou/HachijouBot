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
    public class GetMapInfoCommand : Command
    {
        public override string Name => "getmapinfo";
        public override string Description => "Get map info";

        public GetMapInfoCommand()
        {
            InitOptions();
            MapInfoDatabase.OnMapInfoAdd += MapInfoDatabase_OnMapInfoAdd;
        }

        private void MapInfoDatabase_OnMapInfoAdd(object? sender, MapInfoModel e)
        {
            InitOptions();
            TriggerOptionsChanged();
        }

        public override void InitOptions()
        {
            base.InitOptions();

            SlashCommandOptionBuilder areas = new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Number)
                .WithRequired(true)
                .WithName("areaid")
                .WithDescription("The Area ID");

            MapInfoDatabase.LoadMapInfos();

            foreach (int areaId in MapInfoDatabase.MapsLoaded.Select(x => x.AreaId).Distinct())
            {
                areas.AddChoice(areaId.ToString(), areaId);
            }

            SlashCommandOptionBuilder maps = new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Number)
                .WithRequired(true)
                .WithName("mapid")
                .WithDescription("The map ID");


            foreach (int mapId in MapInfoDatabase.MapsLoaded.Select(x => x.MapId).Distinct())
            {
                maps.AddChoice(mapId.ToString(), mapId);
            }

            Options.Add(areas);
            Options.Add(maps);
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            MapInfoModel model = new MapInfoModel();

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "areaid") model.AreaId = int.Parse(option.Value?.ToString() ?? "0");
                if (option.Name == "mapid") model.MapId = int.Parse(option.Value?.ToString() ?? "0");
            }

            model.GuildId = command.GuildId;

            MapInfoModel? mapInfo = MapInfoDatabase.GetMapInfos(model);

            if (mapInfo != null) return command.RespondAsync(mapInfo.Reply);
            return command.RespondAsync("Map info not found");
        }
    }
}
