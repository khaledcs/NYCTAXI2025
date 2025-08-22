using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    /// <summary>
    /// Model for Reset Password
    /// </summary>
    public class ResetPassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [MinLength(8, ErrorMessage = "The field New Password must contain at least 8 characters")]
        [MaxLength(50, ErrorMessage = "The field New Password should only contain upto 50 characters")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }

        [Required]
        [Display(Name = "Reset Code")]
        public System.Guid ResetCode { get; set; }
    }
}