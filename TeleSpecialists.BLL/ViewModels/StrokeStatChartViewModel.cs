using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{
    public class StrokeStatChartViewModel
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public int PhysicianBlastCount { get; set; }
        public int NavigatorBlastCount { get; set; }
        public int STATCount { get; set; }
    }
}
