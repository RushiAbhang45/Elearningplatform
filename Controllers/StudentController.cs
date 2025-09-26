using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ELearningPlatform.Models;
using ELearningPlatform.Services;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IContentService _contentService;
        private readonly IProgressService _progressService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IContentService contentService,
            IProgressService progressService,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentController> logger)
        {
            _contentService = contentService;
            _progressService = progressService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var studentId = _userManager.GetUserId(User)!;

            // Get student's classes
            var classes = await _contentService.GetClassesForStudentAsync(studentId);

            // Get overall progress
            var overallProgress = await _progressService.GetOverallProgressPercentageAsync(studentId);

            // Get recent quiz attempts
            var recentQuizAttempts = await _progressService.GetRecentQuizAttemptsAsync(studentId, 5);

            // Get subject progresses
            var subjectProgresses = await _progressService.GetSubjectProgressesAsync(studentId);

            // Get recent notices
            var notices = await _contentService.GetNoticesForStudentAsync(studentId);

            ViewBag.OverallProgress = overallProgress;
            ViewBag.RecentQuizAttempts = recentQuizAttempts;
            ViewBag.SubjectProgresses = subjectProgresses;
            ViewBag.RecentNotices = notices.Take(3).ToList();

            return View(classes);
        }

        public async Task<IActionResult> Subjects(int classId)
        {
            var studentId = _userManager.GetUserId(User)!;
            var subjects = await _contentService.GetSubjectsForClassAsync(classId);

            // Get progress for each subject
            var subjectProgresses = new Dictionary<int, double>();
            foreach (var subject in subjects)
            {
                var progress = await _progressService.GetSubjectProgressPercentageAsync(studentId, subject.Id);
                subjectProgresses[subject.Id] = progress;
            }

            ViewBag.ClassId = classId;
            ViewBag.SubjectProgresses = subjectProgresses;

            return View(subjects);
        }

        public async Task<IActionResult> Chapters(int subjectId)
        {
            var studentId = _userManager.GetUserId(User)!;
            var chapters = await _contentService.GetChaptersForSubjectAsync(subjectId);

            // Check completion status for each chapter
            var chapterStatuses = new Dictionary<int, bool>();
            foreach (var chapter in chapters)
            {
                var isCompleted = await _contentService.IsChapterCompletedAsync(studentId, chapter.Id);
                chapterStatuses[chapter.Id] = isCompleted;
            }

            ViewBag.SubjectId = subjectId;
            ViewBag.ChapterStatuses = chapterStatuses;

            if (chapters.Any())
            {
                ViewBag.SubjectName = chapters.First().Subject.Name;
                ViewBag.ClassName = chapters.First().Subject.Class.Name;
            }

            return View(chapters);
        }

        public async Task<IActionResult> Content(int chapterId)
        {
            var studentId = _userManager.GetUserId(User)!;

            var contents = await _contentService.GetContentForChapterAsync(chapterId);
            var quizzes = await _contentService.GetQuizzesForChapterAsync(chapterId);
            var isCompleted = await _contentService.IsChapterCompletedAsync(studentId, chapterId);

            if (contents.Any() || quizzes.Any())
            {
                var chapter = contents.FirstOrDefault()?.Chapter ?? quizzes.FirstOrDefault()?.Chapter;
                if (chapter != null)
                {
                    ViewBag.ChapterTitle = chapter.Title;
                    ViewBag.SubjectName = chapter.Subject.Name;
                }
            }

            ViewBag.ChapterId = chapterId;
            ViewBag.Quizzes = quizzes;
            ViewBag.IsCompleted = isCompleted;

            return View(contents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteChapter(int chapterId)
        {
            try
            {
                var studentId = _userManager.GetUserId(User)!;
                await _contentService.MarkChapterCompletedAsync(studentId, chapterId);

                _logger.LogInformation("Student {StudentId} completed chapter {ChapterId}", studentId, chapterId);

                return Json(new { success = true, message = "Chapter marked as completed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing chapter {ChapterId}", chapterId);
                return Json(new { success = false, message = "An error occurred while completing the chapter." });
            }
        }

        public async Task<IActionResult> Progress()
        {
            var studentId = _userManager.GetUserId(User)!;
            var detailedProgress = await _progressService.GetDetailedProgressAsync(studentId);

            return View(detailedProgress);
        }

        public async Task<IActionResult> Quizzes()
        {
            var studentId = _userManager.GetUserId(User)!;
            var quizAttempts = await _progressService.GetStudentQuizAttemptsAsync(studentId);

            return View(quizAttempts);
        }

        public async Task<IActionResult> Notices()
        {
            var studentId = _userManager.GetUserId(User)!;
            var notices = await _contentService.GetNoticesForStudentAsync(studentId);

            return View(notices);
        }

        // Download content file
        public async Task<IActionResult> DownloadContent(int contentId)
        {
            var content = await _contentService.GetContentForChapterAsync(0); // This needs to be modified in service
                                                                              // Implementation for file download would go here
                                                                              // For now, redirect to file URL

            return NotFound(); // Placeholder
        }
    }
}