using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count,
                TotalTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).Count,
                TotalParents = (await _userManager.GetUsersInRoleAsync("Parent")).Count,
                TotalClasses = await _context.Classes.CountAsync(),
                TotalSubjects = await _context.Subjects.CountAsync(),
                TotalChapters = await _context.Chapters.CountAsync(),
                TotalQuizzes = await _context.Quizzes.CountAsync()
            };

            ViewBag.Stats = stats;

            // Recent activities
            var recentNotices = await _context.Notices
                .Include(n => n.CreatedBy)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentNotices = recentNotices;

            return View();
        }

        // Class Management
        public async Task<IActionResult> Classes()
        {
            var classes = await _context.Classes
                .Include(c => c.Subjects)
                .Include(c => c.StudentClasses)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(classes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Class name is required.";
                return RedirectToAction(nameof(Classes));
            }

            var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.Name == name);
            if (existingClass != null)
            {
                TempData["Error"] = "A class with this name already exists.";
                return RedirectToAction(nameof(Classes));
            }

            var newClass = new Class
            {
                Name = name,
                Description = description
            };

            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin created new class: {ClassName}", name);
            TempData["Success"] = "Class created successfully.";

            return RedirectToAction(nameof(Classes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject(string name, string? description, int classId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Subject name is required.";
                return RedirectToAction(nameof(Classes));
            }

            var classExists = await _context.Classes.AnyAsync(c => c.Id == classId);
            if (!classExists)
            {
                TempData["Error"] = "Selected class does not exist.";
                return RedirectToAction(nameof(Classes));
            }

            var subject = new Subject
            {
                Name = name,
                Description = description,
                ClassId = classId
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin created new subject: {SubjectName} for class ID: {ClassId}", name, classId);
            TempData["Success"] = "Subject created successfully.";

            return RedirectToAction(nameof(Classes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete == null)
            {
                TempData["Error"] = "Class not found.";
                return RedirectToAction(nameof(Classes));
            }

            _context.Classes.Remove(classToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin deleted class: {ClassName}", classToDelete.Name);
            TempData["Success"] = "Class deleted successfully.";

            return RedirectToAction(nameof(Classes));
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // Subject and Chapter Management
        public async Task<IActionResult> Subjects(int? classId)
        {
            var query = _context.Subjects
                .Include(s => s.Class)
                .Include(s => s.Chapters)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(s => s.ClassId == classId.Value);
                ViewBag.SelectedClass = await _context.Classes.FindAsync(classId.Value);
            }

            var subjects = await query.OrderBy(s => s.Class.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();

            ViewBag.Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();

            return View(subjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChapter(string title, string? description, int subjectId, int orderIndex = 1)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Chapter title is required.";
                return RedirectToAction(nameof(Subjects));
            }

            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null)
            {
                TempData["Error"] = "Subject not found.";
                return RedirectToAction(nameof(Subjects));
            }

            var chapter = new Chapter
            {
                Title = title,
                Description = description,
                SubjectId = subjectId,
                OrderIndex = orderIndex
            };

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin created new chapter: {ChapterTitle} for subject ID: {SubjectId}", title, subjectId);
            TempData["Success"] = "Chapter created successfully.";

            return RedirectToAction(nameof(Subjects), new { classId = subject.ClassId });
        }

        // Student Enrollment Management
        public async Task<IActionResult> Enrollments()
        {
            var enrollments = await _context.StudentClasses
                .Include(sc => sc.Student)
                .Include(sc => sc.Class)
                .OrderBy(sc => sc.Class.Name)
                .ThenBy(sc => sc.Student.LastName)
                .ToListAsync();

            ViewBag.Students = await _userManager.GetUsersInRoleAsync("Student");
            ViewBag.Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(string studentId, int classId)
        {
            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction(nameof(Enrollments));
            }

            var classExists = await _context.Classes.AnyAsync(c => c.Id == classId);
            if (!classExists)
            {
                TempData["Error"] = "Class not found.";
                return RedirectToAction(nameof(Enrollments));
            }

            var existingEnrollment = await _context.StudentClasses
                .AnyAsync(sc => sc.StudentId == studentId && sc.ClassId == classId);

            if (existingEnrollment)
            {
                TempData["Error"] = "Student is already enrolled in this class.";
                return RedirectToAction(nameof(Enrollments));
            }

            var enrollment = new StudentClass
            {
                StudentId = studentId,
                ClassId = classId
            };

            _context.StudentClasses.Add(enrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin enrolled student {StudentId} in class {ClassId}", studentId, classId);
            TempData["Success"] = "Student enrolled successfully.";

            return RedirectToAction(nameof(Enrollments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollStudent(int enrollmentId)
        {
            var enrollment = await _context.StudentClasses.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                TempData["Error"] = "Enrollment not found.";
                return RedirectToAction(nameof(Enrollments));
            }

            _context.StudentClasses.Remove(enrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin removed enrollment ID: {EnrollmentId}", enrollmentId);
            TempData["Success"] = "Student unenrolled successfully.";

            return RedirectToAction(nameof(Enrollments));
        }

        // Reports and Analytics
        public async Task<IActionResult> Reports()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalContent = await _context.Contents.CountAsync();
            var totalQuizAttempts = await _context.QuizAttempts.CountAsync();

            // Progress statistics
            var studentsWithProgress = await _context.StudentProgresses
                .Where(sp => sp.IsCompleted)
                .Select(sp => sp.StudentId)
                .Distinct()
                .CountAsync();

            var averageQuizScore = await _context.QuizAttempts
                .AverageAsync(qa => (double)qa.Score / qa.TotalQuestions * 100);

            // Most active classes
            var classActivity = await _context.StudentClasses
                .GroupBy(sc => sc.Class.Name)
                .Select(g => new { ClassName = g.Key, StudentCount = g.Count() })
                .OrderByDescending(x => x.StudentCount)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalContent = totalContent;
            ViewBag.TotalQuizAttempts = totalQuizAttempts;
            ViewBag.StudentsWithProgress = studentsWithProgress;
            ViewBag.AverageQuizScore = Math.Round(averageQuizScore, 1);
            ViewBag.ClassActivity = classActivity;

            return View();
        }
    }
}