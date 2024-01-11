using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpp.DAL
{
    public class Model
    {
        public string Oid { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string FromFolder { get; set; }
        public string ToFolder { get; set; }
        public int SaveDay { get; set; }
        public int CopyType { get; set; }
        public string CopyTypeName
        {
            get
            {
                return CopyType == 1 ? "Gündə bir dəfə" : "Saat intervalı";
            }
        }
        public string RunTime { get; set; }
        public string StartTime { get; set; }
        public string EndTame { get; set; }
        public int Interval { get; set; }
        public string Note { get; set; }
        public bool DefualtModel { get; set; }
        public string Telephone { get; set; }
        public bool SendSMS { get; set; }
        public DateTime InputDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
