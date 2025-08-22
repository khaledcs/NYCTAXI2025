using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    [MetadataType(typeof(DriverLocationMetaData))]
    public partial class DriverLocation
    {
        public string FullAddress { get; set; }
    }

    public class DriverLocationMetaData
    {
        [Required]
        [Display(Name = "Full Address")]
        public string FullAddress { get; set; }

        [Display(Name = "Street No")]
        [MaxLength(10, ErrorMessage = "The field Street No should only contain upto 10 characters")]
        public string StreetNo { get; set; }

        [Required]
        [Display(Name = "Route")]
        [MaxLength(255, ErrorMessage = "The field Route should only contain upto 255 characters")]
        public string Route { get; set; }

        [Required]
        [Display(Name = "City")]
        [MaxLength(50, ErrorMessage = "The field City should only contain upto 50 characters")]
        public string City { get; set; }

        [Required]
        [Display(Name = "State / Province")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "The field State / Province should be 2 capital letters")]
        [RegularExpression("^[A-Z]+$", ErrorMessage = "The field State / Province should be 2 capital letters")]
        public string Province { get; set; }

        [Display(Name = "Zip Postal Code")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "The field Zip Postal Code should contain 5 numeric characters")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Zip Postal Code")]
        public string ZipCode { get; set; }
    }
}