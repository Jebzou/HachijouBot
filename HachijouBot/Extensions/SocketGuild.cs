using Discord;
using Discord.WebSocket;

namespace HachijouBot.Extensions
{
    public static class SocketGuildExtensions
    {
        public static bool IsAdmin(this SocketGuild guild, ulong userId) 
        {
            SocketGuildUser user = guild.GetUser(userId);

            return user.GuildPermissions.Administrator;
        }

        public static bool IsAdmin(this IGuildUser user) => user.GuildPermissions.Administrator;
    }
}
