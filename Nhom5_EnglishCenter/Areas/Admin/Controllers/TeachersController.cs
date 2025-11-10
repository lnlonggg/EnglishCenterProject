using TrungTamAnhNgu.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Models;

namespace Nhom5_EnglishCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeachersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers
                                .Include(t => t.ApplicationUser)
                                .ToListAsync();
            return View(teachers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _userManager.FindByEmailAsync(viewModel.Email);
                if (userExists != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(viewModel);
                }

                ApplicationUser newAppUser = new ApplicationUser
                {
                    UserName = viewModel.Email,
                    Email = viewModel.Email,
                    FullName = viewModel.FullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newAppUser, viewModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAppUser, "Teacher");

                    Teacher newTeacherProfile = new Teacher
                    {
                        ApplicationUserId = newAppUser.Id,
                        Bio = viewModel.Bio,
                        AvatarUrl = viewModel.AvatarUrl
                    };

                    _context.Teachers.Add(newTeacherProfile);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return NotFound();

            TeacherEditViewModel viewModel = new TeacherEditViewModel
            {
                Id = teacher.Id,
                Email = teacher.ApplicationUser.Email,
                FullName = teacher.ApplicationUser.FullName,
                Bio = teacher.Bio,
                AvatarUrl = teacher.AvatarUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherEditViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var teacherFromDb = await _context.Teachers
                        .Include(t => t.ApplicationUser)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (teacherFromDb == null) return NotFound();

                    teacherFromDb.ApplicationUser.FullName = viewModel.FullName;
                    teacherFromDb.ApplicationUser.Email = viewModel.Email;
                    teacherFromDb.ApplicationUser.UserName = viewModel.Email;
                    teacherFromDb.Bio = viewModel.Bio;
                    teacherFromDb.AvatarUrl = viewModel.AvatarUrl;

                    _context.Update(teacherFromDb);

                    if (!string.IsNullOrEmpty(viewModel.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(teacherFromDb.ApplicationUser);
                        var result = await _userManager.ResetPasswordAsync(teacherFromDb.ApplicationUser, token, viewModel.NewPassword);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(viewModel);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(viewModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                await _userManager.DeleteAsync(teacher.ApplicationUser);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
