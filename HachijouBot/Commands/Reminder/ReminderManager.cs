using Discord;
using Discord.WebSocket;

namespace HachijouBot.Commands.Reminder
{
    public class ReminderManager
    {
        public ReminderManager()
        {
            ReminderDatabase.LoadReminders();

            System.Timers.Timer timer = new(6e4);

            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;


            Hachijou.GetInstance().Client.ButtonExecuted += Client_ButtonExecuted;
        }

        private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (ReminderModel reminder in ReminderDatabase.RemindersLoaded.ToList())
            {
                if (reminder.NextReminder <= DateTime.UtcNow)
                {
                    await TriggerReminder(reminder);

                    switch (reminder.ReminderType)
                    {
                        case ReminderType.Once:
                            ReminderDatabase.DeleteReminder(reminder);
                            break;
                        case ReminderType.Daily:
                            reminder.NextReminder = reminder.NextReminder.AddDays(1);
                            break;
                        case ReminderType.Weekly:
                            reminder.NextReminder = reminder.NextReminder.AddDays(7);
                            break;
                        case ReminderType.Monthly:
                            reminder.NextReminder = reminder.NextReminder.AddMonths(1);
                            break;
                    }


                }

            }
        }

        private async Task TriggerReminder(ReminderModel reminder)
        {
            ComponentBuilder builder = new ComponentBuilder();

            if (reminder.ReminderType != ReminderType.Once)
            {
                reminder.CancelButtonId = $"reminder-delete-button-{Guid.NewGuid()}";
                builder.WithButton("Stop reminding me", style: ButtonStyle.Danger, customId: reminder.CancelButtonId);
            }

            MessageComponent cancelButton = builder.Build();

            if (reminder.ChannelId is ulong channelId)
            {
                SocketUser user = Hachijou.GetInstance().Client.GetUser(reminder.UserId);
                SocketTextChannel channel = (SocketTextChannel)Hachijou.GetInstance().Client.GetChannel(channelId);
                await channel.SendMessageAsync($"{user.Mention} Reminder : {reminder.Message}", components: cancelButton);
            }
            else
            {
                SocketUser user = Hachijou.GetInstance().Client.GetUser(reminder.UserId);
                await user.SendMessageAsync($"Reminder : {reminder.Message}", components: cancelButton);
            }
        }

        private async Task Client_ButtonExecuted(SocketMessageComponent arg)
        {
            if (!arg.Data.CustomId.StartsWith("reminder-delete-button-")) return;

            // get the reminder model
            ReminderModel? reminder = ReminderDatabase.RemindersLoaded.Find(r => r.CancelButtonId == arg.Data.CustomId);

            if (reminder is null) return;

            if (reminder.UserId != arg.User.Id) return;

            ReminderDatabase.DeleteReminder(reminder);

            await arg.UpdateAsync(m =>
            {
                m.Content = $"{arg.Message}\n\r Reminder removed";
                m.Components = null;
            });
        }
    }
}
