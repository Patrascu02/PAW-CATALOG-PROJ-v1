using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PAW_CATALOG_PROJ.Data;
using PAW_CATALOG_PROJ.Models;
using Microsoft.AspNetCore.Identity;

namespace PAW_CATALOG_PROJ.Controllers
{
    [Authorize]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public GradesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Grades
        public async Task<IActionResult> Index(string? sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (string.IsNullOrEmpty(user.Email))
                return Forbid();

            var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (appUser == null)
                return Forbid();

            if (User.IsInRole("Profesor"))
            {
                var grades = await _context.Grades
                    .Include(g => g.Enrollment).ThenInclude(e => e.Student)
                    .Include(g => g.Enrollment).ThenInclude(e => e.Course)
                    .Where(g => g.Enrollment.TeacherId == appUser.Id)
                    .ToListAsync();

                ViewBag.Role = "Profesor";
                return View("ProfesorIndex", grades);
            }

            if (User.IsInRole("Student"))
            {
                var gradesQuery = _context.Grades
                    .Include(g => g.Enrollment)
                        .ThenInclude(e => e.Course)
                    .Include(g => g.Enrollment)
                        .ThenInclude(e => e.Student)
                    .Where(g => g.Enrollment.StudentId == appUser.Id);

                gradesQuery = sortBy switch
                {
                    "grade" => gradesQuery.OrderByDescending(g => g.GradeValue),
                    "alphabetical" => gradesQuery.OrderBy(g => g.Enrollment.Course.CourseName),
                    _ => gradesQuery
                };

                var grades = await gradesQuery.ToListAsync();

                double overallAverage = (double)(grades.Any()
                    ? grades.Average(g => g.GradeValue)
                    : 0);

                var fixedYears = new int[] { 1, 2, 3 };

                var averagesForExistingYears = grades
                    .GroupBy(g => g.Enrollment.EnrollmentYear)
                    .Select(group => new
                    {
                        Year = group.Key,
                        Average = group.Average(g => g.GradeValue)
                    })
                    .ToList();

                var completedAveragesPerYear = fixedYears
                    .Select(year =>
                    {
                        var found = averagesForExistingYears.FirstOrDefault(a => a.Year == year.ToString());
                        return new
                        {
                            Year = year,
                            Average = found != null ? Math.Round(found.Average, 2) : 0
                        };
                    })
                    .ToList();

                ViewBag.OverallAverage = Math.Round(overallAverage, 2);
                ViewBag.AveragesPerYear = completedAveragesPerYear;
                ViewBag.Role = "Student";
                ViewBag.SortBy = sortBy;
                return View("StudentIndex", grades);
            }



            return Forbid();
        }



        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Create(int? enrollmentId)
        {
            if (enrollmentId == null)
                return BadRequest();

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return NotFound();

            var grade = new Grade
            {
                EnrollmentId = enrollment.Id,
                DateRecorded = DateTime.Now
            };

            ViewData["EnrollmentInfo"] = $"{enrollment.Student.Name} - {enrollment.Course.CourseName}";

            return View(grade);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Create([Bind("EnrollmentId,Title,GradeValue")] Grade grade)
        {

            Console.WriteLine($"{grade.GradeValue},{grade.EnrollmentId},{grade.Title}");
            Console.WriteLine($"************************************************");
            Console.WriteLine($"************************************************");
            Console.WriteLine($"************************************************");
            Console.WriteLine($"************************************************");
            Console.WriteLine($"************************************************");
            Console.WriteLine($"************************************************");

            if (ModelState.IsValid)
            {
                grade.DateRecorded = DateTime.Now; 

                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Enrollments"); 
            }

          
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == grade.EnrollmentId);

            if (enrollment == null)
                return NotFound();

            ViewData["EnrollmentInfo"] = $"{enrollment.Student.Email} - {enrollment.Course.CourseName}";

            return View(grade);
        }




        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();

            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "Id", grade.EnrollmentId);
            return View(grade);
        }

        [Authorize(Roles = "Profesor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GradeId,EnrollmentId,Title,GradeValue,MaxGrade,DateRecorded")] Grade grade)
        {
            if (id != grade.GradeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Grades.Any(e => e.GradeId == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "Id", grade.EnrollmentId);
            return View(grade);
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var grade = await _context.Grades
                .Include(g => g.Enrollment)
                .FirstOrDefaultAsync(m => m.GradeId == id);

            if (grade == null) return NotFound();

            return View(grade);
        }

        [Authorize(Roles = "Profesor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
                _context.Grades.Remove(grade);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
