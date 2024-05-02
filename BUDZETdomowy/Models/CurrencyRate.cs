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
    }
}
