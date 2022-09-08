using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract Task CommandHandler(SocketSlashCommand command);

        public List<SlashCommandOptionBuilder> Options { get; set; } = new List<SlashCommandOptionBuilder>();

        public virtual GuildPermission? GuildPermission { get; set; }

    }
}
