using System.ComponentModel.DataAnnotations;

namespace AcselApp.Models
{
    public class Registration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Institution { get; set; }

        [Required]
        [MaxLength(50)]
        public string TicketType { get; set; } = string.Empty; // Early Bird, Standard, Student

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        public string? Comments { get; set; }
    }
}
