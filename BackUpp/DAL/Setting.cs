using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpp.DAL
{
    public class Setting
    {
        public string Oid { get; set; } = Guid.NewGuid().ToString();
        public bool AutoStart { get; set; }
        public bool FullBackupp { get; set; }
        public bool DiferensialBackupp { get; set; }
    }
}
