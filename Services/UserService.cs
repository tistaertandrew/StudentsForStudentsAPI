using System.Security.Claims;

namespace StudentsForStudentsAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUserIdFromToken() 
            => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public bool IsTokenValid()
        {
            var exp = _httpContextAccessor?.HttpContext?.User?.FindFirstValue("exp");
            var date = exp != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime : DateTime.MinValue;
            return date > DateTime.Now;
        }

        public bool IsUserAdmin()
            => _httpContextAccessor?.HttpContext?.User?.IsInRole("Admin") ?? false;
    }
}
