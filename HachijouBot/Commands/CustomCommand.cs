using Discord.WebSocket;

namespace HachijouBot.Commands
{
    public class CustomCommand : Command
    {
        public override string Name { get => CustomName; }
        public override string Description { get => CustomDescription; }

        public string CustomName { get; set; }
        public string CustomDescription { get; set; }

        public string TextToReturn { get; set; } = "";

        public ulong GuildId { get; set; }

        public CustomCommand(string name, string description, string reply, ulong guildId)
        {
            CustomName = name;
            CustomDescription = description;
            TextToReturn = reply;
            GuildId = guildId;
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            if (command.GuildId != GuildId) return command.RespondAsync("Command not found");

            return command.RespondAsync(TextToReturn);
        }
    }
}
