using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Models
{
    public class MapInfoModel : PerGuildEntityModel
    {
        public int AreaId { get; set; }

        public int MapId { get; set; }

        public string Reply { get; set; } = "";
    }
}
