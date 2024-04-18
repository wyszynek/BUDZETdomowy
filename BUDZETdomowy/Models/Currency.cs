namespace HomeBudget.Models
{
    public class Currency
    {
        public int CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyString { get; set; }
        public string Display => CurrencyString + " (" + CurrencySymbol + ")";
    }
}
