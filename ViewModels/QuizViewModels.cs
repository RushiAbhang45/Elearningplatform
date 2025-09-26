using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.ViewModels
{
    public class QuizViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int ChapterId { get; set; }
        public int TimeLimit { get; set; } = 30;
        public List<QuizQuestionViewModel> Questions { get; set; } = new();
    }

    public class QuizQuestionViewModel
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
        public string CorrectAnswer { get; set; } = string.Empty;

        public int OrderIndex { get; set; }
    }

    public class StudentProgressViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public List<SubjectProgress> SubjectProgresses { get; set; } = new();
        public List<QuizAttemptSummary> RecentQuizzes { get; set; } = new();
    }

    public class SubjectProgress
    {
        public string SubjectName { get; set; } = string.Empty;
        public int TotalChapters { get; set; }
        public int CompletedChapters { get; set; }
        public double ProgressPercentage => TotalChapters > 0 ? (double)CompletedChapters / TotalChapters * 100 : 0;
    }

    public class QuizAttemptSummary
    {
        public string QuizTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime AttemptedAt { get; set; }
        public double Percentage => TotalQuestions > 0 ? (double)Score / TotalQuestions * 100 : 0;
    }
}