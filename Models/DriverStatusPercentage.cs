using System;

namespace NYC_Taxi_System.Models
{
    /// <summary>
    /// Model for driver status presentage
    /// </summary>
    public class DriverStatusPercentage
    {
        public DateTime Date { get; set; }
        public int OnlinePercentage { get; set; }
        public int OfflinePercentage { get; set; }
    }
}