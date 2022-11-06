using Discord;
using Discord.WebSocket;

namespace HachijouBot.Commands.Reminder
{
    public class AddReminderCommand : Command
    {
        public override string Name => "addreminder";
        public override string Description => "Set a new reminder (UTC time)";

        public AddReminderCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .WithName("day")
                .WithDescription("Reminder day"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .WithName("month")
                .WithDescription("Reminder month"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .WithName("year")
                .WithDescription("Reminder year"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .WithName("hour")
                .WithDescription("Reminder hour"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .WithName("minute")
                .WithDescription("Reminder minute"));

            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .WithName("type")
                .WithDescription("Reminder type")
                .AddChoice(ReminderType.Once.ToString(), (int)ReminderType.Once)
                .AddChoice(ReminderType.Daily.ToString(), (int)ReminderType.Daily)
                .AddChoice(ReminderType.Weekly.ToString(), (int)ReminderType.Weekly)
                .AddChoice(ReminderType.Monthly.ToString(), (int)ReminderType.Monthly));



            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("message")
                .WithDescription("Message"));
        }

        public override Task CommandHandler(SocketSlashCommand command)
        {
            ReminderModel model = new();
            model.NextReminder = new DateTime(1,1,1,0,0,0);

            foreach (var option in command.Data.Options)
            {
                if (option.Name == "day") model.NextReminder = model.NextReminder.AddDays(int.Parse(option.Value?.ToString() ?? "0") - 1);
                if (option.Name == "month") model.NextReminder = model.NextReminder.AddMonths(int.Parse(option.Value?.ToString() ?? "0") - 1);
                if (option.Name == "year") model.NextReminder = model.NextReminder.AddYears(int.Parse(option.Value?.ToString() ?? "0") - 1);

                if (option.Name == "hour") model.NextReminder = model.NextReminder.AddHours(int.Parse(option.Value?.ToString() ?? "0"));
                if (option.Name == "minute") model.NextReminder = model.NextReminder.AddMinutes(int.Parse(option.Value?.ToString() ?? "0"));

                if (option.Name == "type") model.ReminderType = (ReminderType)int.Parse(option.Value?.ToString() ?? "0");
                if (option.Name == "message") model.Message = option.Value?.ToString() ?? "";
            }

            model.UserId = command.User.Id;
            model.ChannelId = command.ChannelId;

            ReminderDatabase.AddReminder(model);

            return command.RespondAsync("Reminder set !");
        }
    }
}
