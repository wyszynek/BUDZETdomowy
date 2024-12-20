﻿using System;
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
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            return View(await _context.Events.Where(x => x.UserId == currentUserId).ToListAsync());
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Start,End,IsCompleted")] Event @event)
        {
            @event.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(@event);

            @event.IsCompleted = "No";

            if (ModelState.IsValid)
            {
                TempData["ToastrMessage"] = "Event has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: Event/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Start,End,IsCompleted")] Event @event)
        {
            @event.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(@event);

            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["ToastrMessage"] = "Budget has been edited successfully";
                TempData["ToastrType"] = "info";
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            TempData["ToastrMessage"] = "Budget has been deleted successfully";
            TempData["ToastrType"] = "warning";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        // GET: Event123/GetEvents
        public async Task<IActionResult> GetEvents()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var events = await _context.Events.Where(x => x.UserId == currentUserId).ToListAsync();
            return Json(events);
        }

        // POST: Event/CreateEvent
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event @event) //[FromBody] informuje framework ze wartosc parametru @event ma byc odczytana z ciala żądania HTTP
        {
            @event.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(@event);

            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return Json(new { id = @event.Id, title = @event.Title, start = @event.Start});
            }
            return BadRequest();
        }
    }
}
