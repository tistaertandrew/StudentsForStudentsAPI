using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
                },
                new
                {
                    Id = 2,
                    Label = "Cybersécurité",
                    SectionId = 1
                },
                new
                {
                    Id = 3,
                    Label = "Marketing",
                    SectionId = 2
                },
                new
                {
                    Id = 4,
                    Label = "Droit",
                    SectionId = 2
                },
                new
                {
                    Id = 5,
                    Label = "Commerce Extérieur",
                    SectionId = 2
                }
                );
        }
    }
}
