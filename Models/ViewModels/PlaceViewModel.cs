using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class PlaceViewModel
    {
        [Required]
        public string Street { get; set; } = string.Empty;
        [Required]
        public int PostalCode { get; set; } = 0;
        [Required]
        public string Number { get; set; } = string.Empty;
        [Required]
        public string Locality { get; set; } = string.Empty;
    }
}
