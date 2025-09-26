using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearningPlatform.Services
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProgressService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<StudentProgress>> GetStudentProgressAsync(string studentId)
        {
            return await _context.StudentProgresses
                .Where(sp => sp.StudentId == studentId)
                .Include(sp => sp.Chapter)
                .ThenInclude(c => c.Subject)
                .ThenInclude(s => s.Class)
                .OrderByDescending(sp => sp.LastAccessedAt)
                .ToListAsync();
        }

        public async Task<double> GetSubjectProgressPercentageAsync(string studentId, int subjectId)
        {
            var totalChapters = await _context.Chapters.CountAsync(c => c.SubjectId == subjectId);

            if (totalChapters == 0) return 0;

            var completedChapters = await _context.StudentProgresses
                .Where(sp => sp.StudentId == studentId && sp.IsCompleted)
                .Join(_context.Chapters, sp => sp.ChapterId, c => c.Id, (sp, c) => c)
                .CountAsync(c => c.SubjectId == subjectId);

            return Math.Round((double)completedChapters / totalChapters * 100, 1);
        }

        public async Task<double> GetOverallProgressPercentageAsync(string studentId)
        {
            var studentClasses = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.ClassId)
                .ToListAsync();

            if (!studentClasses.Any()) return 0;

            var totalChapters = await _context.Chapters
                .Where(c => studentClasses.Contains(c.Subject.ClassId))
                .CountAsync();

            if (totalChapters == 0) return 0;

            var completedChapters = await _context.StudentProgresses
                .Where(sp => sp.StudentId == studentId && sp.IsCompleted)
                .Join(_context.Chapters, sp => sp.ChapterId, c => c.Id, (sp, c) => c)
                .Where(c => studentClasses.Contains(c.Subject.ClassId))
                .CountAsync();

            return Math.Round((double)completedChapters / totalChapters * 100, 1);
        }

        public async Task<List<QuizAttempt>> GetStudentQuizAttemptsAsync(string studentId)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.StudentId == studentId)
                .Include(qa => qa.Quiz)
                .ThenInclude(q => q.Chapter)
                .ThenInclude(c => c.Subject)
                .OrderByDescending(qa => qa.AttemptedAt)
                .ToListAsync();
        }

        public async Task<List<QuizAttempt>> GetRecentQuizAttemptsAsync(string studentId, int count = 5)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.StudentId == studentId)
                .Include(qa => qa.Quiz)
                .ThenInclude(q => q.Chapter)
                .ThenInclude(c => c.Subject)
                .OrderByDescending(qa => qa.AttemptedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<StudentProgressViewModel> GetDetailedProgressAsync(string studentId)
        {
            var student = await _userManager.FindByIdAsync(studentId);
            var studentClasses = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Include(sc => sc.Class)
                .ToListAsync();

            var subjectProgresses = new List<SubjectProgress>();

            foreach (var studentClass in studentClasses)
            {
                var subjects = await _context.Subjects
                    .Where(s => s.ClassId == studentClass.ClassId)
                    .ToListAsync();

                foreach (var subject in subjects)
                {
                    var totalChapters = await _context.Chapters.CountAsync(c => c.SubjectId == subject.Id);
                    var completedChapters = await _context.StudentProgresses
                        .Where(sp => sp.StudentId == studentId && sp.IsCompleted)
                        .Join(_context.Chapters, sp => sp.ChapterId, c => c.Id, (sp, c) => c)
                        .CountAsync(c => c.SubjectId == subject.Id);

                    subjectProgresses.Add(new SubjectProgress
                    {
                        SubjectName = subject.Name,
                        TotalChapters = totalChapters,
                        CompletedChapters = completedChapters
                    });
                }
            }

            var recentQuizzes = await GetRecentQuizAttemptsAsync(studentId, 10);
            var quizSummaries = recentQuizzes.Select(qa => new QuizAttemptSummary
            {
                QuizTitle = qa.Quiz.Title,
                SubjectName = qa.Quiz.Chapter.Subject.Name,
                Score = qa.Score,
                TotalQuestions = qa.TotalQuestions,
                AttemptedAt = qa.AttemptedAt
            }).ToList();

            return new StudentProgressViewModel
            {
                StudentName = student?.FullName ?? "Unknown Student",
                ClassName = studentClasses.FirstOrDefault()?.Class.Name ?? "No Class",
                SubjectProgresses = subjectProgresses,
                RecentQuizzes = quizSummaries
            };
        }

        public async Task<Dictionary<int, double>> GetSubjectProgressesAsync(string studentId)
        {
            var studentClasses = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.ClassId)
                .ToListAsync();

            var subjects = await _context.Subjects
                .Where(s => studentClasses.Contains(s.ClassId))
                .ToListAsync();

            var result = new Dictionary<int, double>();

            foreach (var subject in subjects)
            {
                var progress = await GetSubjectProgressPercentageAsync(studentId, subject.Id);
                result[subject.Id] = progress;
            }

            return result;
        }
    }
}