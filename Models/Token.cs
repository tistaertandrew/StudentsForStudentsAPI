using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StudentsForStudentsAPI.Models.DbModels;

namespace StudentsForStudentsAPI.Models
{
    public static class Token
    {
        public static string CreateToken(User user, UserManager<User> userManager, IConfiguration config)
        {
            var roles = userManager.GetRolesAsync(user).Result;
            var claims = new List<Claim>();

            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("AppSettings:JwtSecret").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(6),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
