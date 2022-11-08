using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;

        public ContactController(DatabaseContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<SuccessViewModel>> Contact(FormViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            var form = new Form();

            if (user == null) form.SenderEmail = request.Email;
            else form.Sender = user;

            form.Date = DateTime.Now;
            form.Message = request.Message;
            form.Status = false;

            _context.Forms.Add(form);
            _context.SaveChanges();
            return Ok(new SuccessViewModel(false, "Formulaire envoyé"));
        }
    }
}
