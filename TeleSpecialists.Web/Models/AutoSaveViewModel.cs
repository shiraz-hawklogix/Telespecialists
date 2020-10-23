using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.Models
{
    public class AutoSaveViewModel
    {
        public int Id { get; set; }
        public int TemplateKey { get; set; }
        public int TemplateEntityType { get; set; }
        public string TemplateKeyName { get; set; }
        public string FacilityTimeZone { get; set; }
        public List<AutoSaveDictionary> FormData {get;set;}
    }

    public class AutoSaveDictionary
    {      
        public string Key { get; set; }
        public string Value { get; set; }
        public bool  SaveAsUTC { get; set; }
        public string PreviousValue { get; set; }
    }
}