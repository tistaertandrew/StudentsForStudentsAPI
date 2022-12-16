namespace StudentsForStudentsAPI.Models
{
    public class Form
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; } = false;
        public string? SenderEmail { get; set; }
        public User? Sender { get; set; }
        public User? Handler { get; set; }
        
        public Form() { }
        
        public Form(string subject, string message, string? senderEmail = null, User? sender = null)
        {
            Date = DateTime.Now;
            Subject = subject;
            Message = message;
            Status = false;

            if (sender == null) SenderEmail = senderEmail;
            else Sender = sender;
        }
    }
}
