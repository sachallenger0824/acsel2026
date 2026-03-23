using System.ComponentModel.DataAnnotations;

namespace AcselApp.Models
{
    public class Registration
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Institution / Organization is required.")]
        [MaxLength(200)]
        public string? Institution { get; set; }

        [Required(ErrorMessage = "Title / Position is required.")]
        [MaxLength(100)]
        public string? TitlePosition { get; set; }

        [Required(ErrorMessage = "Sightseeing Tour selection is required.")]
        [MaxLength(50)]
        public string? SightseeingTour { get; set; }

        [Required(ErrorMessage = "Technical Tour selection is required.")]
        [MaxLength(50)]
        public string? TechnicalTour { get; set; }

        [Required(ErrorMessage = "Registration Fee Category is required.")]
        [MaxLength(50)]
        public string TicketType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment Method is required.")]
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        public string? PaymentLink { get; set; }

        public string? Comments { get; set; }
    }
}
