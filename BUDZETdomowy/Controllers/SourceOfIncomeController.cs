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
            return View(await _context.SourceOfIncomes.ToListAsync());
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
            if (ModelState.IsValid)
            {
                sourceOfIncome.Ratio = CalculateRatio(sourceOfIncome);
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
            return View(sourceOfIncome);
        }

        // POST: SourceOfIncome/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContractType,SalaryType,VAT,HealthInsuranceType,Ratio")] SourceOfIncome sourceOfIncome)
        {
            if (id != sourceOfIncome.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
                return RedirectToAction(nameof(Index));
            }
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
            return (decimal)sourceOfIncome.ContractType + (decimal)sourceOfIncome.SalaryType / (decimal)sourceOfIncome.HealthInsuranceType;
        }
    }
}
