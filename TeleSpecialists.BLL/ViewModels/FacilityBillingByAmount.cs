using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.Reports;

namespace TeleSpecialists.BLL.ViewModels.Reports
{
    public class FacilityBillingByAmount
    {
        public string AssignDate { get; set; }
        public string Facility { get; set; }
        public string FacilityKey { get; set; }
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
        public int? STAT_EEG { get; set; }
        public int? LTM_VIDEO { get; set; }
        public int? EEG_MINUTES { get; set; }
        public int? Not_Seen { get; set; }
        public int? Blast { get; set; }
        public int Total { get; set; }
        public DateTime assign_date { get; set; }
        public decimal Amount { get; set; }
        public string AmountDollar { get; set; }
        public string CC1_StrokeAlertstring { get; set; }
        public string CC1_STATstring { get; set; }
        public string Newstring { get; set; }
        public string FUstring { get; set; }
        public string EEGstring { get; set; }
        public string LTM_EEGstring { get; set; }
        public string TCstring { get; set; }
        public string STAT_EEGstring { get; set; }
        public string LTM_VIDEOstring { get; set; }
        public string EEG_MINUTESstring { get; set; }
        public string Amountstring { get; set; }

    }
}
