using System;
using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    [MetadataType(typeof(ReservationMetaData))]
    public partial class Reservation
    {
        public string FullPickUpAddress { get; set; }
        public string FullDropAddress { get; set; }
    }

    public class ReservationMetaData
    {
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "The field First Name should only contain upto 50 characters")]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "The field First Name should only contain letters")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "The field Last Name should only contain upto 50 characters")]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "The field Last Name should only contain letters")]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The field Phone Number must contain 10 numeric characters")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Pick-Up Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime PickUpDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Pick-Up Time")]
        public TimeSpan PickUpTime { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        public int Vehicle { get; set; }

        [Required]
        [Display(Name = "Full Pick-Up Address")]
        public string FullPickUpAddress { get; set; }

        [Display(Name = "Pick-Up Street No")]
        [MaxLength(10, ErrorMessage = "The field Pick-Up Street No should only contain upto 10 characters")]
        public string PickUpStreetNo { get; set; }

        [Required]
        [Display(Name = "Pick-Up Route")]
        [MaxLength(255, ErrorMessage = "The field Pick-Up Route should only contain upto 255 characters")]
        public string PickUpRoute { get; set; }

        [Required]
        [Display(Name = "Pick-Up City")]
        [MaxLength(50, ErrorMessage = "The field Pick-Up City should only contain upto 50 characters")]
        public string PickUpCity { get; set; }

        [Required]
        [Display(Name = "Pick-Up State / Province")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "The field Pick-Up State / Province should be 2 capital letters")]
        [RegularExpression("^[A-Z]+$", ErrorMessage = "The field Pick-Up State / Province should be 2 capital letters")]
        public string PickUpProvince { get; set; }

        [Display(Name = "Pick-Up Zip Postal Code")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "The field Pick-Up Zip Postal Code should contain 5 numeric characters")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Zip Postal Code")]
        public string PickUpZipCode { get; set; }

        [Required]
        [Display(Name = "Full Destination Address")]
        public string FullDropAddress { get; set; }

        [Display(Name = "Destination Street No")]
        [MaxLength(10, ErrorMessage = "The field Destination Street No should only contain upto 10 characters")]
        public string DropStreetNo { get; set; }

        [Required]
        [Display(Name = "Destination Route")]
        [MaxLength(255, ErrorMessage = "The field Destination Route should only contain upto 255 characters")]
        public string DropRoute { get; set; }

        [Required]
        [Display(Name = "Destination City")]
        [MaxLength(50, ErrorMessage = "The field Destination City should only contain upto 50 characters")]
        public string DropCity { get; set; }

        [Required]
        [Display(Name = "Destination State / Province")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "The field Destination State / Province should be 2 capital letters")]
        [RegularExpression("^[A-Z]+$", ErrorMessage = "The field Destination State / Province should be 2 capital letters")]
        public string DropProvince { get; set; }

        [Display(Name = "Destination Zip Postal Code")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "The field Destination Zip Postal Code should contain 5 numeric characters")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Zip Postal Code")]
        public string DropZipCode { get; set; }

        [Display(Name = "Trip Charge (CAD)")]
        public decimal Charge { get; set; }
    }
}