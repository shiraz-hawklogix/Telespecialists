using System.Collections.Generic;
using Kendo.DynamicLinq;


namespace TeleSpecialists.Helpers
{
    public class DataSourceRequest
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public IEnumerable<Sort> Sort { get; set; }
        public Filter Filter { get; set; }
    }
}
