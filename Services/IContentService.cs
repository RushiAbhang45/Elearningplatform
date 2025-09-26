using ELearningPlatform.Models;

namespace ELearningPlatform.Services
{
    public interface IContentService
    {
        Task<List<Class>> GetClassesForStudentAsync(string studentId);
        Task<List<Subject>> GetSubjectsForClassAsync(int classId);
        Task<List<Chapter>> GetChaptersForSubjectAsync(int subjectId);
        Task<List<Content>> GetContentForChapterAsync(int chapterId);
        Task<List<Quiz>> GetQuizzesForChapterAsync(int chapterId);
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
        Task MarkChapterCompletedAsync(string studentId, int chapterId);
        Task<bool> IsChapterCompletedAsync(string studentId, int chapterId);
        Task<List<Notice>> GetNoticesForStudentAsync(string studentId);
    }
}