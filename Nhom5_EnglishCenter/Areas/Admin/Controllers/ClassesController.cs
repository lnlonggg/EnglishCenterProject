using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using TrungTamAnhNgu.Web.ViewModels;

namespace Nhom5_EnglishCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var classes = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher.ApplicationUser)
                .Include(c => c.Enrollments);
            return View(await classes.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var classModel = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher.ApplicationUser)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (classModel == null) return NotFound();

            return View(classModel);
        }

        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (await IsScheduleConflict(viewModel.TeacherId, viewModel.Schedule, viewModel.StartDate, viewModel.EndDate))
                {
                    ModelState.AddModelError("", "Xung đột lịch dạy! Giáo viên này đã có lớp khác vào thời gian và lịch học này.");
                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }
                Class newClass = new Class
                {
                    ClassName = viewModel.ClassName,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Schedule = viewModel.Schedule,
                    MaxStudents = viewModel.MaxStudents,
                    MinStudents = viewModel.MinStudents,
                    Status = viewModel.Status,
                    Format = viewModel.Format,
                    Location = viewModel.Location,
                    MeetingUrl = viewModel.MeetingUrl,
                    CourseId = viewModel.CourseId,
                    TeacherId = viewModel.TeacherId
                };

                _context.Add(newClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var classModel = await _context.Classes.FindAsync(id);
            if (classModel == null) return NotFound();

            ClassViewModel viewModel = new ClassViewModel
            {
                Id = classModel.Id,
                ClassName = classModel.ClassName,
                StartDate = classModel.StartDate,
                EndDate = classModel.EndDate,
                Schedule = classModel.Schedule,
                MaxStudents = classModel.MaxStudents,
                MinStudents = classModel.MinStudents,
                Status = classModel.Status,
                Format = classModel.Format,
                Location = classModel.Location,
                MeetingUrl = classModel.MeetingUrl,
                CourseId = classModel.CourseId,
                TeacherId = classModel.TeacherId
            };

            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClassViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await IsScheduleConflict(viewModel.TeacherId, viewModel.Schedule, viewModel.StartDate, viewModel.EndDate, id))
                {
                    ModelState.AddModelError("", "Xung đột lịch dạy! Giáo viên này đã có lớp khác vào thời gian và lịch học này.");

                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }
                try
                {
                    var classFromDb = await _context.Classes.FindAsync(id);
                    if (classFromDb == null) return NotFound();

                    classFromDb.ClassName = viewModel.ClassName;
                    classFromDb.StartDate = viewModel.StartDate;
                    classFromDb.EndDate = viewModel.EndDate;
                    classFromDb.Schedule = viewModel.Schedule;
                    classFromDb.MaxStudents = viewModel.MaxStudents;
                    classFromDb.MinStudents = viewModel.MinStudents;
                    classFromDb.Status = viewModel.Status;
                    classFromDb.Format = viewModel.Format;
                    classFromDb.Location = viewModel.Location;
                    classFromDb.MeetingUrl = viewModel.MeetingUrl;
                    classFromDb.CourseId = viewModel.CourseId;
                    classFromDb.TeacherId = viewModel.TeacherId;

                    _context.Update(classFromDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(viewModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var classModel = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classModel == null) return NotFound();

            return View(classModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classModel = await _context.Classes.FindAsync(id);
            if (classModel != null) _context.Classes.Remove(classModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }

        private async Task<bool> IsScheduleConflict(int teacherId, string schedule, DateTime start, DateTime end, int? ignoreClassId = null)
        {
            return await _context.Classes.AnyAsync(c =>
                c.Id != ignoreClassId &&             
                c.Status != ClassStatus.Cancelled && 
                c.TeacherId == teacherId &&          
                c.Schedule == schedule &&            
                c.StartDate < end && c.EndDate > start 
            );
        }
    }
}