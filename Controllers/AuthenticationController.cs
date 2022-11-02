using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;

        public AuthenticationController(DatabaseContext context, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("Sections")]
        [Produces("application/json")]
        public IActionResult GetSections()
        {
            var sections = _context.Sections.ToList();
            return Ok(sections);
        }

        [AllowAnonymous]
        [HttpGet("Cursus/{id}")]
        [Produces("application/json")]
        public IActionResult GetCursus(int id)
        {
            var cursus = _context.Cursus.Include(c => c.Section).Where(c => c.Section.Id == id).ToList();
            return Ok(cursus);
        }

        [AllowAnonymous]
        [HttpPost("Google")]
        [Produces("application/json")]
        public async Task<IActionResult> Google(OAuthViewModel request)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(request.Credentials, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    return NotFound(new ErrorViewModel(true, "Cet utilisateur n'existe pas"));
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
                user.UserName = request.Username;
                user.Email = request.Email;
                user.Cursus = _context.Cursus.Where(c => c.Id == request.CursusId).First();

                var result = request.Password != string.Empty ? await _userManager.CreateAsync(user, request.Password) : await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new ErrorViewModel(true, string.Join(" | ", result.Errors.Select(e => e.Code))));
                    //return BadRequest();
                }

                await _userManager.AddToRoleAsync(user, "Member");
                return Ok();

            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
