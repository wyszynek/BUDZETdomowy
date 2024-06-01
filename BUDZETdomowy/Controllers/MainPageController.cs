using HomeBudget.Data;
using HomeBudget.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeBudget.Controllers
{
    [Authorize]
    public class MainPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MainPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);

            //data początkowa (7 dni wstecz)
            DateTime startDate = DateTime.Today.AddDays(-6);

            //dane transakcji dla ostatnich 7 dni
            var transactions = await _context.Transactions
                .Where(t => t.UserId == currentUserId && t.Date >= startDate)
                .Include(t => t.Category)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                var transactionCurrency = await _context.Currencies.FirstAsync(x => x.Id == transaction.CurrencyId);
                transaction.Amount = await CurrencyRateHelper.Calculate(transaction.Amount, transactionCurrency.Code, "PLN");
            }

            var transactionsData = transactions.GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Income = g.Where(t => t.Category.Type == "Income").Select(t => t.Amount).Sum(),
                    Expense = g.Where(t => t.Category.Type == "Expense").Select(t => t.Amount).Sum()
                })
                .OrderBy(g => g.Date)
                .ToList();

            //lista dni i brakujące dni w przypadku braku transakcji
            List<DateTime> allDays = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i)).ToList();
            var chartData = allDays.Select(day => new
            {
                Date = day,
                Income = transactionsData.FirstOrDefault(d => d.Date.Date == day)?.Income ?? 0,
                Expense = transactionsData.FirstOrDefault(d => d.Date.Date == day)?.Expense ?? 0
            });

            ViewBag.ChartData = chartData.Select(d => new
            {
                Label = d.Date.ToString("dd-MMM"),
                Income = d.Income,
                Expense = d.Expense
            });

            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .Where(t => t.UserId == currentUserId)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            decimal totalIncome = await _context.Transactions
                .Where(t => t.UserId == currentUserId && t.Category.Type == "Income")
                .SumAsync(t => t.Amount);

            decimal totalExpense = await _context.Transactions
                .Where(t => t.UserId == currentUserId && t.Category.Type == "Expense")
                .SumAsync(t => t.Amount);

            ViewBag.TotalIncome = totalIncome.ToString("F2");
            ViewBag.TotalExpense = totalExpense.ToString("F2");
            ViewBag.Balance = (totalIncome - totalExpense).ToString("F2");

            // Prepare data for the doughnut chart
            var categoryExpenses = transactions
                .Where(t => t.Category.Type == "Expense")
                .GroupBy(t => t.Category.CategoryName)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            ViewBag.CategoryExpenses = categoryExpenses;

            return View();
        }
    }
}