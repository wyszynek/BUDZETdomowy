namespace HomeBudget.Models
{
    public class MainPageViewModel
    {
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
