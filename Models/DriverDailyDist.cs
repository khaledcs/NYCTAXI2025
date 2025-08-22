using System;
using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    /// <summary>
    /// Model for driver daily distance
    /// </summary>
    public class DriverDailyDist
    {
        public DateTime Date { get; set; }

        [Display(Name = "Distance converted as revenue for the day")]
        public decimal OnHire { get; set; }

        [Display(Name = "Full distance covered for the day")]
        public decimal Full { get; set; }
    }
}