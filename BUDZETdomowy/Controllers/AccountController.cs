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
using HomeBudget.Migrations;

namespace HomeBudget.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            return View(await _context.Accounts.Include(a => a.Currency).Where(x => x.UserId == currentUserId).ToListAsync());

        }

        // GET: Account/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        public IActionResult CreateEmergencyFund()
        {
            PopulateCurrency();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmergencyFund([Bind("AccountId,AccountName,Note,Income,Expanse,CurrencyId")] Account account)
        {
            account.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(account);

            var existingEmergencyFund = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == account.UserId && a.AccountName == "Emergency Fund");

            if (existingEmergencyFund != null)
            {
                // Jeśli użytkownik ma już konto awaryjne, przekieruj go do szczegółów istniejącego konta.
                return RedirectToAction("Details", new { id = existingEmergencyFund.Id });
            }

            account.AccountName = "Emergency Fund";
            account.Note = "Emergency Fund";

            if (ModelState.IsValid)
            {
                TempData["ToastrMessage"] = "Emergency fund has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCurrency();
            return View(account);
        }


        // GET: Account/Create
        public IActionResult Create()
        {
            PopulateCurrency();
            return View();
        }

        // POST: Account/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,AccountName,Note,Income,Expanse,CurrencyId")] Account account)
        {
            account.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(account);

            if (ModelState.IsValid)
            {
                TempData["ToastrMessage"] = "Account has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCurrency();
            return View(account);
        }

        // GET: Account/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            PopulateCurrency();
            return View(account);
        }

        // POST: Account/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,AccountName,Note,Income,Expanse,CurrencyId")] Account account)
        {
            account.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(account);

            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var previousAccount = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (previousAccount != null)
                    {
                        var sourceCurrency = await _context.Currencies.FirstAsync(x => x.Id == previousAccount.CurrencyId);
                        var targetCurrency = await _context.Currencies.FirstAsync(x => x.Id == account.CurrencyId);

                        account.Income = await CurrencyRateHelper.Calculate(account.Income, sourceCurrency.Code, targetCurrency.Code);
                        account.Expanse = await CurrencyRateHelper.Calculate(account.Expanse, sourceCurrency.Code, targetCurrency.Code);  

                        _context.Update(account);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Insufficient funds in the sender's account after editing.");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["ToastrMessage"] = "Account has been edited successfully";
                TempData["ToastrType"] = "info";
                return RedirectToAction(nameof(Index));
            }

            PopulateCurrency();
            return View(account);
        }

        // GET: Account/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Find and delete any transactions where the account is either the sender or recipient
            var relatedTransactions = await _context.TransactionBetweenAccounts
                .Where(t => t.SenderId == id || t.RecipientId == id)
                .ToListAsync();

            if (relatedTransactions.Any())
            {
                _context.TransactionBetweenAccounts.RemoveRange(relatedTransactions);
            }

            _context.Accounts.Remove(account);

            TempData["ToastrMessage"] = "Account and associated transactions have been deleted successfully";
            TempData["ToastrType"] = "warning";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }

        public void PopulateCurrency()
        {
            var CurrencyCollection = _context.Currencies.ToList();
            Currency DefaultCurrency = new Currency() { Id = 0, Code = "Choose a Currency" };
            CurrencyCollection.Insert(0, DefaultCurrency);
            ViewBag.Currencies = CurrencyCollection;
        }
    }
}
