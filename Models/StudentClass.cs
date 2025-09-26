namespace ELearningPlatform.Models
{
    public class StudentClass
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser Student { get; set; } = null!;
        public virtual Class Class { get; set; } = null!;
    }
}