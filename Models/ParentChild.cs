namespace ELearningPlatform.Models
{
    public class ParentChild
    {
        public int Id { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public string ChildId { get; set; } = string.Empty;
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser Parent { get; set; } = null!;
        public virtual ApplicationUser Child { get; set; } = null!;
    }
}