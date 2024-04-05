using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BUDZETdomowy.Data;
using BUDZETdomowy.Models;

namespace BUDZETdomowy.Controllers
{
    public class TransactionBetweenAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionBetweenAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransactionBetweenAccounts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TransactionBetweenAccounts.Include(t => t.RecipientAccount).Include(t => t.SenderAccount);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TransactionBetweenAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionBetweenAccounts = await _context.TransactionBetweenAccounts
                .Include(t => t.RecipientAccount)
                .Include(t => t.SenderAccount)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transactionBetweenAccounts == null)
            {
                return NotFound();
            }

            return View(transactionBetweenAccounts);
        }

        // GET: TransactionBetweenAccounts/Create
        public IActionResult Create()
        {
            PopulateAccount();
            //ViewData["RecipientId"] = new SelectList(_context.Accounts, "AccountId", "AccountName");
            //ViewData["SenderId"] = new SelectList(_context.Accounts, "AccountId", "AccountName");
            return View(new TransactionBetweenAccounts());
        }

        // POST: TransactionBetweenAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,SenderId,RecipientId,Amount,Note,Date")] TransactionBetweenAccounts transactionBetweenAccounts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transactionBetweenAccounts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateAccount();
            //ViewData["RecipientId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", transactionBetweenAccounts.RecipientId);
            //ViewData["SenderId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", transactionBetweenAccounts.SenderId);
            return View(transactionBetweenAccounts);
        }

        // GET: TransactionBetweenAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionBetweenAccounts = await _context.TransactionBetweenAccounts.FindAsync(id);
            if (transactionBetweenAccounts == null)
            {
                return NotFound();
            }
            ViewData["RecipientId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", transactionBetweenAccounts.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", transactionBetweenAccounts.SenderId);
            return View(transactionBetweenAccounts);
        }

        // POST: TransactionBetweenAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,SenderId,RecipientId,Amount,Note,Date")] TransactionBetweenAccounts transactionBetweenAccounts)
        {
            if (id != transactionBetweenAccounts.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionBetweenAccounts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionBetweenAccountsExists(transactionBetweenAccounts.TransactionId))
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
            ViewData["RecipientId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", transactionBetweenAccounts.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Accounts, "AccountId", "AccountName", transactionBetweenAccounts.SenderId);
            return View(transactionBetweenAccounts);
        }

        // GET: TransactionBetweenAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionBetweenAccounts = await _context.TransactionBetweenAccounts
                .Include(t => t.RecipientAccount)
                .Include(t => t.SenderAccount)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transactionBetweenAccounts == null)
            {
                return NotFound();
            }

            return View(transactionBetweenAccounts);
        }

        // POST: TransactionBetweenAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transactionBetweenAccounts = await _context.TransactionBetweenAccounts.FindAsync(id);
            if (transactionBetweenAccounts != null)
            {
                _context.TransactionBetweenAccounts.Remove(transactionBetweenAccounts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionBetweenAccountsExists(int id)
        {
            return _context.TransactionBetweenAccounts.Any(e => e.TransactionId == id);
        }

        public void PopulateAccount()
        {
            var AccountsCollection = _context.Accounts.ToList();
            Account DefaultAccount = new Account() { AccountId = 0, AccountName = "Choose an account" };
            AccountsCollection.Insert(0, DefaultAccount);
            ViewBag.Accounts = AccountsCollection;
        }
    }
}
