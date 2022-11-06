using Discord;
using HachijouBot.Common;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.Commands.Reminder
{
    public class ReminderDatabase : IDataBase
    {
        private const string CommandPath = "Reminders.json";

        public static List<ReminderModel> RemindersLoaded = new List<ReminderModel>();

        public static void DeleteReminder(ReminderModel reminder)
        {
            bool deleted = RemindersLoaded.Remove(reminder);

            if (!deleted)
            {
                throw new Exception("Remidner not found");
            }

            SaveChanges();
        }

        public static void AddReminder(ReminderModel remidnerToAdd)
        {
            RemindersLoaded.Add(remidnerToAdd); 
            SaveChanges();
        }

        public static void LoadReminders()
        {
            RemindersLoaded.Clear();

            if (!File.Exists(CommandPath)) return;

            RemindersLoaded = JsonHelper.ReadJson<List<ReminderModel>>(CommandPath);
        }

        public static void SaveChanges()
        {
            JsonHelper.WriteJson(CommandPath, RemindersLoaded);
        }

        public DataTable GetData(ulong guildId)
        {
            return GetData(RemindersLoaded
                .Where(c => c.ChannelId != null)
                .Where(c => ((ITextChannel)Hachijou.GetInstance().Client.GetChannel(c.ChannelId ?? 0)).GuildId == guildId)
                .ToList()
            ).Result;
        }

        public DataTable GetData()
        {
            return GetData(RemindersLoaded).Result;
        }

        private async Task<DataTable> GetData(List<ReminderModel> reminders)
        {
            DataTable results = new DataTable();

            results.Columns.Add("Guild");
            results.Columns.Add("Channel");
            results.Columns.Add("User");
            results.Columns.Add("Type");
            results.Columns.Add("Next reminder");
            results.Columns.Add("Message");

            foreach (ReminderModel reminder in reminders)
            {
                string guildName = "Global";
                string channelName = "";
                string userName;

                if (reminder.ChannelId != null)
                {
                    ITextChannel channel = (ITextChannel)Hachijou.GetInstance().Client.GetChannel((ulong)reminder.ChannelId);
                    channelName = channel.Name;
                    guildName = channel.Guild.Name;
                    IGuildUser user = await channel.Guild.GetUserAsync(reminder.UserId);
                    userName = user.DisplayName;
                }
                else
                {
                    userName = Hachijou.GetInstance().Client.GetUser(reminder.UserId).Username;
                }



                results.Rows.Add(guildName, channelName, userName, reminder.ReminderType.ToString(), reminder.NextReminder.ToString("dd/MM/yyyy hh:mm:ss"), reminder.Message);
            }

            return results;
        }
    }
}