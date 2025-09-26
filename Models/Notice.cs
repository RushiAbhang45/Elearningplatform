using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class Notice
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? ClassId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual Class? Class { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
    }
}