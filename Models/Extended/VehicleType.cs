using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    [MetadataType(typeof(VehicleTypeMetaData))]
    public partial class VehicleType
    {
    }

    public class VehicleTypeMetaData
    {
        [Required]
        [Display(Name = "Vehicle Type")]
        [MaxLength(20, ErrorMessage = "The field Vehicle Type should only contain upto 20 characters")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Vehicle Rate (CAD)")]
        public decimal Rate { get; set; }
    }
}