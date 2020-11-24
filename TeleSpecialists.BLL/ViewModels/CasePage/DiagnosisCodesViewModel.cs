using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.CasePage
{
    public class Icd10Codes
    {
        public List<DiagnosisCodesViewModel> _DiagnosisCodesViewModel { get; set; }
        public Icd10CodeCalculator _Icd10CodeCalculator { get; set; }
    } 

    public class DiagnosisCodesViewModel
    {
        public int Id { get; set; }
        public string icd_code { get; set; }
        public string title { get; set; }        
    }

    public class Icd10CodeCalculator
    {
        public int Id { get; set; }
        public string code_name { get; set; }
        public string name { get; set; }
        public string class_name { get; set; }
        public List<Icd10CodeChilds> _icd10CodeChilds { get; set; }
    }

    public class Icd10CodeChilds
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string class_name { get; set; }
        public int? sort_order { get; set; }
    }

    public class Icd10SearchKeys
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Icd10CodeCalParent
    {
        public int Id { get; set; }
        public string cod_name { get; set; }
        public string cod_class_name { get; set; }
        public int? cod_sort_order { get; set; }
    }
}
