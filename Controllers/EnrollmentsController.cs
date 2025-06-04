using Microsoft.AspNetCore.Authorization;
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
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Teacher)
                .Include(e => e.Group)
                .Include(e => e.Course)
                .ToListAsync();

            return View(enrollments);
        }


        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Group)
                .Include(e => e.Student)
                .Include(e => e.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseName");
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "GroupNumber");
            ViewData["StudentId"] = new SelectList(_context.AppUsers, "Id", "Email");
            ViewData["TeacherId"] = new SelectList(_context.AppUsers, "Id", "Email");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,CourseId,TeacherId,GroupId")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();

                var course = await _context.Courses.FindAsync(enrollment.CourseId);
                if (course != null)
                {
                    TempData["EnrollmentAlert"] = $"Ai fost înscris la cursul '{course.CourseName}'!";
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseName", enrollment.CourseId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "GroupNumber", enrollment.GroupId);
            ViewData["StudentId"] = new SelectList(_context.AppUsers, "Id", "Email", enrollment.StudentId);
            ViewData["TeacherId"] = new SelectList(_context.AppUsers, "Id", "Email", enrollment.TeacherId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseName", enrollment.CourseId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "GroupNumber", enrollment.GroupId);
            ViewData["StudentId"] = new SelectList(_context.AppUsers, "Id", "Email", enrollment.StudentId);
            ViewData["TeacherId"] = new SelectList(_context.AppUsers, "Id", "Email", enrollment.TeacherId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,CourseId,TeacherId,GroupId")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseName", enrollment.CourseId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "GroupNumber", enrollment.GroupId);
            ViewData["StudentId"] = new SelectList(_context.AppUsers, "Id", "Email", enrollment.StudentId);
            ViewData["TeacherId"] = new SelectList(_context.AppUsers, "Id", "Email", enrollment.TeacherId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        [Authorize(Roles = "Moderator,Secretar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Group)
                .Include(e => e.Student)
                .Include(e => e.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator,Secretar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ListEnrollments(int courseId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)  
                .Include(e => e.Teacher)
                .ToListAsync();

            ViewBag.CourseId = courseId;  

            return View(enrollments);
        }


        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
