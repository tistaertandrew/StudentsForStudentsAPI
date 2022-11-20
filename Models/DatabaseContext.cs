using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models.ConfigurationFiles;

namespace StudentsForStudentsAPI.Models
{
    public class DatabaseContext : IdentityDbContext<User>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new SectionConfiguration());
            builder.ApplyConfiguration(new CursusConfiguration());
            builder.ApplyConfiguration(new CourseConfiguration());
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Cursus> Cursus { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<File> Files { get; set; }
    }
}
