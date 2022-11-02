using Microsoft.AspNetCore.Identity;

namespace StudentsForStudentsAPI
{
    public static class Seed
    {
        public static async Task AddDefaultRole(RoleManager<IdentityRole> rm)
        {
            IdentityRole role = rm.FindByIdAsync("1").Result;
            if (role == null)
            {
                await rm.CreateAsync(new IdentityRole("Admin"));
                await rm.CreateAsync(new IdentityRole("Member"));
            }
        }
    }
}
