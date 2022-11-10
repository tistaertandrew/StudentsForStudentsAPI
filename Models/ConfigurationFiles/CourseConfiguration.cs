using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StudentsForStudentsAPI.Models.ConfigurationFiles
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");
            builder.HasData(
                new
                {
                    Id = 1,
                    Label = "UE1 - Programmation de base (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 2,
                    Label = "UE2 - Architecture des ordinateurs (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 3,
                    Label = "UE3 - Conception d'applications 1 (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 4,
                    Label = "UE35 - Communication écrite et orale en langue française (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 5,
                    Label = "UE4 - Mathématiques appliquées à l'informatique 1 (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 6,
                    Label = "UE5 - Base de données (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 7,
                    Label = "UE6 - Développement web (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 8,
                    Label = "UE7 - Anglais (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 9,
                    Label = "UE8 - E-Business (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 10,
                    Label = "UE9 - Programmation intermédiaire (B1)",
                    CursusId = 1
                },
                new
                {
                    Id = 11,
                    Label = "UE13 - Systèmes d'exploitation (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 12,
                    Label = "UE14 - Anglais (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 13,
                    Label = "UE15 - Droit et Ethique du monde numérique (B2)",
                    CursusId = 1,
                },
                new
                {
                    Id = 14,
                    Label = "UE16 - Digitalisation et nouvelle économie (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 15,
                    Label = "UE17 - Mathématiques appliquées à l'informatique 2 (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 16,
                    Label = "UE18 - Développement mobile (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 17,
                    Label = "UE19 - Développement web avancé (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 18,
                    Label = "UE20 - Langage de scripts dynamiques (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 19,
                    Label = "UE21 - Réseaux informatiques (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 20,
                    Label = "UE36 - Programmation avancée (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 21,
                    Label = "UE10 - Conception d'applications 2 (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 22,
                    Label = "UE22 - Laboratoire pluridisciplinaire (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 23,
                    Label = "UE23 - SALTo (B2)",
                    CursusId = 1
                },
                new
                {
                    Id = 24,
                    Label = "UE25 - Architectures logicielles (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 25,
                    Label = "UE26 - Frameworks web (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 26,
                    Label = "UE27 - Entrepreneuriat (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 27,
                    Label = "UE28 - Savoir être, culture d'entreprise (B3)",
                    CursusId = 1,
                },
                new
                {
                    Id = 28,
                    Label = "UE29 - Informatique managériale (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 29,
                    Label = "UE30 - Stage et travail de fin d'études (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 30,
                    Label = "UE31 - Mémoire (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 31,
                    Label = "UE32 - Administration réseau et système (LINUX) (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 32,
                    Label = "UE24 - Administration réseau et système (WINDOWS) (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 33,
                    Label = "UE33 - Conférences - Visites - Séminaires (B3)",
                    CursusId = 1
                },
                new
                {
                    Id = 34,
                    Label = "UE34 - SALTo (B3)",
                    CursusId = 1
                }
                );
        }
    }
}
