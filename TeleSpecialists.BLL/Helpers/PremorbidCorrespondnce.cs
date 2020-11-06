using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class PremorbidCorrespondnce
    {
        public string pmc_cas_premorbid_patient_phone { get; set; }

        public List<string> pmc_cas_premorbid_datetime_of_contact { get; set; }
        public List<int?> pmc_cas_premorbid_spokewith { get; set; }
        public List<string> pmc_cas_premorbid_comments { get; set; }
        public int? pmc_cas_premorbid_successful_or_unsuccessful_first { get; set; }
        public int? pmc_cas_premorbid_successful_or_unsuccessful_second { get; set; }
        public int? pmc_cas_premorbid_successful_or_unsuccessful_third { get; set; }
        public List<string> pmc_cas_premorbid_completedby { get; set; }
        public int? pmc_cas_patient_satisfaction_video_experience { get; set; }
        public int? pmc_cas_patient_satisfaction_communication { get; set; }
        public int? pmc_cas_willing_todo_interview { get; set; }
        public int? pmc_cas_consent_sent { get; set; }
        public int? pmc_cas_consent_received { get; set; }
    }
}
