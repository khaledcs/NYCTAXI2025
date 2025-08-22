using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    [MetadataType(typeof(DriverVehicleMetaData))]
    public partial class DriverVehicle
    {
    }

    public class DriverVehicleMetaData
    {
        [Required]
        [Display(Name = "Vehicle Type")]
        public int Type { get; set; }

        [Required]
        [Display(Name = "Number of Seats")]
        [RegularExpression("^[2-9]$|^0[2-9]$|^1[0-9]$|^20$", ErrorMessage = "The field Number of Seats should be a value between 2 to 20")]
        public int Seats { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "The field Brand should only contain upto 20 characters")]
        public string Brand { get; set; }

        [Required]
        [Display(Name = "Vehicle Registration Number")]
        [MaxLength(20, ErrorMessage = "The field Vehicle Registration Number should only contain upto 20 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Please enter a valid vehicle registration number")]
        public string VRN { get; set; }

        [Required]
        [Display(Name = "Vehicle Insurance Number")]
        [MaxLength(20, ErrorMessage = "The field Vehicle Registration Identification Number should only contain upto 20 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Please enter a valid vehicle insurance number")]
        public string InsuranceNumber { get; set; }

        [Required]
        [Display(Name = "Driving Licence Number")]
        [MaxLength(10, ErrorMessage = "The field Driving Licence Number should only contain upto 10 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Please enter a valid driving licence number")]
        public string LicenseNumber { get; set; }

        [Required]
        [Display(Name = "National Identity Card (NIC) Number")]
        [MaxLength(12, ErrorMessage = "The field National Identity Card (NIC) Number should only contain upto 12 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Please enter a valid national identity card number")]
        public string NIC { get; set; }
    }
}