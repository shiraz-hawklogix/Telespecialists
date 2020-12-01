using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.API.ViewModels
{
    public class SaveToken
    {
        public string phy_key { get; set; }
        public string token { get; set; }
        public string deviceType { get; set; }
    }
}