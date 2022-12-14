using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentsForStudentsAPI.Models.DbModels;

namespace StudentsForStudentsAPI.Models.ConfigurationFiles
{
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.ToTable("Sections");
            builder.HasData(
                new Section
                {
                    Id = 1,
                    Label = "Technique"
                }
                );
        }
    }
}
