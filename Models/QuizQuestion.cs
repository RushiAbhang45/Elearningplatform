using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        [Required]
        public string Question { get; set; } = string.Empty;

        [Required]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        public string OptionD { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = string.Empty; // A, B, C, or D

        public int QuizId { get; set; }
        public int OrderIndex { get; set; }

        // Navigation properties
        public virtual Quiz Quiz { get; set; } = null!;
    }
}