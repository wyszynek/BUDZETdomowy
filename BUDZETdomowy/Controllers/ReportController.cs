using HomeBudget.Data;
using HomeBudget.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace HomeBudget.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetTestData")]
        public IEnumerable<string> GetTestData()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("GetAllTransactions")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions()
        {
            return await _context.Transactions.ToListAsync();
        }

        [HttpGet("GeneratePDFReport")]
        public async Task<IActionResult> GeneratePDFReport()
        {
            var transactions = await _context.Transactions.ToListAsync();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("Transaction Report"));

                foreach (var transaction in transactions)
                {
                    var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transaction.CurrencyId);
                    var transactionCategory = await _context.Categories.FirstAsync(x => x.Id == transaction.CategoryId);
                    var transactionUser = await _context.Users.FirstAsync(x => x.Id == transaction.UserId);
                    document.Add(new Paragraph($"User: {transactionUser.UserName}; Date: {transaction.Date}; Amount: {transaction.Amount} {transactionCurrency.Code}; Category: {transactionCategory.CategoryName} - {transactionCategory.Type}"));
                }

                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "TransactionReport.pdf");
            }
        }
    }
}
