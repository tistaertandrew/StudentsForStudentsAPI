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
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            var form = new Form();

            if (user == null) form.SenderEmail = request.Email;
            else form.Sender = user;

            form.Date = DateTime.Now;
            form.Subject = request.Subject;
            form.Message = request.Message;
            form.Status = false;

            _mailService.SendMail(form.Subject, form.Message, null, request.Email);
            _mailService.SendMail("Prise de contact avec un administrateur", "Bonjour, \n\nVotre prise de contact a bien été prise en compte. Nous vous répondrons dans les plus brefs délais.\n\nCordialement,\nL'équipe de Students for Students.", request.Email, null);

            _context.Forms.Add(form);
            _context.SaveChanges();
            return Ok(new SuccessViewModel(false, "Formulaire envoyé"));
        }
    }
}
