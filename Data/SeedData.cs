using Microsoft.AspNetCore.Identity;
using ELearningPlatform.Models;

namespace ELearningPlatform.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roles = { "Admin", "Teacher", "Student", "Parent" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            var adminEmail = "admin@elearning.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create sample data for testing
            await SeedSampleData(context, userManager);
        }

        private static async Task SeedSampleData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Create sample teacher
            var teacherEmail = "teacher@elearning.com";
            var teacher = await userManager.FindByEmailAsync(teacherEmail);

            if (teacher == null)
            {
                teacher = new ApplicationUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FirstName = "John",
                    LastName = "Teacher",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(teacher, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher, "Teacher");
                }
            }

            // Create sample student
            var studentEmail = "student@elearning.com";
            var student = await userManager.FindByEmailAsync(studentEmail);

            if (student == null)
            {
                student = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FirstName = "Jane",
                    LastName = "Student",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(student, "Student@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Student");
                }
            }

            // Create sample parent
            var parentEmail = "parent@elearning.com";
            var parent = await userManager.FindByEmailAsync(parentEmail);

            if (parent == null)
            {
                parent = new ApplicationUser
                {
                    UserName = parentEmail,
                    Email = parentEmail,
                    FirstName = "Bob",
                    LastName = "Parent",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(parent, "Parent@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(parent, "Parent");
                }
            }

            // Create sample class and subjects
            if (!context.Classes.Any())
            {
                var sampleClass = new Class
                {
                    Name = "Grade 10",
                    Description = "High School Grade 10 - Science Stream"
                };

                context.Classes.Add(sampleClass);
                await context.SaveChangesAsync();

                // Add subjects
                var subjects = new List<Subject>
                {
                    new Subject { Name = "Mathematics", Description = "Advanced Mathematics", ClassId = sampleClass.Id },
                    new Subject { Name = "Physics", Description = "Fundamental Physics", ClassId = sampleClass.Id },
                    new Subject { Name = "Chemistry", Description = "Basic Chemistry", ClassId = sampleClass.Id }
                };

                context.Subjects.AddRange(subjects);
                await context.SaveChangesAsync();

                // Enroll sample student in the class
                if (student != null)
                {
                    context.StudentClasses.Add(new StudentClass
                    {
                        StudentId = student.Id,
                        ClassId = sampleClass.Id
                    });
                }

                // Link parent with child
                if (parent != null && student != null)
                {
                    context.ParentChildren.Add(new ParentChild
                    {
                        ParentId = parent.Id,
                        ChildId = student.Id
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}