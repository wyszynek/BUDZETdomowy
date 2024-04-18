using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBudget.Data;
using HomeBudget.Models;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace HomeBudget.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Account).Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult ExportToCSV()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Category, Account, Amount, Date");
            foreach (var transaction in _context.Transactions.Include(t => t.Category).Include(t => t.Account))
            {
                string categoryName = transaction.Category?.CategoryName ?? "Unknown";
                string accountName = transaction.Account?.AccountName ?? "Unknown";
                builder.AppendLine($"{categoryName}, {accountName}, {transaction.Amount}, {transaction.Date}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "transactions.csv");
        }

        public IActionResult ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Transactions");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Category";
                worksheet.Cell(currentRow, 2).Value = "Account";
                worksheet.Cell(currentRow, 3).Value = "Amount";
                worksheet.Cell(currentRow, 4).Value = "Date";
                foreach (var transaction in _context.Transactions.Include(t => t.Category).Include(t => t.Account))
                {
                    currentRow++;
                    string categoryName = transaction.Category?.CategoryName ?? "Unknown";
                    string accountName = transaction.Account?.AccountName ?? "Unknown";
                    worksheet.Cell(currentRow, 1).Value = categoryName;
                    worksheet.Cell(currentRow, 2).Value = accountName;
                    worksheet.Cell(currentRow, 3).Value = transaction.Amount;
                    worksheet.Cell(currentRow, 4).Value = transaction.Date;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transactions.xlsx");
                }
            }
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            PopulateCategoriesAndAccounts();
            //ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountName");
            //ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View(new Transaction());
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,CategoryId,AccountId,Amount,Note,Date")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                // Pobierz konta i kategorię na podstawie ich identyfikatorów
                var targetAccount = await _context.Accounts.FindAsync(transaction.AccountId);
                var categoryType = _context.Categories
                    .Where(c => c.Id == transaction.CategoryId)
                    .Select(c => c.Type)
                    .FirstOrDefault();

                if (targetAccount != null && categoryType != null)
                {
                    if (categoryType == "Expense" && targetAccount.Income < transaction.Amount)
                    {
                        ModelState.AddModelError(string.Empty, "Insufficient funds in the account for this expense category.");
                        PopulateCategoriesAndAccounts();
                        return View(transaction);
                    }

                    if (categoryType == "Income")
                    {
                        targetAccount.Income += transaction.Amount;
                        // Dodaj transakcję do kontekstu bazy danych
                        _context.Add(transaction);

                        // Zapisz zmiany w bazie danych
                        await _context.SaveChangesAsync();

                        // Przekieruj użytkownika do listy transakcji
                        return RedirectToAction(nameof(Index));
                    }
                    else if (categoryType == "Expense")
                    {
                        targetAccount.Expanse += transaction.Amount;
                        targetAccount.Income -= transaction.Amount;
                        // Dodaj transakcję do kontekstu bazy danych
                        _context.Add(transaction);

                        // Zapisz zmiany w bazie danych
                        await _context.SaveChangesAsync();

                        // Przekieruj użytkownika do listy transakcji
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid category or account selected.");
                }
            }

            PopulateCategoriesAndAccounts();
            return View(transaction);
        }


        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            PopulateCategoriesAndAccounts();
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,CategoryId,AccountId,Amount,Note,Date")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTransaction = await _context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    var targetAccount = await _context.Accounts.FindAsync(originalTransaction.AccountId);
                    var categoryType = _context.Categories
                    .Where(c => c.Id == transaction.CategoryId)
                    .Select(c => c.Type)
                    .FirstOrDefault();
                    if (targetAccount != null && categoryType != null)
                    {
                        if (categoryType == "Expense" && targetAccount.Income < transaction.Amount)
                        {
                            ModelState.AddModelError(string.Empty, "Insufficient funds in the account for this expense category.");
                            PopulateCategoriesAndAccounts();
                            return View(transaction);
                        }

                        if (categoryType == "Income")
                        {
                            targetAccount.Income -= originalTransaction.Amount;
                            _context.Update(transaction);
                            targetAccount.Income += transaction.Amount;

                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                        else if (categoryType == "Expense")
                        {
                            targetAccount.Expanse -= originalTransaction.Amount;
                            targetAccount.Income += originalTransaction.Amount;
                            _context.Update(transaction);
                            
                            targetAccount.Income -= transaction.Amount;
                            targetAccount.Expanse += transaction.Amount;

                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                        _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
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
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
            var targetAccount = transaction.Account;
            var categoryType = _context.Categories
            .Where(c => c.Id == transaction.CategoryId)
            .Select(c => c.Type)
            .FirstOrDefault();

            if (transaction == null)
            {
                return NotFound();
            }

            if (categoryType == "Income") 
            {
                targetAccount.Income -= transaction.Amount;
            }
            else if (categoryType == "Expense")
            {
                targetAccount.Expanse -= transaction.Amount;
                targetAccount.Income += transaction.Amount;
            }

            try
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(transaction.Id))
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

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        public void PopulateCategoriesAndAccounts()
        {
            var CategoryCollection = _context.Categories.ToList();
            Category DefaultCategory = new Category() { Id = 0, CategoryName = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;

            var AccountsCollection = _context.Accounts.ToList();
            Account DefaultAccount = new Account() { Id = 0, AccountName = "Choose an Account" };
            AccountsCollection.Insert(0, DefaultAccount);
            ViewBag.Accounts = AccountsCollection;
        }
    }
}
