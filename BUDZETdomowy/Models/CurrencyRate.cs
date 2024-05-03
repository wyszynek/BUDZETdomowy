using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HomeBudget.Models
{
    public class CurrencyRate
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("currency")]
        public string Name { get; set; }

        [JsonProperty("mid")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public decimal Rate { get; set; }
    }

    public class CurrencyRateResponse
    {
        [JsonProperty("rates")]
        public IEnumerable<CurrencyRate> CurrencyRates { get; set; }
    }

    public class CurrencyRateHelper
    {
        private static HttpClient _httpClient = new ();

        public static async Task<IEnumerable<CurrencyRate>> GetCurrencyRates()
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://api.nbp.pl/api/exchangerates/tables/a/?format=json");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var currencyData = JsonConvert.DeserializeObject<IEnumerable<CurrencyRateResponse>>(content);

                return currencyData.First().CurrencyRates;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<decimal> Calculate(decimal amount, string sourceCurrencyCode, string targetCurrencyCode)
        {
            var currencyRates = await GetCurrencyRates();
            
            decimal sourceCurrencyRate;
            if (sourceCurrencyCode == "PLN")
            {
                sourceCurrencyRate = 1;
            }
            else
            {
                var sourceCurrency = currencyRates.FirstOrDefault(x => x.Code == sourceCurrencyCode);
                if (sourceCurrency is null)
                {
                    throw new Exception($"Source currency {sourceCurrencyCode} not found");
                }

                sourceCurrencyRate = sourceCurrency.Rate;
            }

            decimal targetCurrencyRate;
            if (targetCurrencyCode == "PLN")
            {
                targetCurrencyRate = 1;
            }
            else
            {
                var targetCurrency = currencyRates.FirstOrDefault(x => x.Code == targetCurrencyCode);
                if (targetCurrency is null)
                {
                    throw new Exception($"Target currency {targetCurrencyCode} not found");
                }

                targetCurrencyRate = targetCurrency.Rate;
            }

            return amount * sourceCurrencyRate / targetCurrencyRate;
        }
    }
}
