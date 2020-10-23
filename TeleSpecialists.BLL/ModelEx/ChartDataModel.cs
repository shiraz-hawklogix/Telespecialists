using System.Collections.Generic;

namespace TeleSpecialists.BLL.Model
{
    public class ChartDataModel
    {
        public string Title { get; set; }
        public string TitleColor { get; set; }
        public int TitleCount { get; set; }
        public int TitleDay { get; set; }

    }

    /// <summary>
    /// Ref https://code.msdn.microsoft.com/Chart-in-MVC-using-Chartjs-5806c814#content
    /// </summary>
    public class Chart
    {
        public string[] labels { get; set; }
        public List<Datasets> datasets { get; set; }
    }

    public class Datasets
    {
        public string label { get; set; }
        public string[] backgroundColor { get; set; }
        public string[] borderColor { get; set; }
        public string borderWidth { get; set; }
        public int[] data { get; set; }
        public bool fill { get; set; }
    }
}
