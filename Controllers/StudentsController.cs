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

    public class StudentsController : Controller {

        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context) {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index() {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            // Adding code that Scaffolding API left out because the propert holds a collection.
            // The added code allows us to display on the Deatails page the contents using an HTML table.
            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)

                // The AsNoTracking method call improves performance by letting the server know we will
                // not need to update or modify the data of the entity in its lifetime.
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null) {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,EnrollmentDate")]
            Student student) {

            try {
                if (ModelState.IsValid) {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */) {
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists " +
                                             "see your system administrator.");
            }

            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);

            if (student == null) {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ID,LastName,FirstMidName,EnrollmentDate")]
            Student student) {
            if (id != student.ID) {
                return NotFound();
            }

            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);

            if (await TryUpdateModelAsync(studentToUpdate, "", s => s.FirstMidName, s => s.LastName,
                s => s.EnrollmentDate)) {
                try {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */) {
                    ModelState.AddModelError("", "Unable to save changes. " +
                                                 "Try again, and if the problem persists, " +
                                                 "see your system administrator.");
                }
            }

            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
        // Modified this method to allow for error handling on the delete operation
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false) {
            if (id == null) {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null) {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault()) {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        // Modified this method to handle error correction for NULL students
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var student = await _context.Students.FindAsync(id);

            if (student == null) {
                return RedirectToAction(nameof(Index));
            }

            try {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */) {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new {id = id, saveChangesError = true});
            }
        }

        private bool StudentExists(int id) {
            return _context.Students.Any(e => e.ID == id);
        }

    }

}