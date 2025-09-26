using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public enum ContentType
    {
        Text = 1,
        PDF = 2,
        Video = 3,
        Link = 4
    }

    public class Content
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ContentType Type { get; set; }

        public string? TextContent { get; set; }

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        public int ChapterId { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual Chapter Chapter { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
    }
}