using System;

namespace TeleSpecialists.BLL.ViewModels.Reports
{
    public class PhysicianBillingByShiftViewModel
    {
        public string AssignDate { get; set; }
        public string Schedule { get; set; }
        public string Physician { get; set; }
        public string PhysicianKey { get; set; }
        public int? Open { get; set; }
        public int? WaitingToAccept { get; set; }
        public int? Accepted { get; set; }
        public int? Complete { get; set; }
        public int? CC1_StrokeAlert { get; set; }
        public int? CC1_STAT { get; set; }
        public int? New { get; set; }
        public int? FU { get; set; }
        public int? EEG { get; set; }
        public int? LTM_EEG { get; set; }
        public int? TC { get; set; }
        public int? Not_Seen { get; set; }
        public int? Blast { get; set; }
        public int Total { get; set; }
        public DateTime assign_date { get; set; }
        public decimal Amount { get; set; }
        public string AmountDollar { get; set; }
        public string TotalCases { get; set; }
        public DateTime time_from_calc { get; set; }
        public DateTime time_to_calc { get; set; }
    }
}
