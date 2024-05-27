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

namespace HomeBudget.Controllers
{
    [Authorize]
    public class NotepadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotepadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notepad
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            return View(await _context.Notepad.Where(x => x.UserId == currentUserId).ToListAsync());
        }

        // GET: Notepad/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notepad = await _context.Notepad
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notepad == null)
            {
                return NotFound();
            }

            return View(notepad);
        }

        // GET: Notepad/Create
        public IActionResult Create()
        {
            // Ustawienie daty na dzisiejszą przed przekazaniem modelu do widoku
            var model = new Notepad
            {
                Date = DateTime.Now
            };

            return View(model);
        }

        // POST: Notepad/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoteID,Date,Title,Description")] Notepad notepad)
        {
            notepad.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(notepad);
            if (ModelState.IsValid)
            {
                TempData["ToastrMessage"] = "Note has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(notepad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(notepad);
        }

        // GET: Notepad/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notepad = await _context.Notepad.FindAsync(id);
            if (notepad == null)
            {
                return NotFound();
            }
            return View(notepad);
        }

        // POST: Notepad/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NoteID,Date,Title,Description")] Notepad notepad)
        {
            notepad.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(notepad);

            if (id != notepad.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notepad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotepadExists(notepad.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["ToastrMessage"] = "Note has been created successfully";
                TempData["ToastrType"] = "info";
                return RedirectToAction(nameof(Index));
            }
            return View(notepad);
        }

        // GET: Notepad/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notepad = await _context.Notepad
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notepad == null)
            {
                return NotFound();
            }

            return View(notepad);
        }

        // POST: Notepad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notepad = await _context.Notepad.FindAsync(id);
            if (notepad != null)
            {
                _context.Notepad.Remove(notepad);
            }

            TempData["ToastrMessage"] = "Note has been deleted successfully";
            TempData["ToastrType"] = "warning";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotepadExists(int id)
        {
            return _context.Notepad.Any(e => e.Id == id);
        }
    }
}
