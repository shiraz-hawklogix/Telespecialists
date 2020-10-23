using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class QualityGoalscls
    {
        public int qag_key { get; set; }
        public Guid qag_fac_key { get; set; }
        public string facility { get; set; }
        public string QualityMetrics { get; set; }
        public string qag_time_frame { get; set; }
        public int gd_key { get; set; }
        public string qag_door_to_TS_notification_ave_minutes { get; set; }
        public string qag_door_to_TS_notification_median_minutes { get; set; }
        public string qag_percent10_min_or_less_activation_EMS { get; set; }
        public string qag_percent10_min_or_less_activation_PV { get; set; }
        public string qag_percent10_min_or_less_activation_Inpt { get; set; }
        public string qag_TS_notification_to_response_average_minute { get; set; }
        public string qag_TS_notification_to_response_median_minute { get; set; }
        public string qag_percent_TS_at_bedside_grterthan10_minutes { get; set; }
        public string qag_alteplase_administered { get; set; }
        public string qag_door_to_needle_average { get; set; }
        public string qag_door_to_needle_median { get; set; }
        public string qag_verbal_order_to_administration_average_minutes { get; set; }
        public string qag_DTN_grter_or_equal_30minutes_percent { get; set; }
        public string qag_DTN_grter_or_equal_45minutes_percent { get; set; }
        public string qag_DTN_grter_or_equal_60minutes_percent { get; set; }
        public string qag_TS_notification_to_needle_grter_or_equal_30minutes_percent { get; set; }
        public string qag_TS_notification_to_needle_grter_or_equal_45minutes_percent { get; set; }
        public string qag_TS_notification_to_needle_grter_or_equal_60minutes_percent { get; set; }
    }
}
