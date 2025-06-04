namespace PAW_CATALOG_PROJ.Models
{
    public class Message
    {
        public int Id { get; set; }

        public  string? FromId { get; set; }
        public User? From { get; set; } = null!;

        public string? To { get; set; }
        public User? Recipient { get; set; } = null!;

        public string Content { get; set; } = null!;
    }

}
