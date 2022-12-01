using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.MailService;
using System.Net;

namespace StudentsForStudentsAPI.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService;

        public RequestController(DatabaseContext context, IUserService userService, UserManager<User> userManager, IMailService mailService)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
            _mailService = mailService;
        }

        [HttpDelete("{requestId}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public IActionResult DeleteRequest(int requestId)
        {
            if (!_userService.IsTokenValid()) return Unauthorized();
            var request = _context.Requests.Include(r => r.Sender).Where(r => r.Id == requestId).FirstOrDefault();
            if (request == null) return NotFound(new ErrorViewModel(true, "La demande n'existe pas"));

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            if (user == null) return NotFound(new ErrorViewModel(true, "L'utilisateur n'existe pas"));
            if (!request.Sender.Id.Equals(user.Id)) return BadRequest(new ErrorViewModel(true, "Vous n'avez pas le droit de supprimer une demande qui vous n'appartient pas"));
            if (request.Status) return BadRequest(new ErrorViewModel(true, "Vous ne pouvez pas supprimer une demande acceptée"));
            
            _mailService.SendMail($"Suppression de la demande \"{request.Name}\"", $"Bonjour {user.UserName}, \n\nVotre demande \"{request.Name}\" a bien été supprimée. \n\nCordialement, \nL'équipe de Students for Students.", user.Email, null);
            _context.Requests.Remove(request);
            _context.SaveChanges();
            return Ok(new SuccessViewModel(false, "Demande supprimée avec succès"));
        }

        [HttpPut("{requestId}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public IActionResult UpdateRequest(int requestId)
        {
            {
                if (!_userService.IsTokenValid()) return Unauthorized();

                var request = _context.Requests.Include(r => r.Sender).Where(r => r.Id == requestId).FirstOrDefault();
                if (request == null) return NotFound(new ErrorViewModel(true, "La demande n'existe pas"));

                var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
                if (user == null) return NotFound(new ErrorViewModel(true, "L'utilisateur n'existe pas"));
                if (request.Sender.Id.Equals(user.Id)) return BadRequest(new ErrorViewModel(true, "Vous ne pouvez pas accepter une de vos propres demandes"));
                if (request.Status) return BadRequest(new ErrorViewModel(true, "Cette demande a déjà été acceptée"));

                request.Status = !request.Status;
                request.Handler = user;

                _mailService.SendMail($"Demande \"{request.Name}\" acceptée", $"Bonjour {request.Sender.UserName}, \n\nVotre demande \"{request.Name}\" a été acceptée par {request.Handler.UserName}. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter. \n\nCordialement, \nL'équipe de Students for Students.", request.Sender.Email, null);
                _mailService.SendMail($"Demande \"{request.Name}\" acceptée", $"Bonjour {request.Handler.UserName}, \n\nVous avez accepté la demande \"{request.Name}\" de {request.Sender.UserName}. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter. \n\nCordialement, \nL'équipe de Students for Students.", request.Handler.Email, null);
                _context.SaveChanges();
                return Ok(new SuccessViewModel(false, "Demande acceptée avec succès"));
            }
        }

        [HttpGet("{own}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public IActionResult GetRequests(bool own)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            if (!_userService.IsTokenValid()) return Unauthorized();
            
            if (!own)
            {
                var requests = _context.Requests
                    .Include(r => r.Place)
                    .Include(r => r.Course)
                    .ThenInclude(c => c.Cursus)
                    .ThenInclude(c => c.Section)
                    .Include(r => r.Sender)
                    .Where(r => r.Sender.Id != _userService.GetUserIdFromToken() && !r.Status)
                    .OrderBy(r => r.Date)
                    .ToList();

                var finalRequests = requests.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    description = r.Description,
                    date = r.Date.ToString("dd/MM/yyyy"),
                    status = r.Status,
                    sender = r.Sender.UserName,
                    handler = "nobody",
                    place = r.Place,
                    course = r.Course
                });

                return Ok(finalRequests);
            } else
            {
                var requests = _context.Requests
                    .Include(r => r.Place)
                    .Include(r => r.Course)
                    .ThenInclude(c => c.Cursus)
                    .ThenInclude(c => c.Section)
                    .Include(r => r.Sender)
                    .Include(r => r.Handler)
                    .Where(r => r.Sender.Id == _userService.GetUserIdFromToken() || r.Handler.Id == _userService.GetUserIdFromToken() || r.Handler.Id == _userService.GetUserIdFromToken())
                    .OrderBy(r => r.Date)
                    .ToList();

                var finalRequests = requests.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    description = r.Description,
                    date = r.Date.ToString("dd/MM/yyyy"),
                    status = r.Status,
                    sender = r.Sender.UserName,
                    handler = r.Handler == null ? "nobody" : r.Handler.UserName,
                    place = r.Place,
                    course = r.Course
                });

                return Ok(finalRequests);
            }
                
        }

        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [Produces("application/json")]
        public ActionResult CreateRequest(RequestViewModel request)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            if (!_userService.IsTokenValid()) return Unauthorized();

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            if (user == null) return NotFound(new ErrorViewModel(true, "Aucun utilisateur associé à ce token"));

            try
            {
                var newRequest = new Request
                {
                    Name = request.Name,
                    Description = request.Description,
                    Status = false,
                    Date = DateTime.Now,
                    Sender = user,
                    Handler = null,
                    Place = _context.Places.Find(request.PlaceId),
                    Course = _context.Courses.Find(request.CourseId)
                };

                _mailService.SendMail($"Création de la demande \"{newRequest.Name}\"", $"Bonjour {user.UserName}, \n\nVotre demande \"{newRequest.Name}\" a bien été créée. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter. \n\nCordialement, \nL'équipe de Students for Students.", user.Email, null);
                _context.Requests.Add(newRequest);
                _context.SaveChanges();
                
                return Ok(new SuccessViewModel(false, "Demande créée avec succès"));
            } catch(Exception)
            {
                return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            }
        }
    }
}
