using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Services;
using System.Linq;
using System.Web;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public CalendarController(DatabaseContext context, IUserService userService, UserManager<User> userManager)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public async Task<ActionResult> GetCalendar()
        {
            if (!_userService.IsTokenValid()) return Unauthorized();
            
            string? calendarUrl = HttpUtility.UrlDecode(_userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result.CalendarLink);
            string? calendar = Calendar.GetCalendar(calendarUrl).Result;

            if (calendar == null) return NotFound();
            return Ok(calendar);
        }
    }
}
