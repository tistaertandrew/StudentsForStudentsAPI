using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services.MailService;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService;

        public ContactController(DatabaseContext context, UserManager<User> userManager, IMailService mailService)
        {
            _context = context;
            _userManager = userManager;
            _mailService = mailService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<SuccessViewModel>> Contact(FormViewModel request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));

            var user = await _userManager.FindByEmailAsync(request.Email);
            var form = new Form(request.Subject, request.Message, request.Email, user);

            _mailService.SendMail(form.Subject, new[] { form.Message }, "ContactToAdmin", null, request.Email);
            _mailService.SendMail("Prise de contact avec un administrateur", Array.Empty<string>(), "ContactToUser", request.Email);
            _context.Forms.Add(form);
            await _context.SaveChangesAsync();
            
            return Ok(new SuccessViewModel(false, "Mail envoyé avec succès"));
        }
    }
}
