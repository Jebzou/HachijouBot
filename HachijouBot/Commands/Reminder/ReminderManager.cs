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
            if (reminder.ChannelId is ulong channelId)
            {
                SocketUser user = Hachijou.GetInstance().Client.GetUser(reminder.UserId);
                SocketTextChannel channel = (SocketTextChannel)Hachijou.GetInstance().Client.GetChannel(channelId);
                await channel.SendMessageAsync($"{user.Mention} Reminder : {reminder.Message}");
            }
            else
            {
                SocketUser user = Hachijou.GetInstance().Client.GetUser(reminder.UserId);
                await user.SendMessageAsync($"Reminder : {reminder.Message}");
            }
        }
    }
}
