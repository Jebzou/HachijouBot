namespace HachijouBot.Common
{
    public static class EmoteDataBase
    {
        public static Dictionary<string, string> Emotes { get; } = new()
        {
            { "kcfuel", "" },
            { "kcammo", "" },
            { "kcsteel", "" },
            { "kcbauxite", "" },
            { "Screw", "" },
            { "DevMats", "" },
        };

        public static string DevMats => Emotes["DevMats"];
        public static string Screws => Emotes["Screw"];
        public static string Fuel => Emotes["kcfuel"];
        public static string Ammo => Emotes["kcammo"];
        public static string Steel => Emotes["kcsteel"];
        public static string Bauxite => Emotes["kcbauxite"];
    }
}
