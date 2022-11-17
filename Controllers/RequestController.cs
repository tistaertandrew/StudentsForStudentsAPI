using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;

namespace StudentsForStudentsAPI.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public RequestController(DatabaseContext context, IUserService userService, UserManager<User> userManager)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public IActionResult GetRequests()
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            if (!_userService.IsTokenValid()) return Unauthorized();

            var requests = _context.Requests.Include(r => r.Sender).Where(r => r.Sender.Id != _userService.GetUserIdFromToken()).ToList();

            return Ok(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [Produces("application/json")]
        public ActionResult CreateRequest(RequestViewModel request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            if (!_userService.IsTokenValid()) return Unauthorized();

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            if (user == null) return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à ce token"));

            try
            {
                var newRequest = new Request
                {
                    Name = request.Name,
                    Description = request.Description,
                    Status = false,
                    Date = DateTime.Now,
                    Sender = user,
                    Handler = null,
                    Place = _context.Places.Find(request.PlaceId),
                    Course = _context.Courses.Find(request.CourseId)
                };

                _context.Requests.Add(newRequest);
                _context.SaveChanges();
                
                return Ok(new SuccessViewModel(false, "Requête créée avec succès"));
            } catch(Exception)
            {
                return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            }
        }
    }
}
