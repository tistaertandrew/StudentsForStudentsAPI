using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentsForStudentsAPI.Models.DbModels;

namespace StudentsForStudentsAPI.Models.ConfigurationFiles
{
    public class CursusConfiguration : IEntityTypeConfiguration<Cursus>
    {
        public void Configure(EntityTypeBuilder<Cursus> builder)
        {
            builder.ToTable("Cursus");
            builder.HasData(
                new
                {
                    Id = 1,
                    Label = "Développement d'applications",
                    SectionId = 1
                }
                );
        }
    }
}
