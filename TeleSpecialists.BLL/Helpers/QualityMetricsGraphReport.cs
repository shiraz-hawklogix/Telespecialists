using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    class QualityMetricsGraphReport
    {
        public string Title { get; set; }
        public string Mean { get; set; }
        public string Median { get; set; }
        public string MinDate { get; set; }
        public List<string> MeanCalculation { get; set; }
        public List<string> MedianCalculation { get; set; }
        public List<string> Category { get; set; }
    }
}
