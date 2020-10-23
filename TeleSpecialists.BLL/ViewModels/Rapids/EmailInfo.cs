using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.Rapids
{
    public sealed class EmailInfo
    {
        public int Id { get; set; }
        public string UId { get; set; }
        public DateTime DateTimeSent { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public int Attachments { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool isRead { get; set; }

    }
}
