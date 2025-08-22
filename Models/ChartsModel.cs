using DotNet.Highcharts;

namespace NYC_Taxi_System.Models
{
    /// <summary>
    /// Model for charts
    /// </summary>
    public class ChartsModel
    {
        public Highcharts ColumnChart { get; set; }
        public Highcharts BarChart { get; set; }
        public Highcharts AreaChart { get; set; }
        public Highcharts LineChart { get; set; }
        public Highcharts AreasplineChart { get; set; }
        public Highcharts SplineChart { get; set; }
        public Highcharts PieChart { get; set; }
        public Highcharts ScatterChart { get; set; }
    }
}