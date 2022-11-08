namespace StudentsForStudentsAPI.Models
{
    public class Form
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Message { get; set; }
        public bool Status { get; set; } = false;
        public string? SenderEmail { get; set; }
        public User? Sender { get; set; }
        public User? Handler { get; set; }
    }
}
