using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models
{
    public class OAuthViewModel
    {
        [Required]
        public string Credentials { get; set; }

        public OAuthViewModel(string credentials)
        {
            this.Credentials = credentials;
        }
    }
}
