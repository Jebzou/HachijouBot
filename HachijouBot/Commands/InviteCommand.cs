using Discord;
using Discord.WebSocket;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public class InviteCommand : Command
    {
        public override string Name { get => "invite"; }
        public override string Description { get => "Invite this bot to your server"; }

        public InviteCommand() : base()
        {
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            return command.RespondAsync("https://discord.com/api/oauth2/authorize?client_id=1015298562789216348&permissions=139855210560&scope=bot%20applications.commands");
        }
    }
}
