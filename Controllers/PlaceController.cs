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
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Models.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet de gérer les lieux pour les demandes de tutorat
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _context;

        /// <summary>
        /// Constructeur du controller qui permet de gérer les lieux pour les demandes de tutorat
        /// </summary>
        /// <param name="config">Un objet contenant la configuration du système</param>
        /// <param name="context">Un objet permettant d'intéragir avec la base de données</param>
        public PlaceController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer la liste des lieux pour les demandes de tutorat
        /// </summary>
        /// <returns>Une liste contenant les lieux pour les demandes de tutorat</returns>
        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère la liste des lieux pour les demandes de tutorat")]
        [SwaggerResponse(StatusCodes.Status200OK, "Liste des lieux pour les demandes de tutorat", typeof(List<Place>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas authentifié ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        public ActionResult<List<Place>> GetPlaces() => Ok(_context.Places.ToList());

        /// <summary>
        /// Route (POST) qui permet d'ajouter un lieu pour les demandes de tutorat
        /// </summary>
        /// <param name="request">Un objet de transfert de données contenant le potentiel nouveau lieu à ajouter</param>
        /// <returns>Un ViewModel spécifiant que l'ajout s'est correctement effectué</returns>
        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Ajoute un lieu pour les demandes de tutorat")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le lieu pour les demandes de tutorat ajouté", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Le lieu pour les demandes de tutorat est invalide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas authentifié ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        public ActionResult<SuccessViewModel> AddPlace(PlaceDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));

            var correctedPlace = Place.CheckAddress(request.ToString(), _config);

            if (correctedPlace == null) return BadRequest(new ErrorViewModel("Adresse invalide"));

            var place = new Place(correctedPlace.Street, 
                correctedPlace.PostalCode, 
                correctedPlace.Number,
                correctedPlace.Locality);

            if (_context.Places.Any(p =>
                    p.Street == place.Street 
                    && p.PostalCode == place.PostalCode 
                    && p.Number == place.Number 
                    && p.Locality == place.Locality))
                return BadRequest(new ErrorViewModel("Ce lieu existe déjà"));

            _context.Places.Add(place);
            _context.SaveChanges();
            
            return Ok(new SuccessViewModel("Lieu ajouté avec succès"));
        }
    }
}