using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBudget.Data;
using HomeBudget.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HomeBudget.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            return View(await _context.Categories.Where(x => x.UserId == currentUserId).ToListAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,Icon,Type")] Category category)
        {
            category.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(category);

            if (ModelState.IsValid)
            {
                TempData["ToastrMessage"] = "Category has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            if (IsSpecialCategory(category) == true)
            {
                TempData["ToastrMessage"] = "You cannot edit this category.";
                TempData["ToastrType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,Icon,Type")] Category category)
        {
            category.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(category);

            if (id != category.Id)
            {
                return NotFound();
            }

            if (IsSpecialCategory(category) == true)
            {
                TempData["ToastrMessage"] = "You cannot edit this category.";
                TempData["ToastrType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["ToastrMessage"] = "Category has been edited successfully";
                TempData["ToastrType"] = "info";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            if (IsSpecialCategory(category) == true)
            {
                TempData["ToastrMessage"] = "You cannot delete this category.";
                TempData["ToastrType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (IsSpecialCategory(category) == true)
            {
                TempData["ToastrMessage"] = "You cannot delete this category.";
                TempData["ToastrType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            TempData["ToastrMessage"] = "Category has been deleted successfully";
            TempData["ToastrType"] = "warning";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private bool IsSpecialCategory(Category category)
        {
            if (category.Type == "Income" && category.CategoryName == "Work" && category.Icon == "&#128184;")
            {
                return true;
            }

            return false;
        }
    }
}
