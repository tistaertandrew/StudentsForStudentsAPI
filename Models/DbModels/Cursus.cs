namespace StudentsForStudentsAPI.Models.DbModels
{
    public class Cursus
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public Section Section { get; set; }
    }
}
