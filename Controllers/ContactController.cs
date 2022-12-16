using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Models.DTOs;
using StudentsForStudentsAPI.Models.Mails;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services.MailService;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet de contacter l'administrateur du site
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService;

        /// <summary>
        /// Constructeur du controller ContactController qui permet de contacter l'administrateur du site
        /// </summary>
        /// <param name="context">Un objet permettant d'intéragir avec la base de données</param>
        /// <param name="userManager">Un service permettant de gérer l'utilisateur connecté</param>
        /// <param name="mailService">Un service qui permet d'envoyer des mails</param>
        public ContactController(DatabaseContext context, UserManager<User> userManager, IMailService mailService)
        {
            _context = context;
            _userManager = userManager;
            _mailService = mailService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Permet d'envoyer un mail à l'administrateur du site et de stocker le formulaire de contact dans la base de données")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le mail a été envoyé et le formulaire de contact a été stocké dans la base de données", typeof(SuccessViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Les informations du formulaire de contact ne sont pas valides", typeof(ErrorViewModel))]
        public async Task<ActionResult<SuccessViewModel>> Contact(FormDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel("Informations invalides"));

            var user = await _userManager.FindByEmailAsync(request.Email);
            var form = new Form(request.Subject, request.Message, request.Email, user);

            _mailService.SendMail(new ContactToAdminMail(form.Subject, null, request.Email, new []{ form.Message}));
            _mailService.SendMail(new ContactToUserMail("Prise de contact avec un administrateur", request.Email, null, Array.Empty<string>()));
            _context.Forms.Add(form);
            await _context.SaveChangesAsync();
            
            return Ok(new SuccessViewModel("Mail envoyé avec succès"));
        }
    }
}
