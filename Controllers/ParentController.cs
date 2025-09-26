using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Services;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProgressService _progressService;
        private readonly IContentService _contentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ParentController> _logger;

        public ParentController(
            ApplicationDbContext context,
            IProgressService progressService,
            IContentService contentService,
            UserManager<ApplicationUser> userManager,
            ILogger<ParentController> logger)
        {
            _context = context;
            _progressService = progressService;
            _contentService = contentService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var parentId = _userManager.GetUserId(User)!;

            var children = await _context.ParentChildren
                .Where(pc => pc.ParentId == parentId)
                .Include(pc => pc.Child)
                .Select(pc => pc.Child)
                .ToListAsync();

            // Get summary statistics for each child
            var childrenSummary = new List<object>();

            foreach (var child in children)
            {
                var overallProgress = await _progressService.GetOverallProgressPercentageAsync(child.Id);
                var recentQuizzes = await _progressService.GetRecentQuizAttemptsAsync(child.Id, 3);

                var averageQuizScore = recentQuizzes.Any()
                    ? recentQuizzes.Average(qa => (double)qa.Score / qa.TotalQuestions * 100)
                    : 0;

                childrenSummary.Add(new
                {
                    Child = child,
                    OverallProgress = overallProgress,
                    RecentQuizzesCount = recentQuizzes.Count,
                    AverageQuizScore = Math.Round(averageQuizScore, 1)
                });
            }

            ViewBag.ChildrenSummary = childrenSummary;

            return View(children);
        }

        public async Task<IActionResult> ChildProgress(string childId)
        {
            var parentId = _userManager.GetUserId(User)!;

            // Verify this child belongs to this parent
            var parentChild = await _context.ParentChildren
                .FirstOrDefaultAsync(pc => pc.ParentId == parentId && pc.ChildId == childId);

            if (parentChild == null)
            {
                TempData["Error"] = "Child not found or you don't have access to view this child's progress.";
                return RedirectToAction(nameof(Index));
            }

            var child = await _userManager.FindByIdAsync(childId);
            var detailedProgress = await _progressService.GetDetailedProgressAsync(childId);
            var allQuizAttempts = await _progressService.GetStudentQuizAttemptsAsync(childId);

            // Get child's classes and subjects
            var childClasses = await _contentService.GetClassesForStudentAsync(childId);
            var subjectProgresses = new List<object>();

            foreach (var childClass in childClasses)
            {
                var subjects = await _contentService.GetSubjectsForClassAsync(childClass.Id);

                foreach (var subject in subjects)
                {
                    var progress = await _progressService.GetSubjectProgressPercentageAsync(childId, subject.Id);
                    var chapters = await _contentService.GetChaptersForSubjectAsync(subject.Id);

                    var completedChapters = 0;
                    foreach (var chapter in chapters)
                    {
                        if (await _contentService.IsChapterCompletedAsync(childId, chapter.Id))
                        {
                            completedChapters++;
                        }
                    }

                    subjectProgresses.Add(new
                    {
                        SubjectName = subject.Name,
                        ClassName = childClass.Name,
                        Progress = progress,
                        TotalChapters = chapters.Count,
                        CompletedChapters = completedChapters
                    });
                }
            }

            ViewBag.Child = child;
            ViewBag.SubjectProgresses = subjectProgresses;
            ViewBag.AllQuizAttempts = allQuizAttempts;

            return View(detailedProgress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkChild(string childEmail)
        {
            if (string.IsNullOrWhiteSpace(childEmail))
            {
                TempData["Error"] = "Please enter a valid email address.";
                return RedirectToAction(nameof(Index));
            }

            var child = await _userManager.FindByEmailAsync(childEmail);

            if (child == null)
            {
                TempData["Error"] = "No user found with this email address.";
                return RedirectToAction(nameof(Index));
            }

            var isStudent = await _userManager.IsInRoleAsync(child, "Student");
            if (!isStudent)
            {
                TempData["Error"] = "The user with this email is not a student.";
                return RedirectToAction(nameof(Index));
            }

            var parentId = _userManager.GetUserId(User)!;

            var existingLink = await _context.ParentChildren
                .AnyAsync(pc => pc.ParentId == parentId && pc.ChildId == child.Id);

            if (existingLink)
            {
                TempData["Warning"] = "This child is already linked to your account.";
                return RedirectToAction(nameof(Index));
            }

            var parentChild = new ParentChild
            {
                ParentId = parentId,
                ChildId = child.Id
            };

            _context.ParentChildren.Add(parentChild);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Parent {ParentId} linked with child {ChildId}", parentId, child.Id);
            TempData["Success"] = $"Successfully linked with {child.FullName}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkChild(string childId)
        {
            var parentId = _userManager.GetUserId(User)!;

            var parentChild = await _context.ParentChildren
                .FirstOrDefaultAsync(pc => pc.ParentId == parentId && pc.ChildId == childId);

            if (parentChild == null)
            {
                TempData["Error"] = "Child link not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.ParentChildren.Remove(parentChild);
            await _context.SaveChangesAsync();

            var child = await _userManager.FindByIdAsync(childId);
            _logger.LogInformation("Parent {ParentId} unlinked from child {ChildId}", parentId, childId);
            TempData["Success"] = $"Successfully unlinked from {child?.FullName ?? "child"}.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChildNotices(string childId)
        {
            var parentId = _userManager.GetUserId(User)!;

            // Verify this child belongs to this parent
            var parentChild = await _context.ParentChildren
                .FirstOrDefaultAsync(pc => pc.ParentId == parentId && pc.ChildId == childId);

            if (parentChild == null)
            {
                TempData["Error"] = "Child not found or you don't have access to view this information.";
                return RedirectToAction(nameof(Index));
            }

            var child = await _userManager.FindByIdAsync(childId);
            var notices = await _contentService.GetNoticesForStudentAsync(childId);

            ViewBag.Child = child;

            return View(notices);
        }

        // Get quiz performance details for a specific child
        public async Task<IActionResult> QuizPerformance(string childId)
        {
            var parentId = _userManager.GetUserId(User)!;

            // Verify this child belongs to this parent
            var parentChild = await _context.ParentChildren
                .FirstOrDefaultAsync(pc => pc.ParentId == parentId && pc.ChildId == childId);

            if (parentChild == null)
            {
                TempData["Error"] = "Child not found or you don't have access to view this information.";
                return RedirectToAction(nameof(Index));
            }

            var child = await _userManager.FindByIdAsync(childId);
            var quizAttempts = await _progressService.GetStudentQuizAttemptsAsync(childId);

            // Group quiz attempts by subject for better visualization
            var quizPerformanceBySubject = quizAttempts
                .GroupBy(qa => qa.Quiz.Chapter.Subject.Name)
                .Select(g => new
                {
                    SubjectName = g.Key,
                    Attempts = g.OrderByDescending(qa => qa.AttemptedAt).ToList(),
                    AverageScore = g.Average(qa => (double)qa.Score / qa.TotalQuestions * 100),
                    TotalAttempts = g.Count(),
                    BestScore = g.Max(qa => (double)qa.Score / qa.TotalQuestions * 100)
                })
                .OrderBy(x => x.SubjectName)
                .ToList();

            ViewBag.Child = child;
            ViewBag.QuizPerformanceBySubject = quizPerformanceBySubject;

            return View(quizAttempts);
        }
    }
}