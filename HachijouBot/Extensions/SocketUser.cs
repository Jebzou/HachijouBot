using Discord.WebSocket;

namespace HachijouBot.Extensions
{
    public static class SocketUserExtensions
    {
        public static bool IsBotOwner(this SocketUser user) => Hachijou.GetInstance().Client.BotOwnerId == user.Id;
    }
}
