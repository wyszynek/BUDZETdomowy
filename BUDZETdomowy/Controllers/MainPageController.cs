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

            // Pobierz datę początkową (7 dni wstecz)
            DateTime startDate = DateTime.Today.AddDays(-6);

            // Pobierz dane transakcji dla ostatnich 7 dni
            var transactionsData = await _context.Transactions
                .Where(t => t.UserId == currentUserId && t.Date >= startDate)
                .GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Income = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount),
                    Expense = g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            // Utwórz listę dni i uzupełnij brakujące dni w przypadku braku transakcji
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

            // Calculate total income and total expenses for the current user
            decimal totalIncome = await _context.Transactions
                .Where(t => t.UserId == currentUserId && t.Category.Type == "Income")
                .SumAsync(t => t.Amount);

            decimal totalExpense = await _context.Transactions
                .Where(t => t.UserId == currentUserId && t.Category.Type == "Expense")
                .SumAsync(t => t.Amount);

            ViewBag.TotalIncome = totalIncome.ToString("C0");
            ViewBag.TotalExpense = totalExpense.ToString("C0");
            ViewBag.Balance = (totalIncome - totalExpense).ToString("C0");

            return View();
        }
    }
}
