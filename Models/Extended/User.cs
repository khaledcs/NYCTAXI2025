using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    /// <summary>
    /// Partial class of User model
    /// </summary>
    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Validation for the fields in the User model
    /// </summary>
    public class UserMetaData
    {
        [Required]
        [Display(Name = "Username")]
        [MinLength(3, ErrorMessage = "The field Username should at least contain 3 characters")]
        [MaxLength(50, ErrorMessage = "The field Username should only contain upto 50 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "The field Username should only contain letters and numbers")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "The field First Name should only contain upto 50 characters")]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "The field First Name should only contain letters")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "The field Last Name should only contain upto 50 characters")]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "The field Last Name should only contain letters")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The field Phone Number must contain 10 numeric characters")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(255, ErrorMessage = "The field Email should only contain upto 255 characters")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "The field Password must contain at least 8 characters")]
        [MaxLength(50, ErrorMessage = "The field Password should only contain upto 50 characters")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Account Status")]
        public bool IsEmailVerified { get; set; }
    }
}