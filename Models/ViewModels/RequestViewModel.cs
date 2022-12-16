using StudentsForStudentsAPI.Models.DbModels;

namespace StudentsForStudentsAPI.Models.ViewModels;

public class RequestViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Date { get; set; }
    public bool Status { get; set; }
    public string Sender { get; set; }
    public string Handler { get; set; }
    public Place Place { get; set; }
    public Course Course { get; set; }
    
    public RequestViewModel(Request request)
    {
        Id = request.Id;
        Name = request.Name;
        Description = request.Description;
        Date = request.Date.ToString("dd/MM/yyyy");
        Status = request.Status;
        Sender = request.Sender.UserName;
        Handler = request.Handler != null ? request.Handler.UserName : "nobody";
        Place = request.Place;
        Course = request.Course;
    }
}