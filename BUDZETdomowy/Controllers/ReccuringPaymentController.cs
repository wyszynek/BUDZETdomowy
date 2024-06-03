using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBudget.Data;
using HomeBudget.Models;
using HomeBudget.Models.Enum;
using HomeBudget.Migrations;

namespace HomeBudget.Controllers
{
    public class ReccuringPaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReccuringPaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReccuringPayment
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var applicationDbContext = _context.ReccuringPayments.Include(r => r.Account).Include(r => r.Category).Include(r => r.Currency).Where(x => x.UserId == currentUserId);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ReccuringPayment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reccuringPayment = await _context.ReccuringPayments
                .Include(r => r.Account)
                .Include(r => r.Category)
                .Include(r => r.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reccuringPayment == null)
            {
                return NotFound();
            }

            return View(reccuringPayment);
        }

        // GET: ReccuringPayment/Create
        public IActionResult Create()
        {
            PopulateCategoriesAndAccounts();
            PopulateEnums();
            return View();
        }

        // POST: ReccuringPayment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,CategoryId,AccountId,Amount,CurrencyId,HowOften,FirstPaymentDate,LastPaymentDate")] ReccuringPayment reccuringPayment)
        {
            reccuringPayment.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(reccuringPayment);
            var reccuringPaymentCategory = _context.Categories.FirstOrDefault(t => t.CategoryName == "Reccuring Payment" && t.Icon == "&#128257;" && t.Type == "Expense" && t.UserId == reccuringPayment.UserId);

            if (ModelState.IsValid)
            {
                reccuringPayment.Category = reccuringPaymentCategory;
                TempData["ToastrMessage"] = "Reccuring payment has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(reccuringPayment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateCategoriesAndAccounts();
            PopulateEnums();
            return View(reccuringPayment);
        }

        // GET: ReccuringPayment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reccuringPayment = await _context.ReccuringPayments.FindAsync(id);
            if (reccuringPayment == null)
            {
                return NotFound();
            }

            PopulateCategoriesAndAccounts();
            PopulateEnums();
            return View(reccuringPayment);
        }

        // POST: ReccuringPayment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,CategoryId,AccountId,Amount,CurrencyId,HowOften,FirstPaymentDate,LastPaymentDate")] ReccuringPayment reccuringPayment)
        {
            reccuringPayment.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(reccuringPayment);

            if (id != reccuringPayment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    TempData["ToastrMessage"] = "Reccuring payment has been edited successfully";
                    TempData["ToastrType"] = "info";
                    _context.Update(reccuringPayment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReccuringPaymentExists(reccuringPayment.Id))
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
            PopulateEnums();
            return View(reccuringPayment);
        }

        // GET: ReccuringPayment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reccuringPayment = await _context.ReccuringPayments
                .Include(r => r.Account)
                .Include(r => r.Category)
                .Include(r => r.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reccuringPayment == null)
            {
                return NotFound();
            }

            return View(reccuringPayment);
        }

        // POST: ReccuringPayment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reccuringPayment = await _context.ReccuringPayments.FindAsync(id);
            if (reccuringPayment != null)
            {
                _context.ReccuringPayments.Remove(reccuringPayment);
            }

            TempData["ToastrMessage"] = "Reccuring payment has been deleted successfully";
            TempData["ToastrType"] = "warning";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReccuringPaymentExists(int id)
        {
            return _context.ReccuringPayments.Any(e => e.Id == id);
        }

        public void PopulateCategoriesAndAccounts()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);

            var userCategories = _context.Categories.Where(c => c.UserId == currentUserId && c.Type == "Expense").ToList();
            Category DefaultCategory = new Category() { Id = 0, CategoryName = "Choose a Category" };
            userCategories.Insert(0, DefaultCategory);
            ViewBag.Categories = userCategories;

            var userAccounts = _context.Accounts.Where(a => a.UserId == currentUserId).ToList();
            Account DefaultAccount = new Account() { Id = 0, AccountName = "Choose an Account" };
            userAccounts.Insert(0, DefaultAccount);
            ViewBag.Accounts = userAccounts;

            var userSources = _context.SourceOfIncomes.Where(a => a.UserId == currentUserId).ToList();
            SourceOfIncome DefaultSource = new SourceOfIncome() { Id = 0, Name = "Choose a Source" };
            userSources.Insert(0, DefaultSource);
            ViewBag.SourceOfIncomes = userSources;

            var CurrencyCollection = _context.Currencies.ToList();
            Currency DefaultCurrency = new Currency() { Id = 0, Code = "Choose a Currency" };
            CurrencyCollection.Insert(0, DefaultCurrency);
            ViewBag.Currencies = CurrencyCollection;
        }

        private void PopulateEnums()
        {
            ViewBag.ContractType = new SelectList(Enum.GetValues(typeof(ReccuringPaymentFrequency)).Cast<ReccuringPaymentFrequency>().Select(e => new SelectListItem
            {
                Value = ((decimal)e).ToString(),
                Text = e.ToString()
            }).ToList(), "Value", "Text");
        }
    }
}
