using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands.Reminder
{
    public class ReminderModel
    {
        public ulong? ChannelId { get; set; }
        public ulong UserId { get; set; }

        public ReminderType ReminderType { get; set; }

        public DateTime NextReminder { get; set; }

        public string Message { get; set; } = "";

        public string CancelButtonId { get; set; } = "";
    }
}
