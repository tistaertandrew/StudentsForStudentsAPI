namespace StudentsForStudentsAPI.Models.DbModels
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
        
        public Request() { }

        public Request(string name, string description, User sender, Place place, Course course)
        {
            Name = name;
            Description = description;
            Status = false;
            Date = DateTime.Now;
            Sender = sender;
            Handler = null;
            Place = place;
            Course = course;
        }
    }
}
