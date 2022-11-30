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
            string? street = null;
            string? number = null;
            int postalCode = -1;
            string? locality = null;
            
            if (!_userService.IsTokenValid()) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));

            string fullAddress = $"{request.Street} {request.Number}, {request.PostalCode} {request.Locality}";
            string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(fullAddress), _config["AppSettings:GoogleApiKey"]);

            using (var client = new HttpClient())
            {
                var resp = client.GetAsync(requestUri).Result;
                var content = resp.Content.ReadAsStringAsync().Result;
                var xml = XDocument.Parse(content);
                var elements = xml.Element("GeocodeResponse")?.Element("result")?.Elements("address_component");

                if (elements == null || !elements.Any()) return BadRequest(new ErrorViewModel(true, "Informations invalides"));

                foreach (var element in elements)
                {
                    switch(element.Element("type")!.Value)
                    {
                        case "route":
                            street = element.Element("long_name")!.Value;
                            break;
                        case "street_number":
                            number = element.Element("long_name")!.Value;
                            break;
                        case "locality":
                            locality = element.Element("long_name")!.Value;
                            break;
                        case "postal_code":
                            postalCode = int.Parse(element.Element("long_name")!.Value);
                            break;
                    }
                }

                if(street == null || number == null || postalCode == -1 || locality == null)
                {
                    return BadRequest(new ErrorViewModel(true, "Informations invalides"));
                }
            }

            var place = new Place
            {
                Street = street,
                PostalCode = postalCode,
                Number = number,
                Locality = locality
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
