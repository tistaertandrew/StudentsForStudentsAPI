using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public UserController(DatabaseContext context, UserManager<User> userManager, IConfiguration config, IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
            _userService = userService;
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
            
            var userFromManager = await _userManager.FindByIdAsync(id);
            var userFromContextWithCursus = _context.Users.Where(user => user.Id == userFromManager.Id).Include(user => user.Cursus).First();
            return Ok(new UserViewModel(userFromContextWithCursus, Token.CreateToken(userFromContextWithCursus, _userManager, _config)));
        }

        [AllowAnonymous]
        [HttpPost("Google")]
        [Produces("application/json")]
        public async Task<ActionResult<UserViewModel>> Google(OAuthViewModel request)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(request.Credentials, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    return NotFound(new ErrorViewModel(true, payload.Email));
                }

                return Ok(new UserViewModel(user, Token.CreateToken(user, _userManager, _config)));
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

            var user = _context.Users.Where(user => user.Email == request.Email).Include(user => user.Cursus).First();
            user = await _userManager.FindByEmailAsync(request.Email);
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

            return Ok(new UserViewModel(user, Token.CreateToken(user, _userManager, _config)));
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
                return Ok(new SuccessViewModel(false, "Compte créée avec succès"));
            }
            catch (Exception)
            {
                return NotFound(new ErrorViewModel(true, "Cursus invalide"));
            }
        }
    }
}
