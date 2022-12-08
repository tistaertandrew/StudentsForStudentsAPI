using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.MailService;
using System.Security.Claims;

namespace StudentsForStudentsAPI.Controllers
{
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

        public UserController(DatabaseContext context, UserManager<User> userManager, IConfiguration config, IUserService userService, IMailService mailService, IHubContext<SignalRHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
            _userService = userService;
            _mailService = mailService;
            _hubContext = hubContext;
        }

        [HttpDelete("{emailAddress}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        public async Task<ActionResult<SuccessViewModel>> DeleteUser(string emailAddress)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Adresse email invalide"));
            if (!_userService.IsTokenValid()) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null) return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à cette adresse email"));
            if (_userManager.IsInRoleAsync(user, "Admin").Result) return BadRequest(new ErrorViewModel(true, "Impossible de supprimer un administrateur"));

            var requests = _context.Requests.Include(r => r.Sender).Include(r => r.Handler).Where(r => r.Sender.Id.Equals(user.Id) || (r.Status && r.Handler.Id.Equals(user.Id))).ToList();
            var files = _context.Files.Include(f => f.Owner).Where(f => f.Owner.Id.Equals(user.Id)).ToList();
            var forms = _context.Forms.Include(f => f.Sender).Where(f => f.SenderEmail.Equals(user.Email) || f.Sender.Id.Equals(user.Id)).ToList();

            foreach (var request in requests) _context.Requests.Remove(request);
            foreach (var file in files) _context.Files.Remove(file);
            foreach (var form in forms) _context.Forms.Remove(form);

            _context.SaveChanges();
            await _userManager.DeleteAsync(user);
            await _hubContext.Clients.All.SendAsync("updateUsersCount");
            await _hubContext.Clients.All.SendAsync("updateUsers", user.Email);
            await _hubContext.Clients.All.SendAsync("updateRequests");
            _mailService.SendMail("Suppression de votre compte", new string[] { user.UserName }, "DeleteAccount", user.Email);
            //_mailService.SendMail("Suppression de votre compte", $"Bonjour {user.UserName}, \n\nVotre compte a été supprimé par un administrateur. \n\nCordialement, \nL'équipe de Students for Students.", user.Email);

            return Ok(new SuccessViewModel(false, "Utilisateur supprimé avec succès"));
        }

        [HttpPut("{emailAddress}/Username/{username}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        public async Task<ActionResult<SuccessViewModel>> EditUser(string emailAddress, string username)
        {
            {
                if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Adresse email ou nom d'utilisateur invalide"));
                if (!_userService.IsTokenValid()) return Unauthorized();

                var user = await _userManager.FindByEmailAsync(emailAddress);
                if (user == null) return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à cette adresse email"));

                user.UserName = username;
                var result = await _userManager.UpdateAsync(user);
                if(!result.Succeeded) return BadRequest(new ErrorViewModel(true, string.Join(" | ", result.Errors.Select(e => e.Code))));

                await _hubContext.Clients.All.SendAsync("updateUsers", user.Email);

                _mailService.SendMail("Statut de votre compte", new string[] { user.UserName }, "EditAccount", user.Email);
                //_mailService.SendMail("Statut de votre compte", $"Bonjour {user.UserName}, \n\nVotre compte a été modifié par un administrateur. \n\nCordialement, \nL'équipe de Students for Students.", user.Email);

                return Ok(new SuccessViewModel(false, "Utilisateur modifié avec succès"));
            }
        }

        [HttpPut("Status/{emailAddress}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        public async Task<ActionResult<SuccessViewModel>> UpdateBannedStatus(string emailAddress)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Adresse email invalide"));
            if (!_userService.IsTokenValid()) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null) return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à cette adresse email"));
            if (_userManager.IsInRoleAsync(user, "Admin").Result) return BadRequest(new ErrorViewModel(true, "Impossible de bloquer un administrateur"));

            user.IsBanned = !user.IsBanned;
            await _userManager.UpdateAsync(user);
            await _hubContext.Clients.All.SendAsync("updateUsers", user.Email);
            _mailService.SendMail("Statut de votre compte", new string[] { user.UserName, user.IsBanned ? "true" : "false" }, "UpdateAccount", user.Email);
            //_mailService.SendMail("Statut de votre compte", $"Bonjour {user.UserName}, \n\nVotre compte a été {(user.IsBanned ? "bloqué" : "débloqué")} par un administrateur. \n\nCordialement, \nL'équipe de Students for Students.", user.Email);

            return Ok(new SuccessViewModel(false, $"Utilisateur {(user.IsBanned ? "bloqué" : "débloqué")} avec succès"));
        }

        [HttpPut("{calendarLink}")]
        [Authorize(Roles = "Member, Admin")]
        [Produces("application/json")]
        public ActionResult<SuccessViewModel> UpdateCalendarLink(string calendarLink)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Le lien du calendrier est invalide"));
            
            if (!_userService.IsTokenValid()) return Unauthorized();
            
            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            user.CalendarLink = calendarLink;
            _context.SaveChanges();
            return Ok(new SuccessViewModel(false, "Lien du calendrier mis à jour avec succès"));
        }

        [HttpGet("List")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        public async Task<ActionResult<List<UserViewModel>>> GetUsers()
        {
            if (!_userService.IsTokenValid()) return Unauthorized();

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


        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public async Task<ActionResult<UserViewModel>> WhoAmI()
        {
            if (!_userService.IsTokenValid()) return Unauthorized();

            var id = _userService.GetUserIdFromToken();
            if (id == null)
            {
                return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à ce token"));
            }
            
            var user = await _userManager.FindByIdAsync(id);
            if (user.IsBanned) return BadRequest(new ErrorViewModel(true, "Votre compte a été bloqué par un administrateur"));

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            user.Cursus = _context.Users.Where(u => u.Id == user.Id).Include(u => u.Cursus).ThenInclude(c => c.Section).FirstOrDefault().Cursus;
            
            return Ok(new UserViewModel(user, isAdmin, Token.CreateToken(user, _userManager, _config)));
        }

        [AllowAnonymous]
        [HttpGet("Count")]
        [Produces("application/json")]
        public ActionResult GetUsersCount()
        {
            return Ok(_context.Users.Count());
        }

        [AllowAnonymous]
        [HttpPost("Google")]
        [Produces("application/json")]
        public async Task<ActionResult<UserViewModel>> Google(OAuthViewModel request)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(request.Credentials, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = _userManager.FindByEmailAsync(payload.Email).Result;
                
                if (user == null) return NotFound(new ErrorViewModel(true, payload.Email));
                if (user.IsBanned) return BadRequest(new ErrorViewModel(true, "Votre compte a été bloqué par un administrateur"));

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                user.Cursus = _context.Users.Where(u => u.Id == user.Id).Include(u => u.Cursus).ThenInclude(c => c.Section).FirstOrDefault().Cursus;

                return Ok(new UserViewModel(user, isAdmin, Token.CreateToken(user, _userManager, _config)));
            }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel(true, "Mauvaise requête"));
            }
        }

        [AllowAnonymous]
        [HttpPost("SignIn")]
        [Produces("application/json")]
        public async Task<ActionResult<UserViewModel>> SignIn(UserSignIn request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new ErrorViewModel(true, "Email invalide"));
            }

            if (request.Password != string.Empty)
            {
                var result = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!result)
                {
                    return BadRequest(new ErrorViewModel(true, "Mot de passe invalide"));
                }
            }

            if (user.IsBanned) return BadRequest(new ErrorViewModel(true, "Votre compte a été bloqué par un administrateur"));

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            user.Cursus = _context.Users.Where(u => u.Id == user.Id).Include(u => u.Cursus).ThenInclude(c => c.Section).FirstOrDefault().Cursus;

            return Ok(new UserViewModel(user, isAdmin, Token.CreateToken(user, _userManager, _config)));
        }

        [AllowAnonymous]
        [HttpPost("SignUp")]
        [Produces("application/json")]
        public async Task<ActionResult> SignUp(UserSignUp request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Informations invalides");
            }

            try
            {
                var user = new User();
                user.UserName = string.Format("{0} {1}", request.LastName, request.FirstName);
                user.Email = request.Email;
                user.Cursus = _context.Cursus.Where(c => c.Id == request.CursusId).First();

                var result = request.Password != string.Empty ? await _userManager.CreateAsync(user, request.Password) : await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new ErrorViewModel(true, string.Join(" | ", result.Errors.Select(e => e.Code))));
                }

                await _userManager.AddToRoleAsync(user, "Member");
                user.Cursus = _context.Users.Where(u => u.Id == user.Id).Include(u => u.Cursus).ThenInclude(c => c.Section).FirstOrDefault().Cursus;


                _mailService.SendMail("Bienvue sur Students for Students !", new string[] { user.UserName }, "DeleteAccount", user.Email);
                //_mailService.SendMail("Bienvenue sur Students for Students !", "Bonjour " + user.UserName + ",\n\nNous vous souhaitons la bienvenue sur notre application Students for Students !\n\nCordialement,\nL'équipe de Students for Students.", user.Email, null);
                await _hubContext.Clients.All.SendAsync("updateUsersCount");
                await _hubContext.Clients.All.SendAsync("updateUsers");

                return Ok(new SuccessViewModel(false, "Compte créé avec succès"));

            }
            catch (Exception)
            {
                return NotFound(new ErrorViewModel(true, "Cursus invalide"));
            }
        }
    }
}
