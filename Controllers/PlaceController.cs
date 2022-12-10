using Geocoding;
using Geocoding.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using System.Xml.Linq;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly DatabaseContext _context;

        public PlaceController(IUserService userService, IConfiguration config, UserManager<User> userManager, DatabaseContext context)
        {
            _userService = userService;
            _config = config;
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

            string fullAddress = $"{request.Street} {request.Number}, {request.PostalCode} {request.Locality}";
            var correctedPlace = Place.CheckAddress(fullAddress, _config);

            if (correctedPlace == null) return BadRequest(new ErrorViewModel(true, "Adresse invalide"));

            var place = new Place
            {
                Street = correctedPlace.Street,
                PostalCode = correctedPlace.PostalCode,
                Number = correctedPlace.Number,
                Locality = correctedPlace.Locality
            };

            if (_context.Places.Any(p => p.Street == place.Street && p.PostalCode == place.PostalCode && p.Number == place.Number && p.Locality == place.Locality))
            {
                return BadRequest(new ErrorViewModel(true, "Ce lieu existe déjà"));
            }
            _context.Places.Add(place);
            _context.SaveChanges();
            return Ok(new SuccessViewModel(false, "Lieu ajouté avec succès"));
        }
    }
}
