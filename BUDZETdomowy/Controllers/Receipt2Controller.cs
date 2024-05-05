using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBudget.Data;
using HomeBudget.Models;

namespace HomeBudget.Controllers
{
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
            return View(await _context.Receipts2.ToListAsync());
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

                _context.Add(receipt2);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(receipt2);
        }

        //// Metoda pomocnicza do odczytywania ContentType na podstawie rozszerzenia pliku
        //private string GetContentTypeFromExtension(string fileName)
        //{
        //    string extension = Path.GetExtension(fileName).ToLowerInvariant();
        //    switch (extension)
        //    {
        //        case ".jpg":
        //        case ".jpeg":
        //            return "image/jpeg";
        //        case ".png":
        //            return "image/png";
        //        case ".gif":
        //            return "image/gif";
        //        // Dodaj dodatkowe rozszerzenia plików i ich ContentType według potrzeb
        //        default:
        //            return "application/octet-stream"; // Domyślny ContentType dla nieznanych rozszerzeń
        //    }
        //}


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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ContentType,Data")] Receipt2 receipt2)
        {
            if (id != receipt2.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Receipt2Exists(int id)
        {
            return _context.Receipts2.Any(e => e.Id == id);
        }
    }
}
