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
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Budget
        public async Task<IActionResult> Index()
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            var applicationDbContext = _context.Budgets.Include(b => b.Account).Include(b => b.Category).Where(t => t.UserId.ToString() == currentUserId);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Budget/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets
                .Include(b => b.Account)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // GET: Budget/Create
        public IActionResult Create()
        {
            PopulateCategoriesAndAccounts();
            //ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountName");
            //ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View(new Budget());
        }

        // POST: Budget/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BudgetId,BudgetName,CategoryId,AccountId,Limit,BudgetProgress,CreationTime,EndTime")] Budget budget)
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            budget.UserId = int.Parse(currentUserId);
            await TryUpdateModelAsync(budget);

            if (ModelState.IsValid)
            {
                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCategoriesAndAccounts();
            //ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", budget.AccountId);
            //ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", budget.CategoryId);
            return View(budget);
        }

        // GET: Budget/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            PopulateCategoriesAndAccounts();
            return View(budget);
        }

        // POST: Budget/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BudgetId,BudgetName,CategoryId,AccountId,Limit,BudgetProgress,CreationTime,EndTime")] Budget budget)
        {
            if (id != budget.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BudgetExists(budget.Id))
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
            PopulateCategoriesAndAccounts();
            return View(budget);
        }

        // GET: Budget/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets
                .Include(b => b.Account)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // POST: Budget/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }

        [NonAction]
        public void PopulateCategoriesAndAccounts()
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

            var userCategories = _context.Categories.Where(c => c.UserId.ToString() == currentUserId).ToList();
            Category DefaultCategory = new Category() { Id = 0, CategoryName = "Choose a Category" };
            userCategories.Insert(0, DefaultCategory);
            ViewBag.Categories = userCategories;

            var userAccounts = _context.Accounts.Where(a => a.UserId.ToString() == currentUserId).ToList();
            Account DefaultAccount = new Account() { Id = 0, AccountName = "Choose an Account" };
            userAccounts.Insert(0, DefaultAccount);
            ViewBag.Accounts = userAccounts;
        }
    }
}
