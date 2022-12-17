using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Models.DTOs;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services.MailService;
using StudentsForStudentsAPI.Models.Mails;
using StudentsForStudentsAPI.Services.UserService;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet de gérer les utilisateurs
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IMailService _mailService;
        private readonly IHubContext<SignalRHub> _hubContext;

        /// <summary>
        /// Constructeur du controller qui permet de gérer les utilisateurs
        /// </summary>
        /// <param name="context">Un objet permettant d'intéragir avec la base de données</param>
        /// <param name="userManager">Un service permettant de gérer l'utilisateur connecté</param>
        /// <param name="config">Un objet contenant la configuration du système</param>
        /// <param name="userService">Un service permettant d'intéragir avec l'utilisateur connecté</param>
        /// <param name="mailService">Un service qui permet d'envoyer des mails</param>
        /// <param name="hubContext">Un objet permettant d'envoyer des notifications aux différents clients connectés</param>
        public UserController(DatabaseContext context, UserManager<User> userManager, IConfiguration config,
            IUserService userService, IMailService mailService, IHubContext<SignalRHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
            _userService = userService;
            _mailService = mailService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Route (DELETE) qui permet de supprimer un utilisateur
        /// </summary>
        /// <param name="emailAddress">L'adresse mail de l'utilisateur à supprimer</param>
        /// <returns>Un ViewModel spécifiant que l'utilisateur a bien été supprimée</returns>
        [HttpDelete("{emailAddress}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Supprime un utilisateur")]
        [SwaggerResponse(StatusCodes.Status200OK, "L'utilisateur a bien été supprimé", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "L'adresse mail est invalide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> DeleteUser(string emailAddress)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Adresse email invalide"));

            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
                return NotFound(new ErrorViewModel("Aucun utilisateur associé à cette adresse email"));
            
            if (_userManager.IsInRoleAsync(user, "Admin").Result)
                return BadRequest(new ErrorViewModel("Impossible de supprimer un administrateur"));

            var requests = _context.Requests
                .Include(r => r.Sender)
                .Include(r => r.Handler)
                .Where(r => r.Handler != null &&
                            (r.Sender.Id.Equals(user.Id) || (r.Status && r.Handler.Id.Equals(user.Id))))
                .ToList();

            var files = _context.Files
                .Include(f => f.Owner)
                .Where(f => f.Owner != null && f.Owner.Id.Equals(user.Id))
                .ToList();

            var forms = _context.Forms
                .Include(f => f.Sender)
                .Where(f =>
                    f.Sender != null && f.SenderEmail != null &&
                    (f.SenderEmail.Equals(user.Email) || f.Sender.Id.Equals(user.Id)))
                .ToList();

            foreach (var request in requests) _context.Requests.Remove(request);
            foreach (var file in files) _context.Files.Remove(file);
            foreach (var form in forms) _context.Forms.Remove(form);

            await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            await _hubContext.Clients.All.SendAsync("updateUsersCount");
            await _hubContext.Clients.All.SendAsync("updateUsers", user.Email);
            await _hubContext.Clients.All.SendAsync("updateRequests");
            _mailService.SendMail(new DeleteAccountMail("Suppression de votre compte", user.Email, null,
                new[] { user.UserName }));

            return Ok(new SuccessViewModel("Utilisateur supprimé avec succès"));
        }

        /// <summary>
        /// Route (PUT) qui permet de modifier le nom d'utilisateur d'un utilisateur
        /// </summary>
        /// <param name="emailAddress">L'adresse mail de l'utilisateur à modifier</param>
        /// <param name="request">Le potentiel nouveau nom d'utilisateur de l'utilisateur ciblé</param>
        /// <returns>Un ViewModel spécifiant que la modification s'est bien effectuée</returns>
        [HttpPut("{emailAddress}/Username")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Modifie le nom d'utilisateur d'un utilisateur")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le nom d'utilisateur a bien été modifié", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "L'adresse mail ou le nom d'utilisateur est invalide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> EditUser(string emailAddress, [FromBody] UsernameDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorViewModel("Adresse email ou nom d'utilisateur invalide"));

            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
                return NotFound(new ErrorViewModel("Aucun utilisateur associé à cette adresse email"));

            user.UserName = request.Username;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ErrorViewModel(string.Join(" | ", result.Errors.Select(e => e.Code))));

            await _hubContext.Clients.All.SendAsync("updateUsers", user.Email);
            _mailService.SendMail(new EditAccountMail("Compte modifié", user.Email, null, new[] { user.UserName }));

            return Ok(new SuccessViewModel("Utilisateur modifié avec succès"));
        }

        /// <summary>
        /// Route (PUT) qui permet de bloquer/débloquer un utilisateur
        /// </summary>
        /// <param name="emailAddress">L'adresse mail de l'utilisateur à bloquer/débloquer</param>
        /// <returns>Un ViewModel spécifiant que l'utilisateur a été bloqué/débloqué</returns>
        [HttpPut("{emailAddress}/Status")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Bloque ou débloque un utilisateur")]
        [SwaggerResponse(StatusCodes.Status200OK, "L'utilisateur a bien été bloqué/débloqué", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "L'adresse mail est invalide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> UpdateBannedStatus(string emailAddress)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Adresse email invalide"));

            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
                return NotFound(new ErrorViewModel("Aucun utilisateur associé à cette adresse email"));
            
            if (_userManager.IsInRoleAsync(user, "Admin").Result)
                return BadRequest(new ErrorViewModel("Impossible de bloquer un administrateur"));

            user.IsBanned = !user.IsBanned;
            await _userManager.UpdateAsync(user);
            await _hubContext.Clients.All.SendAsync("updateUsers", user.Email);
            _mailService.SendMail(new UpdateAccountMail("Statut de votre compte", user.Email, null,
                new[] { user.UserName, user.IsBanned ? "true" : "false" }));

            return Ok(new SuccessViewModel(
                $"Utilisateur {(user.IsBanned ? "bloqué" : "débloqué")} avec succès"));
        }

        /// <summary>
        /// Route (PUT) qui permet de modifier le lien du calendrier d'un utilisateur
        /// </summary>
        /// <param name="calendarLink">Le lien (horairix) du calendrier</param>
        /// <returns>Un ViewModel spécifiant que le lien du calendrier a bien été mis à jour</returns>
        [HttpPut("{calendarLink}")]
        [Authorize(Roles = "Member, Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Modifie le lien du calendrier d'un utilisateur")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le lien du calendrier a bien été modifié", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Le lien du calendrier est invalide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'existe pas", typeof(ErrorViewModel))]
        public ActionResult<SuccessViewModel> UpdateCalendarLink(string calendarLink)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Le lien du calendrier est invalide"));

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            user.CalendarLink = calendarLink;
            _context.SaveChanges();
            
            return Ok(new SuccessViewModel("Lien du calendrier mis à jour avec succès"));
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer les utilisateurs de l'application
        /// </summary>
        /// <returns>Une liste d'utilisateurs transformées en ViewModel</returns>
        [HttpGet("List")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère la liste des utilisateurs de l'application")]
        [SwaggerResponse(StatusCodes.Status200OK, "La liste des utilisateurs a bien été récupérée", typeof(List<UserViewModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        public async Task<ActionResult<List<UserViewModel>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Cursus)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                userViewModels.Add(new UserViewModel(user, isAdmin, "nothing to see here"));
            }

            return Ok(userViewModels);
        }
        
        /// <summary>
        /// Route (GET) qui permet de récupérer les informations de l'utilisateur connecté
        /// </summary>
        /// <returns>L'utilisateur (sous forme de ViewModel) contenant les informations de l'utilisateur connecté</returns>
        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère les informations de l'utilisateur connecté")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les informations de l'utilisateur ont bien été récupérées", typeof(UserViewModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Le compte associé au token n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<UserViewModel>> WhoAmI()
        {
            var id = _userService.GetUserIdFromToken();
            if (id == null) return NotFound(new ErrorViewModel("Aucun utilisateur associé à ce token"));

            var user = await _userManager.FindByIdAsync(id);
            if (user.IsBanned)
                return BadRequest(new ErrorViewModel("Votre compte a été bloqué par un administrateur"));

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            user.Cursus = _context.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.Cursus)
                .ThenInclude(c => c.Section)
                .FirstOrDefault()!.Cursus;

            return Ok(new UserViewModel(user, isAdmin, Token.CreateToken(user, _userManager, _config)));
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer le nombre d'utilisateurs de l'application
        /// </summary>
        /// <returns>Le nombre d'utilisateurs de l'application</returns>
        [AllowAnonymous]
        [HttpGet("Count")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère le nombre d'utilisateurs de l'application")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le nombre d'utilisateurs a bien été récupéré", typeof(int))]
        public ActionResult<int> GetUsersCount() => Ok(_context.Users.Count());

        /// <summary>
        /// Route (POST) qui permet de connecter un utilisateur via son token Google
        /// </summary>
        /// <param name="request">Le token fourni par Google</param>
        /// <returns>Les informations de l'utilisateur (sous forme de ViewModel) qui souhaite se connecter via Google</returns>
        [AllowAnonymous]
        [HttpPost("Google")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Connecte un utilisateur via son token Google")]
        [SwaggerResponse(StatusCodes.Status200OK, "L'utilisateur a bien été connecté", typeof(UserViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Le token Google est invalide", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<UserViewModel>> Google(OAuthDto request)
        {
            if(!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));
            try
            {
                var payload = GoogleJsonWebSignature
                    .ValidateAsync(request.Credentials, new GoogleJsonWebSignature.ValidationSettings()).Result;
                
                var user = _userManager.FindByEmailAsync(payload.Email).Result;

                if (user == null) return NotFound(new ErrorViewModel(payload.Email));
                if (user.IsBanned)
                    return BadRequest(new ErrorViewModel("Votre compte a été bloqué par un administrateur"));

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                user.Cursus = _context.Users
                    .Where(u => u.Id == user.Id)
                    .Include(u => u.Cursus)
                    .ThenInclude(c => c.Section)
                    .FirstOrDefault()!.Cursus;

                return Ok(new UserViewModel(user, isAdmin, Token.CreateToken(user, _userManager, _config)));
            }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel("Mauvaise requête"));
            }
        }

        /// <summary>
        /// Route (POST) qui permet de connecter un utilisateur
        /// </summary>
        /// <param name="request">Les informations nécessaires pour connecter un utilisateur</param>
        /// <returns>Les informations de l'utilisateur (sous forme de ViewModel) qui souhaite se connecter</returns>
        [AllowAnonymous]
        [HttpPost("SignIn")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Connecte un utilisateur")]
        [SwaggerResponse(StatusCodes.Status200OK, "L'utilisateur a bien été connecté", typeof(UserViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Les informations de connexion sont invalides", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "L'utilisateur n'existe pas", typeof(ErrorViewModel))]
        public async Task<ActionResult<UserViewModel>> SignIn(UserSignInDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return BadRequest(new ErrorViewModel("Email invalide"));

            if (request.Password != string.Empty)
            {
                var result = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!result) return BadRequest(new ErrorViewModel("Mot de passe invalide"));
            }

            if (user.IsBanned)
                return BadRequest(new ErrorViewModel("Votre compte a été bloqué par un administrateur"));

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            user.Cursus = _context.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.Cursus)
                .ThenInclude(c => c.Section)
                .FirstOrDefault()!.Cursus;

            return Ok(new UserViewModel(user, isAdmin, Token.CreateToken(user, _userManager, _config)));
        }

        /// <summary>
        /// Route (POST) qui permet de créer un utilisateur
        /// </summary>
        /// <param name="request">Les informations nécessaires pour créer un nouvel utilisateur</param>
        /// <returns>Un ViewModel spécifiant que l'utilisateur a bien été créé</returns>
        [AllowAnonymous]
        [HttpPost("SignUp")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Crée un nouvel utilisateur")]
        [SwaggerResponse(StatusCodes.Status200OK, "L'utilisateur a bien été créé", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Les informations de l'utilisateur sont invalides ou l'utilisateur existe déjà", typeof(ErrorViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Informations transmises invalides", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> SignUp(UserSignUpDto request)
        {
            if (!ModelState.IsValid) return BadRequest("Informations invalides");

            try
            {
                var user = new User
                {
                    UserName = $"{request.LastName} {request.FirstName}",
                    Email = request.Email,
                    Cursus = _context.Cursus.First(c => c.Id == request.CursusId)
                };

                var result = request.Password != string.Empty
                    ? await _userManager.CreateAsync(user, request.Password)
                    : await _userManager.CreateAsync(user);
                
                if (!result.Succeeded)
                    return BadRequest(new ErrorViewModel(string.Join(" | ", result.Errors.Select(e => e.Code))));

                await _userManager.AddToRoleAsync(user, "Member");
                user.Cursus = _context.Users
                    .Where(u => u.Id == user.Id)
                    .Include(u => u.Cursus)
                    .ThenInclude(c => c.Section)
                    .FirstOrDefault()!.Cursus;

                _mailService.SendMail(new AddAccountMail("Bienvenue sur Students for Students !", user.Email, null,
                    new[] { user.UserName }));
                
                await _hubContext.Clients.All.SendAsync("updateUsersCount");
                await _hubContext.Clients.All.SendAsync("updateUsers");

                return Ok(new SuccessViewModel("Compte créé avec succès"));
            }
            catch (Exception)
            {
                return NotFound(new ErrorViewModel("Cursus invalide"));
            }
        }
    }
}