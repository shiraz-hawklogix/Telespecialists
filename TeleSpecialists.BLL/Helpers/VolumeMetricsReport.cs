using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class VolumeMetricsReport
    {
        public string PhysicianKey { get; set; }
        public string PhysicianName { get; set; }
        public string TimeCycle { get; set; }
        public int StrokeAlert { get; set; }
        public int STAT { get; set; }
        public int New { get; set; }
        public int FollowUp { get; set; }
        public int EEG { get; set; }
        public string Name { get; set; }
        public List<string> Category { get; set; }
        public List<int> DataList { get; set; }
        public List<string> Datalist { get; set; }
        public string  xAxisLabel { get; set; }
        public int EMS { get; set; }
        public string EMSPercent { get; set; }
        public int Triage { get; set; }
        public string TriagePercent { get; set; }
        public int Inpatient { get; set; }
        public string InpatientPercent { get; set; }
        public int EDOnset { get; set; }
        public string EDOnsetPercent { get; set; }
        public int TotalCases { get; set; }
        public long PhysicianId { get; set; }
    }
}
