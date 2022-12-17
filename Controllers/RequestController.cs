using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.MailService;
using System.Net;
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Models.DTOs;
using StudentsForStudentsAPI.Models.Mails;
using StudentsForStudentsAPI.Services.UserService;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet d'effectuer les opérations CRUD sur les demandes de tutorat
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService;
        private readonly IHubContext<SignalRHub> _hubContext;

        /// <summary>
        /// Constructeur du controller qui permet d'effectuer les opérations CRUD sur les demandes de tutorat
        /// </summary>
        /// <param name="context">Un objet permettant d'intéragir avec la base de données</param>
        /// <param name="userService">Un service permettant d'intéragir avec l'utilisateur connecté</param>
        /// <param name="userManager">Un service permettant de gérer l'utilisateur connecté</param>
        /// <param name="mailService">Un service qui permet d'envoyer des mails</param>
        /// <param name="hubContext">Un objet permettant d'envoyer des notifications aux différents clients connectés</param>
        public RequestController(DatabaseContext context, IUserService userService, UserManager<User> userManager,
            IMailService mailService, IHubContext<SignalRHub> hubContext)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
            _mailService = mailService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Route (DELETE) qui permet de supprimer une demande de tutorat
        /// </summary>
        /// <param name="requestId">L'identifiant de la demande de tutorat à supprimer</param>
        /// <returns>Un ViewModel spécifiant que la suppression s'est bien effectuée</returns>
        [HttpDelete("{requestId:int}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Supprime une demande de tutorat")]
        [SwaggerResponse(StatusCodes.Status200OK, "La demande de tutorat a bien été supprimée", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "L'identifiant de la demande de tutorat n'est pas valide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "La demande de tutorat n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> DeleteRequest(int requestId)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));
            var request = _context.Requests.Include(r => r.Sender).FirstOrDefault(r => r.Id == requestId);
            if (request == null) return NotFound(new ErrorViewModel("La demande n'existe pas"));

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;

            if (user == null) return NotFound(new ErrorViewModel("L'utilisateur n'existe pas"));
            if (!request.Sender.Id.Equals(user.Id))
                return BadRequest(new ErrorViewModel(
                    "Vous n'avez pas le droit de supprimer une demande qui vous n'appartient pas"));
            
            if (request.Status)
                return BadRequest(new ErrorViewModel("Vous ne pouvez pas supprimer une demande acceptée"));

            _mailService.SendMail(new DeleteRequestMail($"Suppression de la demande \"{request.Name}\"", user.Email,
                null, new[] { user.UserName, request.Name }));
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("updateRequests");

            return Ok(new SuccessViewModel("Demande supprimée avec succès"));
        }

        /// <summary>
        /// Route (PUT) qui permet de modifier le statut et le tuteur d'une demande de tutorat
        /// </summary>
        /// <param name="requestId">L'identifiant de le demande</param>
        /// <returns>Un ViewModel spécifiant que la demande a bien été acceptée</returns>
        [HttpPut("{requestId:int}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Accepte une demande de tutorat")]
        [SwaggerResponse(StatusCodes.Status200OK, "La demande de tutorat a bien été acceptée", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "L'identifiant de la demande de tutorat n'est pas valide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "La demande de tutorat n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> UpdateRequest(int requestId)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));
            var request = _context.Requests.Include(r => r.Sender).FirstOrDefault(r => r.Id == requestId);
            if (request == null) return NotFound(new ErrorViewModel("La demande n'existe pas"));

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;

            if (user == null) return NotFound(new ErrorViewModel("L'utilisateur n'existe pas"));
            if (request.Sender.Id.Equals(user.Id))
                return BadRequest(new ErrorViewModel("Vous ne pouvez pas accepter une de vos propres demandes"));
            if (request.Status) return BadRequest(new ErrorViewModel("Cette demande a déjà été acceptée"));

            request.Status = !request.Status;
            request.Handler = user;

            _mailService.SendMail(new UpdateSenderRequestMail($"Demande \"{request.Name}\" acceptée",
                request.Sender.Email, null, new[] { request.Sender.UserName, request.Name, request.Handler.UserName }));
            
            _mailService.SendMail(new UpdateHandlerRequestMail($"Demande \"{request.Name}\" acceptée",
                request.Handler.Email, null, new[] { request.Handler.UserName, request.Name, request.Sender.UserName }));
            
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("updateRequestStatus", request.Name, request.Sender.UserName,
                request.Handler.UserName);

            return Ok(new SuccessViewModel("Demande acceptée avec succès"));
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer :
        /// Si own = false : les demandes de tutorat publiques (sans les demandes de l'utilisateur connecté)
        /// Si owner = true : les demandes de tutorat personnelles (celles de l'utilisateur connecté)
        /// </summary>
        /// <param name="own">Un booléen qui permet de savoir si l'on veut les demandes publiques ou personnelles</param>
        /// <returns>Une liste de requêtes transformées en ViewModel</returns>
        [HttpGet("{own:bool}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère les demandes de tutorat (publiques ou personnelles)")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les demandes de tutorat ont bien été récupérées", typeof(List<RequestViewModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        public ActionResult<List<RequestViewModel>> GetRequests(bool own)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));

            if (!own)
            {
                var requests = GetPublicRequests();
                var finalRequests = GetFinalRequests(requests);
                return Ok(finalRequests);
            }
            else
            {
                var requests = GetOwnRequests();
                var finalRequests = GetFinalRequests(requests);
                return Ok(finalRequests);
            }
        }

        /// <summary>
        /// Route (POST) qui permet de créer une demande de tutorat
        /// </summary>
        /// <param name="request">La potentielle nouvelle demande à ajouter</param>
        /// <returns>Un ViewModel spécifiant que la demande a bien été créée</returns>
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Crée une demande de tutorat")]
        [SwaggerResponse(StatusCodes.Status200OK, "La demande de tutorat a bien été créée", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Les informations de la demande de tutorat sont invalides", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        public async Task<ActionResult<SuccessViewModel>> CreateRequest(RequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            if (user == null) return NotFound(new ErrorViewModel("Aucun utilisateur associé à ce token"));

            try
            {
                var dbRequest = new Request(request.Name, request.Description, user,
                    _context.Places.Find(request.PlaceId), _context.Courses.Find(request.CourseId));

                _mailService.SendMail(new AddRequestMail($"Création de la demande \"{dbRequest.Name}\"", user.Email,
                    null, new[] { user.UserName, dbRequest.Name }));
                
                _context.Requests.Add(dbRequest);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("updateRequests");

                return Ok(new SuccessViewModel("Demande créée avec succès"));
            }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel("Informations invalides"));
            }
        }

        private static IEnumerable<RequestViewModel> GetFinalRequests(IEnumerable<Request> requests)
            => requests.Select(r => new RequestViewModel(r));

        private IEnumerable<Request> GetOwnRequests()
            => _context.Requests
                .Include(r => r.Place)
                .Include(r => r.Course)
                .ThenInclude(c => c.Cursus)
                .ThenInclude(c => c.Section)
                .Include(r => r.Sender)
                .Include(r => r.Handler)
                .Where(r => r.Sender.Id.Equals(_userService.GetUserIdFromToken()) ||
                                                  r.Handler.Id.Equals(_userService.GetUserIdFromToken()))
                .OrderBy(r => r.Date)
                .ToList();

        private IEnumerable<Request> GetPublicRequests()
            => _context.Requests
                .Include(r => r.Place)
                .Include(r => r.Course)
                .ThenInclude(c => c.Cursus)
                .ThenInclude(c => c.Section)
                .Include(r => r.Sender)
                .Where(r => !r.Sender.Id.Equals(_userService.GetUserIdFromToken()) && !r.Status)
                .OrderBy(r => r.Date)
                .ToList();
    }
}