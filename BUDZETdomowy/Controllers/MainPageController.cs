using HomeBudget.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using HomeBudget.Data;
using Microsoft.AspNetCore.Authorization;

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

        public async Task<ActionResult> Index()
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

            //Last 7 Days
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions.Include(x => x.Category).Where(y => y.Date >= StartDate && y.Date <= EndDate).Where(y => y.UserId.ToString() == currentUserId).ToListAsync();
            
            // Calculate total income and total expenses for the current user
            decimal totalIncome = await _context.Transactions
                .Where(t => t.UserId.ToString() == currentUserId && t.Category.Type == "Income")
                .SumAsync(t => t.Amount);

            decimal totalExpense = await _context.Transactions
                .Where(t => t.UserId.ToString() == currentUserId && t.Category.Type == "Expense")
                .SumAsync(t => t.Amount);

            ViewBag.TotalIncome = totalIncome.ToString("C0");
            ViewBag.TotalExpense = totalExpense.ToString("C0");
            ViewBag.Balance = (totalIncome - totalExpense).ToString("C0");

            //Spline Chart - Income vs Expense

            //Income
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(x => x.UserId.ToString() == currentUserId)
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = (int)k.Sum(l => l.Amount)
                })
                .ToList();

            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(x => x.UserId.ToString() == currentUserId)
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = (int)k.Sum(l => l.Amount)
                })
                .ToList();

            //Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
        ViewBag.RecentTransactions = await _context.Transactions
            .Include(i => i.Category)
            .Where(t => t.UserId.ToString() == currentUserId)
            .OrderByDescending(j => j.Date)
            .Take(5)
            .ToListAsync();


            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}
