using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly DatabaseContext _context;

        public PlaceController(IUserService userService, UserManager<User> userManager, DatabaseContext context)
        {
            _userService = userService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public ActionResult<List<Place>> GetPlaces()
        {
            if (!_userService.IsTokenValid()) return Unauthorized();

            return Ok(_context.Places.ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public ActionResult AddPlace(PlaceViewModel request)
        {
            if (!_userService.IsTokenValid()) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));

            var place = new Place
            {
                Street = request.Street,
                PostalCode = request.PostalCode,
                Number = request.Number,
                Locality = request.Locality
            };
            _context.Places.Add(place);
            _context.SaveChanges();
            return Ok(new SuccessViewModel(false, "Lieu ajouté avec succès"));
        }
    }
}
