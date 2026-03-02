using System.ComponentModel.DataAnnotations;

namespace AcselApp.Models
{
    public class UpdateNewsItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
