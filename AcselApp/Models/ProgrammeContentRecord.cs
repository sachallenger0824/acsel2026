using System.ComponentModel.DataAnnotations;

namespace AcselApp.Models
{
    public sealed class ProgrammeContentRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ContentJson { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
