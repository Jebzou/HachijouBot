using HachijouBot.Commands;
using HachijouBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Common
{
    public static class MapInfoDatabase
    {
        private const string Path = "MapInfo.json";

        public static List<MapInfoModel> MapsLoaded = new List<MapInfoModel>();

        public static event EventHandler<MapInfoModel>? OnMapInfoAdd;

        public static void AddMapInfo(MapInfoModel mapInfoToAdd)
        {
            MapsLoaded.Add(mapInfoToAdd);
            JsonHelper.WriteJson(Path, MapsLoaded);

            OnMapInfoAdd?.Invoke(null, mapInfoToAdd);
        }

        public static void LoadMapInfos()
        {
            MapsLoaded.Clear();

            if (!File.Exists(Path)) return;

            MapsLoaded = JsonHelper.ReadJson<List<MapInfoModel>>(Path);
        }

        public static MapInfoModel? GetMapInfos(MapInfoModel baseModel) 
        {
            if (MapsLoaded.Count == 0) LoadMapInfos();

            MapInfoModel? val = MapsLoaded.Find(model => model.AreaId == baseModel.AreaId && model.MapId == baseModel.MapId && model.GuildId == baseModel.GuildId);

            return val;
        }
    }
}
