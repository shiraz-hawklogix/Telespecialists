using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class tPACaseAnalysis
    {
        public string Facility { get; set; }
        public int CaseNumber { get; set; }
        public int Case_Key { get; set; }
        public string Date { get; set; }
        public string Process { get; set; }
        public string DTN { get; set; }
        public string tPAdelaynotes { get; set; }
        public string QPSanalysis { get; set; }
        public string MedicalDirectorAnalysis { get; set; }
        public string RootCauseGrp { get; set; }
        public int RootId { get; set; }
        public string RootCause { get; set; }
        public int RootCauseCount { get; set; }
    }
}
