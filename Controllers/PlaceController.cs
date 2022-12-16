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
        private readonly IConfiguration _config;
        private readonly DatabaseContext _context;

        public PlaceController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public ActionResult<List<Place>> GetPlaces() => Ok(_context.Places.ToList());

        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public ActionResult<SuccessViewModel> AddPlace(PlaceViewModel request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));

            var fullAddress = $"{request.Street} {request.Number}, {request.PostalCode} {request.Locality}";
            var correctedPlace = Place.CheckAddress(fullAddress, _config);

            if (correctedPlace == null) return BadRequest(new ErrorViewModel(true, "Adresse invalide"));

            var place = new Place(correctedPlace.Street, 
                correctedPlace.PostalCode, 
                correctedPlace.Number,
                correctedPlace.Locality);

            if (_context.Places.Any(p =>
                    p.Street == place.Street 
                    && p.PostalCode == place.PostalCode 
                    && p.Number == place.Number 
                    && p.Locality == place.Locality))
                return BadRequest(new ErrorViewModel(true, "Ce lieu existe déjà"));

            _context.Places.Add(place);
            _context.SaveChanges();
            
            return Ok(new SuccessViewModel(false, "Lieu ajouté avec succès"));
        }
    }
}