using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Services;
using System.Linq;
using System.Web;
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services.UserService;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet de gérer les événements du calendrier de l'utilisateur connecté
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructeur du controller CalendarController qui permet de gérer les évènements du calendrier de l'utilisateur connecté
        /// </summary>
        /// <param name="userService">Un service permettant d'intéragir avec l'utilisateur connecté</param>
        /// <param name="userManager">Un service permettant de gérer l'utilisateur connecté</param>
        public CalendarController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        /// <summary>
        /// Route (GET) permettant de récupérer les évènements du calendrier de l'utilisateur connecté
        /// </summary>
        /// <returns>Les évènements du calendrier de l'utilisateur connecté sous forme de chaine de caractères</returns>
        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère les évènements du calendrier de l'utilisateur connecté")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les évènements du calendrier de l'utilisateur connecté sous forme d'une chaine de caractères", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'a pas de calendrier lié au système", typeof(ErrorViewModel))]
        public ActionResult<string> GetCalendar()
        {
            var calendarUrl = HttpUtility.UrlDecode(_userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result.CalendarLink);
            var calendar = Calendar.GetCalendar(calendarUrl).Result;

            if (calendar == null) return NotFound(new ErrorViewModel("Aucun calendrier trouvé"));
            
            return Ok(calendar);
        }
    }
}
