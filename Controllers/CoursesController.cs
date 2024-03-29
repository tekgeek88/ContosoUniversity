using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers {

    public class CoursesController : Controller {

        private readonly SchoolContext _context;

        public CoursesController(SchoolContext context) {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index() {
//            var schoolContext = _context.Courses.Include(c => c.Department);
            // 
            var courses = _context.Courses
                .Include(c => c.Department)
                .AsNoTracking();
            return View(await courses.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                /* To optimize performance of the Course Details and Delete pages,
                 * add AsNoTracking calls in the Details and HttpGet Delete methods. */
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);

            if (course == null) {
                return NotFound();
            }

            return View(course);
        }

        // Added in dykstra 07
        /* The HttpGet Create method calls the PopulateDepartmentsDropDownList method without
         * setting the selected item, because for a new course the department isn't established yet:
         */
        public IActionResult create() {
            PopulateDepartmentsDropDownList();
            return View();
        }


        /**
         * 
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID,Credits,DepartmentID,Title")]
            Course course) {
            if (ModelState.IsValid) {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }


        /**
         * The HttpGet Edit method sets the selected item, based on the ID of the department
         * that's already assigned to the course being edited:
         */
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);

            if (course == null) {
                return NotFound();
            }

            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id) {
            if (id == null) {
                return NotFound();
            }

            var courseToUpdate = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (await TryUpdateModelAsync<Course>(courseToUpdate,
                "",
                c => c.Credits, c => c.DepartmentID, c => c.Title)) {
                try {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */) {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                                                 "Try again, and if the problem persists, " +
                                                 "see your system administrator.");
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentID);
            return View(courseToUpdate);
        }


        private void PopulateDepartmentsDropDownList(object selectedDepartment = null) {
            var departmentsQuery = from d in _context.Departments
                orderby d.Name
                select d;
            ViewBag.DepartmentID =
                new SelectList(departmentsQuery.AsNoTracking(), "DepartmentID", "Name", selectedDepartment);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)

                // Added this to increase performance s
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);

            if (course == null) {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var course = await _context.Courses.FindAsync(id);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id) {
            return _context.Courses.Any(e => e.CourseID == id);
        }

        // Added a new page to allow admins to update the number of credits for all course by a factor
        public IActionResult UpdateCourseCredits() {
            return View();
        }

        // This method will execute an update method to update all courses credit loads
        // by the given factor.
        // If rows were effected it returns the number of rows effected.
        [HttpPost]
        public async Task<IActionResult> UpdateCourseCredits(int? multiplier) {
            if (multiplier != null) {
                ViewData["RowsAffected"] =
                    await _context.Database.ExecuteSqlCommandAsync(
                        "UPDATE Course SET Credits = Credits * {0}",
                        parameters: multiplier);
            }

            return View();
        }

    }

}