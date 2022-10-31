using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Models
{
    public interface IDataBase
    {
        public DataTable GetData();
    }
}
