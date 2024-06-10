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
    public class Receipt2Controller : Controller
    {
        private readonly ApplicationDbContext _context;

        public Receipt2Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Receipt2
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            return View(await _context.Receipts2.Where(x => x.UserId == currentUserId).ToListAsync());
        }

        // GET: Receipt2/DisplayImage/5
        public async Task<IActionResult> DisplayImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receipt2 = await _context.Receipts2.FirstOrDefaultAsync(m => m.Id == id);
            if (receipt2 == null)
            {
                return NotFound();
            }

            return File(receipt2.Data, receipt2.ContentType);
        }

        // GET: Receipt2/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Receipt2/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Receipt2 receipt2)
        {
            receipt2.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(receipt2);

            if (ModelState.IsValid)
            {
                // Sprawdź czy użytkownik przesłał plik
                if (Request.Form.Files.Count > 0)
                {
                    var imageFile = Request.Form.Files[0]; // Pierwszy przesłany plik

                    // Jeśli użytkownik przesłał plik, obsłuż jego przesłanie
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        receipt2.Data = memoryStream.ToArray();
                        receipt2.ContentType = imageFile.ContentType;
                    }
                }

                TempData["ToastrMessage"] = "Receipt has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(receipt2);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(receipt2);
        }

        // GET: Receipt2/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receipt2 = await _context.Receipts2
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt2 == null)
            {
                return NotFound();
            }

            return View(receipt2);
        }

        // GET: Receipt2/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receipt2 = await _context.Receipts2.FindAsync(id);
            if (receipt2 == null)
            {
                return NotFound();
            }
            return View(receipt2);
        }

        // POST: Receipt2/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Receipt2 receipt2, IFormFile newImage)
        {
            receipt2.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(receipt2);

            if (id != receipt2.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Sprawdź, czy użytkownik przesłał nowy plik obrazu
                    if (newImage != null && newImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await newImage.CopyToAsync(memoryStream);
                            receipt2.Data = memoryStream.ToArray();
                            receipt2.ContentType = newImage.ContentType;
                        }
                    }
                    else
                    {
                        // Zachowaj istniejące dane obrazu, jeśli nowy plik nie został przesłany
                        var existingReceipt = await _context.Receipts2.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                        if (existingReceipt != null)
                        {
                            receipt2.Data = existingReceipt.Data;
                            receipt2.ContentType = existingReceipt.ContentType;
                        }
                    }

                    _context.Update(receipt2);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Receipt2Exists(receipt2.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["ToastrMessage"] = "Receipt has been edited successfully";
                TempData["ToastrType"] = "info";
                return RedirectToAction(nameof(Index));
            }
            return View(receipt2);
        }

        // GET: Receipt2/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receipt2 = await _context.Receipts2
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt2 == null)
            {
                return NotFound();
            }

            return View(receipt2);
        }

        // POST: Receipt2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var receipt2 = await _context.Receipts2.FindAsync(id);
            if (receipt2 != null)
            {
                _context.Receipts2.Remove(receipt2);
            }

            TempData["ToastrMessage"] = "Receipt has been deleted successfully";
            TempData["ToastrType"] = "warning";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Receipt2Exists(int id)
        {
            return _context.Receipts2.Any(e => e.Id == id);
        }
    }
}
