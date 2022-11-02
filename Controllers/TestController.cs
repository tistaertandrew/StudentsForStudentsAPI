using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using System.Security.Claims;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public TestController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet, Authorize]
        public ActionResult<object> GetMe()
        {
            var userName = User?.Identity?.Name;
            var email = User?.FindFirstValue(ClaimTypes.Email);
            return Ok(new { userName, email });
        }

        [HttpGet("Users")]
        [Produces("application/json")]
        [Authorize(Roles = "Member")]
        public async Task<string> GetNbrUsers()
        {
            return _databaseContext.Users.Count().ToString();
        }
    }
}
