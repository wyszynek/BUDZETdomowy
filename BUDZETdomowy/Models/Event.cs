namespace HomeBudget.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
