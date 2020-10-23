using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.Models
{
    public class AjaxResult
    {
        public object data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public string redirectUrl { get; set; }
        public bool refershPage { get; set; }

    }
}