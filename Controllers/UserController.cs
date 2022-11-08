using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
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
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;

        public UserController(DatabaseContext context, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpGet]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<UserViewModel>> WhoAmI()
        {
            var email = User?.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à ce token"));
            }
            var user = await _userManager.FindByEmailAsync(email);
            return Ok(new UserViewModel(user, Token.CreateToken(user, _userManager, _config)));
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
