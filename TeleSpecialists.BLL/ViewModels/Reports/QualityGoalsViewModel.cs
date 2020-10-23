using System;
using System.Collections.Generic;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.BLL.ViewModels.Reports
{
    public class QualityGoalsViewModel
    {
        public List<Guid> Facilities { get; set; }
        public List<int?> System { get; set; }
        public List<int?> Regional { get; set; }
        public List<int?> States { get; set; }
        public List<string> QPS { get; set; }
        public string qag_time_frame { get; set; }
        public string fromMonth { get; set; }
        public string toMonth { get; set; }
        public QualityGoalsData obj { get; set; }
        public string ReportType { get; set; }
    }
    public class QualityGoalsData
    {
        public int gd_key { get; set; }
        public string Quater { get; set; }
        public List<string> qag_door_to_TS_notification_ave_minutes { get; set; }
        public List<string> qag_door_to_TS_notification_median_minutes { get; set; }
        public List<string> qag_percent10_min_or_less_activation_EMS { get; set; }
        public List<string> qag_percent10_min_or_less_activation_PV { get; set; }
        public List<string> qag_percent10_min_or_less_activation_Inpt { get; set; }
        public List<string> qag_TS_notification_to_response_average_minute { get; set; }
        public List<string> qag_TS_notification_to_response_median_minute { get; set; }
        public List<string> qag_percent_TS_at_bedside_grterthan10_minutes { get; set; }
        public List<string> qag_alteplase_administered { get; set; }
        public List<string> qag_door_to_needle_average { get; set; }
        public List<string> qag_door_to_needle_median { get; set; }
        public List<string> qag_verbal_order_to_administration_average_minutes { get; set; }
        public List<string> qag_DTN_grter_or_equal_30minutes_percent { get; set; }
        public List<string> qag_DTN_grter_or_equal_45minutes_percent { get; set; }
        public List<string> qag_DTN_grter_or_equal_60minutes_percent { get; set; }
        public List<string> qag_TS_notification_to_needle_grter_or_equal_30minutes_percent { get; set; }
        public List<string> qag_TS_notification_to_needle_grter_or_equal_45minutes_percent { get; set; }
        public List<string> qag_TS_notification_to_needle_grter_or_equal_60minutes_percent { get; set; }
    }
}
