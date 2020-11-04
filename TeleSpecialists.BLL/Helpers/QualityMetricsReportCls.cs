using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    class QualityMetricsReportCls
    {
        public string reportname { get; set; }
        public int hospitals { get; set; }
        public TimeSpan _meantime { get; set; }
        public TimeSpan _mediantime { get; set; }
        //public List<Guid> hospitalid { get; set; }
    }
}
