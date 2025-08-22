using System.ComponentModel.DataAnnotations;

namespace NYC_Taxi_System.Models
{
    [MetadataType(typeof(FeedbackMetaData))]
    public partial class Feedback
    {
    }

    public class FeedbackMetaData
    {
        [Required]
        [Range(0, 5, ErrorMessage = "Rating should be a value between 0 of 5")]
        public double Rating { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(255, ErrorMessage = "The field Comment should only contain upto 255 characters")]
        public string Comment { get; set; }
    }
}