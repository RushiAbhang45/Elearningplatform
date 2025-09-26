using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace ELearningPlatform.Services
{
    public class ContentService : IContentService
    {
        private readonly ApplicationDbContext _context;

        public ContentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Class>> GetClassesForStudentAsync(string studentId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Include(sc => sc.Class)
                .ThenInclude(c => c.Subjects)
                .Select(sc => sc.Class)
                .ToListAsync();
        }

        public async Task<List<Subject>> GetSubjectsForClassAsync(int classId)
        {
            return await _context.Subjects
                .Where(s => s.ClassId == classId)
                .Include(s => s.Class)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<Chapter>> GetChaptersForSubjectAsync(int subjectId)
        {
            return await _context.Chapters
                .Where(c => c.SubjectId == subjectId)
                .Include(c => c.Subject)
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<List<Content>> GetContentForChapterAsync(int chapterId)
        {
            return await _context.Contents
                .Where(c => c.ChapterId == chapterId)
                .Include(c => c.CreatedBy)
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesForChapterAsync(int chapterId)
        {
            return await _context.Quizzes
                .Where(q => q.ChapterId == chapterId)
                .Include(q => q.Questions)
                .Include(q => q.Chapter)
                .ThenInclude(c => c.Subject)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.OrderIndex))
                .Include(q => q.Chapter)
                .ThenInclude(c => c.Subject)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task MarkChapterCompletedAsync(string studentId, int chapterId)
        {
            var progress = await _context.StudentProgresses
                .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ChapterId == chapterId);

            if (progress == null)
            {
                progress = new StudentProgress
                {
                    StudentId = studentId,
                    ChapterId = chapterId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                };
                _context.StudentProgresses.Add(progress);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.LastAccessedAt = DateTime.UtcNow;
            }
            else
            {
                progress.LastAccessedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsChapterCompletedAsync(string studentId, int chapterId)
        {
            var progress = await _context.StudentProgresses
                .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ChapterId == chapterId);

            return progress?.IsCompleted == true;
        }

        public async Task<List<Notice>> GetNoticesForStudentAsync(string studentId)
        {
            var studentClasses = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.ClassId)
                .ToListAsync();

            return await _context.Notices
                .Where(n => n.ClassId == null || studentClasses.Contains(n.ClassId.Value))
                .Include(n => n.CreatedBy)
                .Include(n => n.Class)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}