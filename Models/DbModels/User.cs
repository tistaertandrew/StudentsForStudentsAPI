using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace StudentsForStudentsAPI.Models.DbModels
{
    public class User : IdentityUser, IEqualityComparer<User>
    {
        public Cursus Cursus { get; set; }
        public string? CalendarLink { get; set; } = null;
        public bool IsBanned { get; set; } = false;

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