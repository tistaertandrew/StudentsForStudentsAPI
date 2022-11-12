namespace StudentsForStudentsAPI.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public DateTime Date { get; set; }
        public User Sender { get; set; }
        public User? Handler { get; set; }
        public Place Place { get; set; }
        public Course Course { get; set; }

    }
}
