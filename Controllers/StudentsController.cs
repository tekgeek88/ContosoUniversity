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
        // Modifying the index page to be able to load with a sorted order
//        public async Task<IActionResult> Index(
//            string sortOrder, 
//            string currentFilter,
//            string searchString,
//            int? pageNumber) {
//            // added this line so we can remember the page number and the sort the page was on
//            ViewData["CurrentSort"] = sortOrder;
//            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
//            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
//            // This line was removed to allow us to keep track of pages
//            //            ViewData["currentFilter"] = searchString;
//
//            if (searchString != null) {
//                pageNumber = 1;
//            }
//            else {
//                searchString = currentFilter;
//            }
//
//            ViewData["CurrentFilter"] = searchString;
//            
//            var students = from s in _context.Students select s;
//
//            if (!String.IsNullOrEmpty(searchString)) {
//                students = students.Where(
//                    s => s.LastName.Contains(searchString) ||
//                         s.FirstMidName.Contains(searchString));
//            }
//            
//            switch (sortOrder) {
//                case "name_desc":
//                    students = students.OrderByDescending(s => s.LastName);
//                    break;
//                case "Date":
//                    students = students.OrderBy(s => s.EnrollmentDate);
//                    break;
//
//                case "date_desc":
//
//                    students = students.OrderByDescending(s => s.EnrollmentDate);
//                    break;
//                default:
//                    students = students.OrderBy(s => s.LastName);
//                    break;
//            }
//
//            int pageSize = 3;
//            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
//        }


        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber) {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] =
                String.IsNullOrEmpty(sortOrder) ? "LastName_desc" : "";
            ViewData["DateSortParm"] =
                sortOrder == "EnrollmentDate" ? "EnrollmentDate_desc" : "EnrollmentDate";

            if (searchString != null) {
                pageNumber = 1;
            }
            else {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var students = from s in _context.Students
                select s;

            if (!String.IsNullOrEmpty(searchString)) {
                students = students.Where(s => s.LastName.Contains(searchString)
                                               || s.FirstMidName.Contains(searchString));
            }

            // This statement replaces the default statement from the
            // switched version
            if (string.IsNullOrEmpty(sortOrder)) {
                sortOrder = "LastName";
            }

            bool descending = false;

            // This is checking for ether students name or
            // enrollment date should be descending.
            if (sortOrder.EndsWith("_desc")) {
                sortOrder = sortOrder.Substring(0, sortOrder.Length - 5);
                descending = true;
            }

            if (descending) {
                students = students.OrderByDescending(e => EF.Property<object>(e, sortOrder));
            }
            else {
                students = students.OrderBy(e => EF.Property<object>(e, sortOrder));
            }

            int pageSize = 3;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(),
                pageNumber ?? 1, pageSize));
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
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id) {
            if (id == null) {
                return NotFound();
            }

            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);

            if (await TryUpdateModelAsync<Student>(
                studentToUpdate,
                "",
                s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate)) {
                try {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                /**
                 * The scaffold code uses the create-and-attach approach but only catches DbUpdateConcurrencyException
                 * exceptions and returns 404 error codes. The example shown catches any database update exception and
                 * displays an error message.
                 */
                catch (DbUpdateException /* ex */) {
                    //Log the error (uncomment ex variable name and write a log.)
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