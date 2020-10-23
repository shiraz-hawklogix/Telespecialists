using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class HourlyMeanReport
    {
        public string TimeCycle { get; set; }
        public double StrokeAlert { get; set; }
        public double STAT { get; set; }
        public string Name { get; set; }
        public List<string> Category { get; set; }
        public List<double> DataList { get; set; }
        public string xAxisLabel { get; set; }
    }
}
