using ELearningPlatform.Models;
using ELearningPlatform.ViewModels;

namespace ELearningPlatform.Services
{
    public interface IProgressService
    {
        Task<List<StudentProgress>> GetStudentProgressAsync(string studentId);
        Task<double> GetSubjectProgressPercentageAsync(string studentId, int subjectId);
        Task<double> GetOverallProgressPercentageAsync(string studentId);
        Task<List<QuizAttempt>> GetStudentQuizAttemptsAsync(string studentId);
        Task<List<QuizAttempt>> GetRecentQuizAttemptsAsync(string studentId, int count = 5);
        Task<StudentProgressViewModel> GetDetailedProgressAsync(string studentId);
        Task<Dictionary<int, double>> GetSubjectProgressesAsync(string studentId);
    }
}