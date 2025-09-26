using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TeacherController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var teacherId = _userManager.GetUserId(User)!;

            // Get recent notices created by this teacher
            var recentNotices = await _context.Notices
                .Where(n => n.CreatedById == teacherId)
                .Include(n => n.Class)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get content created by this teacher
            var contentCount = await _context.Contents
                .CountAsync(c => c.CreatedById == teacherId);

            // Get classes (for statistics)
            var totalClasses = await _context.Classes.CountAsync();
            var totalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count;

            ViewBag.ContentCount = contentCount;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.NoticesCount = recentNotices.Count;

            return View(recentNotices);
        }

        // Notice Management
        public async Task<IActionResult> Notices()
        {
            var teacherId = _userManager.GetUserId(User)!;

            var notices = await _context.Notices
                .Where(n => n.CreatedById == teacherId)
                .Include(n => n.Class)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();

            return View(notices);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotice(string title, string content, int? classId)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Title and content are required.";
                return RedirectToAction(nameof(Notices));
            }

            var teacherId = _userManager.GetUserId(User)!;

            var notice = new Notice
            {
                Title = title,
                Content = content,
                ClassId = classId,
                CreatedById = teacherId
            };

            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher {TeacherId} created notice: {NoticeTitle}", teacherId, title);
            TempData["Success"] = "Notice created successfully.";

            return RedirectToAction(nameof(Notices));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            var teacherId = _userManager.GetUserId(User)!;

            var notice = await _context.Notices
                .FirstOrDefaultAsync(n => n.Id == id && n.CreatedById == teacherId);

            if (notice == null)
            {
                TempData["Error"] = "Notice not found or you don't have permission to delete it.";
                return RedirectToAction(nameof(Notices));
            }

            _context.Notices.Remove(notice);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher {TeacherId} deleted notice: {NoticeTitle}", teacherId, notice.Title);
            TempData["Success"] = "Notice deleted successfully.";

            return RedirectToAction(nameof(Notices));
        }

        // Content Management
        public async Task<IActionResult> Content()
        {
            var teacherId = _userManager.GetUserId(User)!;

            var classes = await _context.Classes
                .Include(c => c.Subjects)
                .ThenInclude(s => s.Chapters)
                .ThenInclude(ch => ch.Contents.Where(content => content.CreatedById == teacherId))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(classes);
        }

        public async Task<IActionResult> CreateContent(int? chapterId)
        {
            ViewBag.ChapterId = chapterId;

            var classes = await _context.Classes
                .Include(c => c.Subjects)
                .ThenInclude(s => s.Chapters)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Classes = classes;

            if (chapterId.HasValue)
            {
                var chapter = await _context.Chapters
                    .Include(c => c.Subject)
                    .ThenInclude(s => s.Class)
                    .FirstOrDefaultAsync(c => c.Id == chapterId.Value);

                ViewBag.Chapter = chapter;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContent(string title, string? description, int chapterId,
            ContentType type, string? textContent, string? fileUrl, string? videoUrl)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Content title is required.";
                return RedirectToAction(nameof(CreateContent), new { chapterId });
            }

            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                TempData["Error"] = "Chapter not found.";
                return RedirectToAction(nameof(Content));
            }

            var teacherId = _userManager.GetUserId(User)!;

            // Get the next order index
            var nextOrderIndex = await _context.Contents
                .Where(c => c.ChapterId == chapterId)
                .CountAsync() + 1;

            var content = new Content
            {
                Title = title,
                Description = description,
                ChapterId = chapterId,
                Type = type,
                TextContent = textContent,
                FileUrl = fileUrl,
                VideoUrl = videoUrl,
                CreatedById = teacherId,
                OrderIndex = nextOrderIndex
            };

            _context.Contents.Add(content);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher {TeacherId} created content: {ContentTitle} for chapter {ChapterId}",
                teacherId, title, chapterId);
            TempData["Success"] = "Content created successfully.";

            return RedirectToAction(nameof(Content));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContent(int id)
        {
            var teacherId = _userManager.GetUserId(User)!;

            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedById == teacherId);

            if (content == null)
            {
                TempData["Error"] = "Content not found or you don't have permission to delete it.";
                return RedirectToAction(nameof(Content));
            }

            _context.Contents.Remove(content);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher {TeacherId} deleted content: {ContentTitle}", teacherId, content.Title);
            TempData["Success"] = "Content deleted successfully.";

            return RedirectToAction(nameof(Content));
        }

        // Student Engagement
        public async Task<IActionResult> Students()
        {
            // Get all students and their basic engagement metrics
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var studentEngagement = new List<object>();

            foreach (var student in students)
            {
                var progressCount = await _context.StudentProgresses
                    .CountAsync(sp => sp.StudentId == student.Id && sp.IsCompleted);

                var quizAttempts = await _context.QuizAttempts
                    .CountAsync(qa => qa.StudentId == student.Id);

                var averageScore = await _context.QuizAttempts
                    .Where(qa => qa.StudentId == student.Id)
                    .Select(qa => (double)qa.Score / qa.TotalQuestions * 100)
                    .DefaultIfEmpty(0)
                    .AverageAsync();

                studentEngagement.Add(new
                {
                    Student = student,
                    CompletedChapters = progressCount,
                    QuizAttempts = quizAttempts,
                    AverageScore = Math.Round(averageScore, 1)
                });
            }

            ViewBag.StudentEngagement = studentEngagement;

            return View();
        }

        // Quiz Management
        public async Task<IActionResult> Quizzes()
        {
            var teacherId = _userManager.GetUserId(User)!;

            var classes = await _context.Classes
                .Include(c => c.Subjects)
                .ThenInclude(s => s.Chapters)
                .ThenInclude(ch => ch.Quizzes)
                .ThenInclude(q => q.Questions)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(classes);
        }

        public async Task<IActionResult> CreateQuiz(int? chapterId)
        {
            ViewBag.ChapterId = chapterId;

            var classes = await _context.Classes
                .Include(c => c.Subjects)
                .ThenInclude(s => s.Chapters)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Classes = classes;

            if (chapterId.HasValue)
            {
                var chapter = await _context.Chapters
                    .Include(c => c.Subject)
                    .ThenInclude(s => s.Class)
                    .FirstOrDefaultAsync(c => c.Id == chapterId.Value);

                ViewBag.Chapter = chapter;
            }

            return View();
        }

        // Get chapters for a subject (AJAX endpoint)
        [HttpGet]
        public async Task<IActionResult> GetChapters(int subjectId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.SubjectId == subjectId)
                .OrderBy(c => c.OrderIndex)
                .Select(c => new { c.Id, c.Title })
                .ToListAsync();

            return Json(chapters);
        }
    }
}   