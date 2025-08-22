using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    /// <summary>
    /// Model for driver daily revenue
    /// </summary>
    public class DriverMonthlyRev
    {
        public int Month { get; set; }

        [Display(Name = "Month")]
        public string MonthInText { get; set; }

        [Display(Name = "Total income for the month")]
        public decimal Income { get; set; }

        [Display(Name = "Total commission for the month")]
        public decimal Commission { get; set; }
    }
}