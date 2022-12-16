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
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public CalendarController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public ActionResult<string> GetCalendar()
        {
            var calendarUrl = HttpUtility.UrlDecode(_userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result.CalendarLink);
            var calendar = Calendar.GetCalendar(calendarUrl).Result;

            if (calendar == null) return NotFound(new ErrorViewModel(true, "Aucun calendrier trouvé"));
            
            return Ok(calendar);
        }
    }
}
