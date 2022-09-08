using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Models
{
    public abstract class PerGuildEntityModel
    {
        public ulong? GuildId { get; set; }
    }
}
