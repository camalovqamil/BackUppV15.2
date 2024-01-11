using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpp.DAL
{
    public class History
    {
        public string Oid { get; set; } = Guid.NewGuid().ToString();
        public string Model { get; set; }
        public DateTime InputDate { get; set; } = DateTime.Now;
        public int FileSize { get; set; }
        public int FileSizeMB
        {
            get
            {
                return FileSize / 1024;
            }
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool CopyIsSuccess { get; set; }
        public string Result { get; set; }
        public Model CurrentModel { get; set; }
    }
}
