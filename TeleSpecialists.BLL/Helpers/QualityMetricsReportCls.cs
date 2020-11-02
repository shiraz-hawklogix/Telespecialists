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
        public string  Navigator { get; set; }
        public string timeframe { get; set; }
        public string NavigatorID { get; set; }
        //public List<Guid> hospitalid { get; set; }
    }
}
