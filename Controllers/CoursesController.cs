using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PAW_CATALOG_PROJ.Data;
using PAW_CATALOG_PROJ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PAW_CATALOG_PROJ.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (appUser == null)
                return Forbid();

            List<Course> courses = new List<Course>();

            if (User.IsInRole("Student"))
            {
                courses = await _context.Courses
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.Student)
                    .Where(c => c.Enrollments.Any(e => e.StudentId == appUser.Id))
                    .ToListAsync();

                ViewBag.Role = "Student";
            }
            else if (User.IsInRole("Profesor"))
            {
                courses = await _context.Courses
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.Teacher)
                    .Where(c => c.Enrollments.Any(e => e.TeacherId == appUser.Id))
                    .ToListAsync();

                ViewBag.Role = "Profesor";
            }
            else if (User.IsInRole("Moderator"))
            {
                courses = await _context.Courses
                    .Include(c => c.Enrollments)
                    .ToListAsync();

                ViewBag.Role = "Moderator";
            }
            else
            {
                return Forbid();
            }

            return View(courses);
        }





        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchCourses(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction(nameof(Index));

            var loweredQuery = query.ToLower();

            
            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Teacher)
                .Where(c =>
                    c.CourseName.ToLower().Contains(loweredQuery) ||
                    c.Enrollments.Any(e => e.Teacher.Name.ToLower().Contains(loweredQuery)))
                .Distinct()
                .ToListAsync();

            ViewBag.Role = User.IsInRole("Student") ? "Student" :
                           User.IsInRole("Profesor") ? "Profesor" :
                           User.IsInRole("Moderator") ? "Moderator" : "Other";

            return View("Index", courses); 
        }


       /* public async Task<IActionResult> ShowSearchedCourse(string SearchCourses, string ProfesorId)
        {
            var query = _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchCourses))
            {
                query = query.Where(e => e.Course.CourseName.Contains(SearchCourses));
            }

            if (!string.IsNullOrEmpty(ProfesorId))
            {
                query = query.Where(e => e.TeacherId == ProfesorId);
            }

            var courses = await query
                .Select(e => e.Course)
                .Distinct()
                .ToListAsync();

            var profesori = await _context.AppUsers.ToListAsync();

            ViewData["Profesori"] = new SelectList(profesori, "Id", "Name", ProfesorId);

            return View("Index", courses);
        }

        */



        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }


        [Authorize(Roles = "Moderator")]
        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }


      
        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Create([Bind("Id,CourseName")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseName")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> ManageEnrollments(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Group)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            ViewBag.CourseName = course.CourseName;

            var students = await _userManager.GetUsersInRoleAsync("Student");
            ViewBag.AllStudents = new SelectList(students, "Id", "Name");

            var teachers = await _userManager.GetUsersInRoleAsync("Profesor");
            ViewBag.AllTeachers = new SelectList(teachers, "Id", "Name");

            ViewBag.Groups = new SelectList(_context.Groups, "Id", "GroupNumber");

            return View(course);
        }



        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> AddEnrollment(int CourseId, string StudentId, string TeacherId, int GroupId, string EnrollmentYear)
        {
            var exists = await _context.Enrollments.AnyAsync(e =>
                e.CourseId == CourseId && e.StudentId == StudentId);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    CourseId = CourseId,
                    StudentId = StudentId,
                    TeacherId = TeacherId,
                    GroupId = GroupId,
                    EnrollmentYear = EnrollmentYear
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageEnrollments", new { id = CourseId });
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
