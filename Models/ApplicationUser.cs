using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
        public virtual ICollection<ParentChild> ParentChildren { get; set; } = new List<ParentChild>();
        public virtual ICollection<ParentChild> ChildParents { get; set; } = new List<ParentChild>();
    }
}