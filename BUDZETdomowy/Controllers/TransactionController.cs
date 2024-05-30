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
using HomeBudget.Migrations;

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
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var applicationDbContext = _context.Transactions.Include(t => t.Account).Include(t => t.Currency).Include(t => t.Category).Where(t => t.UserId == currentUserId);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult ExportToCSV()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Include(t => t.Currency)
                .Where(t => t.UserId == currentUserId)
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Category, Account, Amount, Date");

            foreach (var transaction in transactions)
            {
                string categoryName = transaction.Category?.CategoryName ?? "Unknown";
                string accountName = transaction.Account?.AccountName ?? "Unknown";
                builder.AppendLine($"{categoryName}, {accountName}, {transaction.Amount}, {transaction.Currency.Code}, {transaction.Date}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "transactions.csv");
        }

        public IActionResult ExportToExcel()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Include(t => t.Currency)
                .Where(t => t.UserId == currentUserId)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Transactions");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Category";
                worksheet.Cell(currentRow, 2).Value = "Account";
                worksheet.Cell(currentRow, 3).Value = "Amount";
                worksheet.Cell(currentRow, 4).Value = "Currency";
                worksheet.Cell(currentRow, 5).Value = "Date";

                foreach (var transaction in transactions)
                {
                    currentRow++;
                    string categoryName = transaction.Category?.CategoryName ?? "Unknown";
                    string accountName = transaction.Account?.AccountName ?? "Unknown";
                    worksheet.Cell(currentRow, 1).Value = categoryName;
                    worksheet.Cell(currentRow, 2).Value = accountName;
                    worksheet.Cell(currentRow, 3).Value = transaction.Amount;
                    worksheet.Cell(currentRow, 4).Value = transaction.Currency.Code;
                    worksheet.Cell(currentRow, 5).Value = transaction.Date;
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
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: AddFundsFromSource
        public IActionResult AddFundsFromSource()
        {
            var viewModel = new TransactionViewModel();
            PopulateCategoriesAndAccounts();
            return View(viewModel);
        }

        // POST: AddFundsFromSource
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFundsFromSource(TransactionViewModel viewModel)
        {
            viewModel.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(viewModel);

            var workCategory = _context.Categories.FirstOrDefault(t => t.CategoryName == "Work" && t.Icon == "&#128184;" && t.Type == "Income" && t.UserId == viewModel.UserId);

            if (ModelState.IsValid)
            {
                if (workCategory != null)
                {
                    var sourceOfIncome = _context.SourceOfIncomes.FirstOrDefault(s => s.Id == viewModel.SourceOfIncomeId);
                    var targetAccount = _context.Accounts.FirstOrDefault(s => s.Id == viewModel.AccountId);
                    var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == viewModel.CurrencyId);
                    var targetAccountCurrency = await _context.Currencies.FirstAsync(x => x.Id == targetAccount.CurrencyId);

                    if (sourceOfIncome != null && targetAccount != null)
                    {
                        var income = sourceOfIncome.Ratio * viewModel.Amount;
                        var currencyExchange = await CurrencyRateHelper.Calculate(income, transactionCurrency.Code, targetAccountCurrency.Code);
                        targetAccount.Income += currencyExchange;

                        var transaction = new Transaction
                        {
                            CategoryId = workCategory.Id,
                            AccountId = viewModel.AccountId,
                            Amount = income,
                            Date = viewModel.Date,
                            CurrencyId = viewModel.CurrencyId,
                            UserId = viewModel.UserId,
                        };

                        _context.Add(transaction);
                        await _context.SaveChangesAsync(); 
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Source of income not found.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Work category not found.");
                }
            }

            PopulateCategoriesAndAccounts();
            return View(viewModel);
        }



        // GET: Transaction/Create
        public IActionResult Create()
        {
            PopulateCategoriesAndAccounts();
            return View(new Transaction());
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,CategoryId,AccountId,Amount,Note,Date,CurrencyId")] Transaction transaction)
        {
            transaction.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(transaction);

            if (ModelState.IsValid)
            {
                // Pobierz konta i kategorię na podstawie ich identyfikatorów
                var targetAccount = await _context.Accounts.FindAsync(transaction.AccountId);
                var categoryType = _context.Categories
                    .Where(c => c.Id == transaction.CategoryId)
                    .Select(c => c.Type)
                    .FirstOrDefault();

                var budgets = _context.Budgets.Where(b => b.AccountId == transaction.AccountId && b.CategoryId == transaction.CategoryId).ToList();

                var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transaction.CurrencyId);
                var targetAccountCurrency = await _context.Currencies.FirstAsync(x => x.Id == targetAccount.CurrencyId);
                var currencyExchange = await CurrencyRateHelper.Calculate(transaction.Amount, transactionCurrency.Code, targetAccountCurrency.Code);

                if (targetAccount != null && categoryType != null)
                {
                    if (categoryType == "Expense" && targetAccount.Income < currencyExchange)
                    {
                        ModelState.AddModelError(string.Empty, "Insufficient funds in the account for this expense category.");
                        PopulateCategoriesAndAccounts();
                        return View(transaction);
                    }

                    if (categoryType == "Income")
                    {
                        targetAccount.Income += currencyExchange;

                        _context.Add(transaction);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else if (categoryType == "Expense")
                    {
                        targetAccount.Expanse += currencyExchange;
                        targetAccount.Income -= currencyExchange;

                        foreach (var budget in budgets)
                        {
                            if (transaction.Date >= budget.CreationTime && transaction.Date <= budget.EndTime)
                            {
                                budget.BudgetProgress += currencyExchange;
                            }
                        }

                        _context.Add(transaction);
                        await _context.SaveChangesAsync();

                        TempData["ToastrMessage"] = "Transaction has been created successfully";
                        TempData["ToastrType"] = "success";
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
        public async Task<IActionResult> EditFundsFromSource(int? id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFundsFromSource(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Zapisz zmiany w transakcji
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

            var categoryIcon = _context.Categories
                .Where(c => c.Id == transaction.CategoryId)
                .Select(c => c.Icon)
                .FirstOrDefault();

            //if (categoryIcon == "&#128184;")
            //{
            //    // Jeśli kategoria jest specjalna, przekieruj użytkownika na inny widok
            //    return RedirectToAction("EditFundsFromSource", new { id = transaction.Id });
            //}

            if (categoryIcon == "&#128184;")
            {
                TempData["ToastrMessage"] = "You cannot edit this transaction.";
                TempData["ToastrType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            PopulateCategoriesAndAccounts();
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,CategoryId,AccountId,Amount,Note,Date,CurrencyId")] Transaction transaction)
        {
            transaction.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(transaction);
            PopulateCategoriesAndAccounts();

            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTransaction = await _context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    var originalAccount = await _context.Accounts.FindAsync(originalTransaction.AccountId);
                    var originalTransactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == originalTransaction.CurrencyId);
                    var originalAccountCurrency = await _context.Currencies.FirstAsync(x => x.Id == originalAccount.CurrencyId);
                    var originalCurrencyExchange = await CurrencyRateHelper.Calculate(originalTransaction.Amount, originalTransactionCurrency.Code, originalAccountCurrency.Code);
                    var originalBudgets = _context.Budgets.Where(b => b.AccountId == originalTransaction.AccountId && b.CategoryId == originalTransaction.CategoryId).ToList();

                    var categoryType = _context.Categories
                        .Where(c => c.Id == transaction.CategoryId)
                        .Select(c => c.Type)
                        .FirstOrDefault();

                    if (originalAccount != null && categoryType != null)
                    {
                        var targetAccount = await _context.Accounts.FindAsync(transaction.AccountId);
                        var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transaction.CurrencyId);
                        var targetAccountCurrency = await _context.Currencies.FirstAsync(x => x.Id == targetAccount.CurrencyId);
                        var currencyExchange = await CurrencyRateHelper.Calculate(transaction.Amount, transactionCurrency.Code, targetAccountCurrency.Code);
                        var budgets = _context.Budgets.Where(b => b.AccountId == transaction.AccountId && b.CategoryId == transaction.CategoryId).ToList();

                        if (categoryType == "Expense" && targetAccount.Income < currencyExchange)
                        {
                            ModelState.AddModelError(string.Empty, "Insufficient funds in the account for this expense category.");
                            PopulateCategoriesAndAccounts();
                            return View(transaction);
                        }

                        if (categoryType == "Income")
                        {
                            //oddajemy to co zabralismy
                            originalAccount.Income -= originalCurrencyExchange;

                            //przydzielamy do nowej osoby
                            targetAccount.Income += currencyExchange;

                            _context.Update(transaction);

                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                        else if (categoryType == "Expense")
                        {
                            //oddajemy co zabralismy
                            originalAccount.Expanse -= originalCurrencyExchange;
                            originalAccount.Income += originalCurrencyExchange;

                            foreach (var originalBudget in originalBudgets)
                            {
                                if (originalTransaction.Date >= originalBudget.CreationTime && originalTransaction.Date <= originalBudget.EndTime)
                                {
                                    originalBudget.BudgetProgress += currencyExchange;
                                }
                            }

                            //przydzielamy na nowo
                            targetAccount.Income -= currencyExchange;
                            targetAccount.Expanse += currencyExchange;

                            //dodajemy zaktualizowany budzet
                            foreach (var budget in budgets)
                            {
                                if (transaction.Date >= budget.CreationTime && transaction.Date <= budget.EndTime)
                                {
                                    budget.BudgetProgress += currencyExchange;
                                }
                            }

                            _context.Update(transaction);

                            await _context.SaveChangesAsync();

                            TempData["ToastrMessage"] = "Transaction has been edited successfully";
                            TempData["ToastrType"] = "info";
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
                .Include(a => a.Currency)
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

            var targetAccount = await _context.Accounts.FindAsync(transaction.AccountId);
            var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transaction.CurrencyId);
            var targetAccountCurrency = await _context.Currencies.FirstAsync(x => x.Id == targetAccount.CurrencyId);
            var currencyExchange = await CurrencyRateHelper.Calculate(transaction.Amount, transactionCurrency.Code, targetAccountCurrency.Code);
            
            var categoryType = _context.Categories
                .Where(c => c.Id == transaction.CategoryId)
                .Select(c => c.Type)
                .FirstOrDefault();

            var budgets = _context.Budgets.Where(b => b.AccountId == transaction.AccountId && b.CategoryId == transaction.CategoryId).ToList();

            if (transaction == null)
            {
                return NotFound();
            }

            if (categoryType == "Income") 
            {
                targetAccount.Income -= currencyExchange;
            }
            else if (categoryType == "Expense")
            {
                targetAccount.Expanse -= currencyExchange;
                targetAccount.Income += currencyExchange;

                foreach (var budget in budgets)
                {
                    if (transaction.Date >= budget.CreationTime && transaction.Date <= budget.EndTime)
                    {
                        budget.BudgetProgress -= currencyExchange;
                    }
                }
            }

            try
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["ToastrMessage"] = "Transaction has been deleted successfully";
                TempData["ToastrType"] = "warning";
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
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);

            var userCategories = _context.Categories.Where(c => c.UserId == currentUserId).ToList();
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
    }
}
