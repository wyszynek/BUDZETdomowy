using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBudget.Data;
using HomeBudget.Models;
using ClosedXML.Excel;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage;

namespace HomeBudget.Controllers
{
    [Authorize]
    public class TransactionBetweenAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionBetweenAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToCSV()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var transactionBetweenAccounts = _context.TransactionBetweenAccounts
                .Include(t => t.SenderAccount)
                .Include(t => t.RecipientAccount)
                .Include(t => t.Currency)
                .Where(t => t.UserId == currentUserId)
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Sender; Recipient; Amount; Date");
            foreach (var TBA in transactionBetweenAccounts)
            {
                string senderName = TBA.SenderAccount?.AccountName ?? "Unknown";
                string recipientName = TBA.RecipientAccount?.AccountName ?? "Unknown";
                builder.AppendLine($"{senderName}; {recipientName}; {TBA.Amount}; {TBA.Currency.Code}; {TBA.Date}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "TransactionBetweenAccounts.csv");
        }

        public IActionResult ExportToExcel()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var transactionBetweenAccounts = _context.TransactionBetweenAccounts
                .Include(t => t.SenderAccount)
                .Include(t => t.RecipientAccount)
                .Include(t => t.Currency)
                .Where(t => t.UserId == currentUserId)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("TransactionBetweenAccounts");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Sender";
                worksheet.Cell(currentRow, 2).Value = "Recipient";
                worksheet.Cell(currentRow, 3).Value = "Amount";
                worksheet.Cell(currentRow, 4).Value = "Currency";
                worksheet.Cell(currentRow, 5).Value = "Date";

                foreach (var TBA in transactionBetweenAccounts)
                {
                    currentRow++;
                    string senderName = TBA.SenderAccount?.AccountName ?? "Unknown";
                    string recipientName = TBA.RecipientAccount?.AccountName ?? "Unknown";
                    worksheet.Cell(currentRow, 1).Value = senderName;
                    worksheet.Cell(currentRow, 2).Value = recipientName;
                    worksheet.Cell(currentRow, 3).Value = TBA.Amount;
                    worksheet.Cell(currentRow, 4).Value = TBA.Currency.Code;
                    worksheet.Cell(currentRow, 5).Value = TBA.Date;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TransactionBetweenAccounts.xlsx");
                }
            }
        }

        // GET: TransactionBetweenAccounts
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            var applicationDbContext = _context.TransactionBetweenAccounts.Include(t => t.RecipientAccount).Include(t => t.SenderAccount).Include(t => t.Currency).Where(t => t.UserId == currentUserId);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var applicationDbContext = _context.TransactionBetweenAccounts.Include(t => t.RecipientAccount).Include(t => t.SenderAccount).Include(t => t.Currency).Include(t => t.User);
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
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);
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
        public async Task<IActionResult> Create([Bind("TransactionId,SenderId,RecipientId,Amount,Note,Date,CurrencyId")] TransactionBetweenAccounts transactionBetweenAccounts)
        {
            transactionBetweenAccounts.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(transactionBetweenAccounts);

            if (ModelState.IsValid)
            {
                var senderAccount = await _context.Accounts.FindAsync(transactionBetweenAccounts.SenderId);
                var recipientAccount = await _context.Accounts.FindAsync(transactionBetweenAccounts.RecipientId);

                if (senderAccount != null && recipientAccount != null && senderAccount != recipientAccount)
                {
                    var senderCurrency = await _context.Currencies.FirstAsync(x => x.Id == senderAccount.CurrencyId);
                    var recipientCurrency = await _context.Currencies.FirstAsync(x => x.Id == recipientAccount.CurrencyId);
                    var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transactionBetweenAccounts.CurrencyId);

                    var senderCurrencyExchange = await CurrencyRateHelper.Calculate(transactionBetweenAccounts.Amount, transactionCurrency.Code, senderCurrency.Code);
                    var recipientCurrencyExchange = await CurrencyRateHelper.Calculate(transactionBetweenAccounts.Amount, transactionCurrency.Code, recipientCurrency.Code);

                    if (senderAccount.Income >= senderCurrencyExchange)
                    {
                        senderAccount.Expanse += senderCurrencyExchange;
                        recipientAccount.Income += recipientCurrencyExchange;
                        senderAccount.Income -= senderCurrencyExchange;

                        _context.Add(transactionBetweenAccounts);
                        await _context.SaveChangesAsync();

                        TempData["ToastrMessage"] = "Transaction has been created successfully";
                        TempData["ToastrType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Insufficient funds in the sender's account.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Same account detected.");
                }
            }
            // Wypełnij listę kont przed wyświetleniem widoku
            PopulateAccount();
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
            PopulateAccount();            
            return View(transactionBetweenAccounts);
        }

        // POST: TransactionBetweenAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,SenderId,RecipientId,Amount,Note,Date,CurrencyId")] TransactionBetweenAccounts transactionBetweenAccounts)
        {
            transactionBetweenAccounts.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(transactionBetweenAccounts);

            if (id != transactionBetweenAccounts.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTransaction = await _context.TransactionBetweenAccounts.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    var originalTransactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == originalTransaction.CurrencyId);

                    var originalSenderAccount = await _context.Accounts.FindAsync(originalTransaction.SenderId);
                    var originalRecipientAccount = await _context.Accounts.FindAsync(originalTransaction.RecipientId);
                    var originalSenderCurrency = await _context.Currencies.FirstAsync(x => x.Id == originalSenderAccount.CurrencyId);
                    var originalRecipientCurrency = await _context.Currencies.FirstAsync(x => x.Id == originalRecipientAccount.CurrencyId);

                    if (originalSenderAccount != null && originalRecipientAccount != null && originalSenderAccount != originalRecipientAccount)
                    {
                        var originalSenderCurrencyExchange = await CurrencyRateHelper.Calculate(originalTransaction.Amount, originalTransactionCurrency.Code, originalSenderCurrency.Code);
                        var originalRecipientCurrencyExchange = await CurrencyRateHelper.Calculate(originalTransaction.Amount, originalTransactionCurrency.Code, originalRecipientCurrency.Code);

                        originalSenderAccount.Income += originalSenderCurrencyExchange;
                        originalRecipientAccount.Income -= originalRecipientCurrencyExchange;
                        originalSenderAccount.Expanse -= originalSenderCurrencyExchange;

                        var senderAccount = await _context.Accounts.FindAsync(transactionBetweenAccounts.SenderId);
                        var recipientAccount = await _context.Accounts.FindAsync(transactionBetweenAccounts.RecipientId);

                        if (senderAccount != null && recipientAccount != null && senderAccount != recipientAccount)
                        {
                            if (senderAccount.Income >= transactionBetweenAccounts.Amount)
                            {
                                var senderCurrency = await _context.Currencies.FirstAsync(x => x.Id == senderAccount.CurrencyId);
                                var recipientCurrency = await _context.Currencies.FirstAsync(x => x.Id == recipientAccount.CurrencyId);
                                var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transactionBetweenAccounts.CurrencyId);

                                var senderCurrencyExchange = await CurrencyRateHelper.Calculate(transactionBetweenAccounts.Amount, transactionCurrency.Code, senderCurrency.Code);
                                var recipientCurrencyExchange = await CurrencyRateHelper.Calculate(transactionBetweenAccounts.Amount, transactionCurrency.Code, recipientCurrency.Code);

                                senderAccount.Expanse += senderCurrencyExchange;
                                recipientAccount.Income += recipientCurrencyExchange;
                                senderAccount.Income -= senderCurrencyExchange;

                                _context.Update(transactionBetweenAccounts);
                                await _context.SaveChangesAsync();

                                TempData["ToastrMessage"] = "Transaction has been edited successfully";
                                TempData["ToastrType"] = "info";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Insufficient funds in the sender's account after editing.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Same account detected.");
                        }

                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionBetweenAccountsExists(transactionBetweenAccounts.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            PopulateAccount();
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
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var transactionBetweenAccounts = await _context.TransactionBetweenAccounts
                .Include(t => t.SenderAccount)
                .Include(t => t.RecipientAccount)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transactionBetweenAccounts == null)
            {
                return NotFound();
            }

            var senderAccount = await _context.Accounts.FindAsync(transactionBetweenAccounts.SenderId);
            var recipientAccount = await _context.Accounts.FindAsync(transactionBetweenAccounts.RecipientId);
            var senderCurrency = await _context.Currencies.FirstAsync(x => x.Id == senderAccount.CurrencyId);
            var recipientCurrency = await _context.Currencies.FirstAsync(x => x.Id == recipientAccount.CurrencyId);
            var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transactionBetweenAccounts.CurrencyId);

            var senderCurrencyExchange = await CurrencyRateHelper.Calculate(transactionBetweenAccounts.Amount, transactionCurrency.Code, senderCurrency.Code);
            var recipientCurrencyExchange = await CurrencyRateHelper.Calculate(transactionBetweenAccounts.Amount, transactionCurrency.Code, recipientCurrency.Code);

            senderAccount.Income += senderCurrencyExchange;
            senderAccount.Expanse -= senderCurrencyExchange;
            recipientAccount.Income -= recipientCurrencyExchange;

            try
            {
                _context.TransactionBetweenAccounts.Remove(transactionBetweenAccounts);
                await _context.SaveChangesAsync();
                TempData["ToastrMessage"] = "Transaction has been deleted successfully";
                TempData["ToastrType"] = "warning";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionBetweenAccountsExists(transactionBetweenAccounts.Id))
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


        private bool TransactionBetweenAccountsExists(int id)
        {
            return _context.TransactionBetweenAccounts.Any(e => e.Id == id);
        }

        public void PopulateAccount()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);

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
