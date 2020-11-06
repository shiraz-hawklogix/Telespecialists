using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{
    public class CaseRejectionReasonsViewModel
    {

        public int crr_key { get; set; }
        public string crr_parent_reason { get; set; }
        public string crr_sub_reason { get; set; }
        public bool crr_troubleshoot { get; set; }
        public string crr_users { get; set; }
    }
}
