using System.ComponentModel.DataAnnotations;

namespace AcselApp.Models
{
    public class AbstractSubmission
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string PresentationType { get; set; } = "No Preference";

        [Required, MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Authors { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Affiliations { get; set; }

        [Required, MaxLength(200)]
        public string CorrespondingAuthor { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string CorrespondingEmail { get; set; } = string.Empty;

        [Required]
        public string AbstractText { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Keywords { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
    }
}
