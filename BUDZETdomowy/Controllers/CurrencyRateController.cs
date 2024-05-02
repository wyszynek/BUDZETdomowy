using Microsoft.AspNetCore.Mvc;
using HomeBudget.Models;

namespace HomeBudget.Controllers
{
    public class CurrencyRateController : Controller
    {
        // GET: CurrencyRate
        public async Task<IActionResult> Index()
        {
            return View(await CurrencyRateHelper.GetCurrencyRates());
        }
    }
}
