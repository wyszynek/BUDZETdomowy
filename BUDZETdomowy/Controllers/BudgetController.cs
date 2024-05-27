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
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var applicationDbContext = _context.Budgets.Include(b => b.Account).Include(t => t.Account.Currency).Include(b => b.Category).Where(t => t.UserId == currentUserId);
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
                .Include(b => b.Account.Currency)
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
            budget.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(budget);

            if (ModelState.IsValid)
            {
                TempData["ToastrMessage"] = "Budget has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCategoriesAndAccounts();

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
            budget.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(budget);

            if (id != budget.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalBudget = await _context.Budgets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    budget.BudgetProgress = originalBudget.BudgetProgress;

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

                TempData["ToastrMessage"] = "Budget has been edited successfully";
                TempData["ToastrType"] = "info";
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
                .Include(b => b.Account.Currency)
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

            TempData["ToastrMessage"] = "Budget has been deleted successfully";
            TempData["ToastrType"] = "warning";
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
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);

            var expenseCategories = _context.Categories.Where(c => c.UserId == currentUserId && c.Type == "Expense").ToList();
            Category DefaultCategory = new Category() { Id = 0, CategoryName = "Choose a Category" };
            expenseCategories.Insert(0, DefaultCategory);
            ViewBag.Categories = expenseCategories;

            var userAccounts = _context.Accounts.Where(a => a.UserId == currentUserId).ToList();
            Account DefaultAccount = new Account() { Id = 0, AccountName = "Choose an Account" };
            userAccounts.Insert(0, DefaultAccount);
            ViewBag.Accounts = userAccounts;

            var CurrencyCollection = _context.Currencies.ToList();
            Currency DefaultCurrency = new Currency() { Id = 0, Code = "Choose a Currency" };
            CurrencyCollection.Insert(0, DefaultCurrency);
            ViewBag.Currencies = CurrencyCollection;
        }
    }
}
