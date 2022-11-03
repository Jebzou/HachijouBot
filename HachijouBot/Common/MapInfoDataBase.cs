using HachijouBot.Commands;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.Common
{
    public class MapInfoDatabase : IDataBase
    {
        private const string Path = "MapInfo.json";

        public static List<MapInfoModel> MapsLoaded = new List<MapInfoModel>();

        public static event EventHandler<MapInfoModel>? OnMapInfoAdd;

        public static void AddMapInfo(MapInfoModel mapInfoToAdd)
        {
            MapsLoaded.Add(mapInfoToAdd);
            SaveMapInfos();

            OnMapInfoAdd?.Invoke(null, mapInfoToAdd);
        }

        public static void LoadMapInfos()
        {
            MapsLoaded.Clear();

            if (!File.Exists(Path)) return;

            MapsLoaded = JsonHelper.ReadJson<List<MapInfoModel>>(Path);
        }

        public static void SaveMapInfos()
        {
            JsonHelper.WriteJson(Path, MapsLoaded);
        }

        public static MapInfoModel? GetMapInfos(MapInfoModel baseModel)
        {
            if (MapsLoaded.Count == 0) LoadMapInfos();

            MapInfoModel? val = MapsLoaded.Find(model => model.AreaId == baseModel.AreaId && model.MapId == baseModel.MapId && model.GuildId == baseModel.GuildId);

            return val;
        }

        public DataTable GetData(ulong guildId)
        {
            return GetData(MapsLoaded.Where(c => c.GuildId == guildId).ToList());
        }

        public DataTable GetData()
        {
            return GetData(MapsLoaded);
        }

        private DataTable GetData(List<MapInfoModel> commands)
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Area ID");
            results.Columns.Add("Map ID");
            results.Columns.Add("Reply");

            foreach (MapInfoModel map in commands)
            {
                string guildName = "";
                
                if (map.GuildId != null) guildName = Hachijou.GetInstance().Client.GetGuild((ulong)map.GuildId).Name;
                
                results.Rows.Add(guildName, map.AreaId, map.MapId, map.Reply);
            }

            return results;
        }
    }
}
