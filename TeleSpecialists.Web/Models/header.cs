using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.Models
{
    public class header
    {
        public string HeaderName { get; set; }
        public string headerBody { get; set; }
    }
    public class HeaderListDetail
    {
        public List<header> Records { get; set; }
        

        //public List<header> Records
        //{
        //    get { return Records; }
        //    set { myVar = value; }
        //}

    }
}