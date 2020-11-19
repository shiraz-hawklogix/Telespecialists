using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{
    public class UserPresenceListings
    {
        public string Id { get; set; }
        public string CreatedDate { get; set; }
        public string date { get; set; }
        public string Physician { get; set; }
        public int Available { get; set; }
        public string AvailableS { get; set; }
        public int TPA { get; set; }
        public string TPAS { get; set; }
        public int StrokeAlert { get; set; }
        public string StrokeAlertS { get; set; }
        public int Rounding { get; set; }
        public string RoundingS { get; set; }
        public int STATConsult { get; set; }
        public string STATConsultS { get; set; }
        public int Break { get; set; }
        public string BreakS { get; set; }
        public long PhysicianId { get; set; }
    }
    public class UserPresenceGraph {
        public string Id { get; set; }
        public string CreatedDate { get; set; }
        public string Physician { get; set; }
        public string StatusName { get; set; }
        public int diff { get; set; }
        public string StartTime { get; set; }
    }

}
