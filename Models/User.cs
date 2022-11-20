using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace StudentsForStudentsAPI.Models
{
    public class User : IdentityUser, IEqualityComparer<User>
    {
        public Cursus Cursus { get; set; }
        public string? CalendarLink { get; set; } = null;

        public bool Equals(User? x, User? y)
        {
            if (x == null || y == null) return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode([DisallowNull] User obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}