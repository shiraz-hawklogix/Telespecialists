using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.Models
{
    public class SignalRCaseDataModel
    {

        public int cas_key { get; set; }
        public string cas_response_first_atempt { get; set; }
        public bool IsLoginDelayRequired { get; set; }
        public bool cas_phy_has_technical_issue { get; set; }
        public string cas_phy_key { get; set; }
        public int cas_cst_key { get; set; }
    }
}