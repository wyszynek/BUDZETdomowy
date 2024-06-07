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
using Microsoft.AspNetCore.Authorization;

namespace HomeBudget.Controllers
{
    public class SourceOfIncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SourceOfIncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SourceOfIncome
        public async Task<IActionResult> Index()
        {
            var currentUserId = UserHelper.GetCurrentUserId(HttpContext);
            return View(await _context.SourceOfIncomes.Where(x => x.UserId == currentUserId).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            return View(await _context.SourceOfIncomes.Include(x => x.User).ToListAsync());
        }

        // GET: SourceOfIncome/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceOfIncome = await _context.SourceOfIncomes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sourceOfIncome == null)
            {
                return NotFound();
            }

            return View(sourceOfIncome);
        }

        // GET: SourceOfIncome/Create
        public IActionResult Create()
        {
            PopulateEnums();
            return View();
        }

        // POST: SourceOfIncome/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContractType,SalaryType,VAT,HealthInsuranceType,Ratio")] SourceOfIncome sourceOfIncome)
        {
            sourceOfIncome.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(sourceOfIncome);

            if (ModelState.IsValid)
            {
                sourceOfIncome.Ratio = CalculateRatio(sourceOfIncome);
                TempData["ToastrMessage"] = "Source has been created successfully";
                TempData["ToastrType"] = "success";
                _context.Add(sourceOfIncome);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateEnums();
            return View(sourceOfIncome);
        }

        // GET: SourceOfIncome/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceOfIncome = await _context.SourceOfIncomes.FindAsync(id);
            if (sourceOfIncome == null)
            {
                return NotFound();
            }
            PopulateEnums();
            return View(sourceOfIncome);
        }

        // POST: SourceOfIncome/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContractType,SalaryType,VAT,HealthInsuranceType,Ratio")] SourceOfIncome sourceOfIncome)
        {
            sourceOfIncome.UserId = UserHelper.GetCurrentUserId(HttpContext);
            await TryUpdateModelAsync(sourceOfIncome);

            if (id != sourceOfIncome.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                sourceOfIncome.Ratio = CalculateRatio(sourceOfIncome);
                try
                {
                    _context.Update(sourceOfIncome);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SourceOfIncomeExists(sourceOfIncome.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["ToastrMessage"] = "Source has been edited successfully";
                TempData["ToastrType"] = "info";
                return RedirectToAction(nameof(Index));
            }
            PopulateEnums();
            return View(sourceOfIncome);
        }

        // GET: SourceOfIncome/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceOfIncome = await _context.SourceOfIncomes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sourceOfIncome == null)
            {
                return NotFound();
            }

            return View(sourceOfIncome);
        }

        // POST: SourceOfIncome/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sourceOfIncome = await _context.SourceOfIncomes.FindAsync(id);
            if (sourceOfIncome != null)
            {
                _context.SourceOfIncomes.Remove(sourceOfIncome);
            }

            TempData["ToastrMessage"] = "Source has been deleted successfully";
            TempData["ToastrType"] = "warning";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SourceOfIncomeExists(int id)
        {
            return _context.SourceOfIncomes.Any(e => e.Id == id);
        }

        private void PopulateEnums()
        {
            ViewBag.ContractType = new SelectList(Enum.GetValues(typeof(ContractType)).Cast<ContractType>().Select(e => new SelectListItem
            {
                Value = ((decimal)e).ToString(),
                Text = e.ToString()
            }).ToList(), "Value", "Text");

            ViewBag.SalaryType = new SelectList(Enum.GetValues(typeof(SalaryType)).Cast<SalaryType>().Select(e => new SelectListItem
            {
                Value = ((decimal)e).ToString(),
                Text = e.ToString()
            }).ToList(), "Value", "Text");

            ViewBag.HealthInsuranceType = new SelectList(Enum.GetValues(typeof(HealthInsuranceType)).Cast<HealthInsuranceType>().Select(e => new SelectListItem
            {
                Value = ((decimal)e).ToString(),
                Text = e.ToString()
            }).ToList(), "Value", "Text");
        }

        private decimal CalculateRatio(SourceOfIncome sourceOfIncome)
        {
            return NewContractWeight(sourceOfIncome.ContractType) + (decimal)sourceOfIncome.SalaryType / (decimal)sourceOfIncome.HealthInsuranceType / ((decimal)sourceOfIncome.VAT * (decimal)0.1);
        }

        private decimal NewContractWeight(ContractType contractType)
        {
            if (contractType == ContractType.OrderContract)
            {
                return 0.75m;
            }
            else if (contractType == ContractType.EmploymentContract)
            {
                return 1;
            }
            else if (contractType == ContractType.ContractForWork)
            {
                return 1.2m;
            }
            else if (contractType == ContractType.Practice)
            {
                return 0.5m;
            }

            return 0; 
        }

    }
}
