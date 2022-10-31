using Discord.WebSocket;

namespace HachijouBot.Extensions
{
    public class HachijouDiscordSocketClient : DiscordSocketClient
    {
        /// <summary>
        /// Owner of the bot 
        /// </summary>
        public ulong BotOwnerId { get; set; } = 0;
    }
}
