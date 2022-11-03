using Discord.WebSocket;

namespace HachijouBot.Extensions
{
    public static class SocketUserExtensions
    {
        public static bool IsBotOwner(this SocketUser user) => Hachijou.GetInstance().Client.BotOwnerId == user.Id;

        public static bool IsAdmin(this SocketUser user, ulong guildId) => Hachijou.GetInstance().Client.GetGuild(guildId).IsAdmin(user.Id);
    }
}
