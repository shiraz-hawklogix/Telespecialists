using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TeleSpecialists.BLL.ViewModels
{
    public class FireBaseData
    {
        public string user_id { get; set; }
        public int teleid { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string UserName { get; set; }
        public string ImgPath { get; set; }
        //public FileContentResult ImgPath { get; set; }

    }
}