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

        public CustomCommand(string name, string description, string reply)
        {
            CustomName = name;
            CustomDescription = description;
            TextToReturn = reply;
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            return command.RespondAsync(TextToReturn);
        }
    }
}
