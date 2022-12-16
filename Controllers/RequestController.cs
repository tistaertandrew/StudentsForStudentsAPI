using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.MailService;
using System.Net;
using StudentsForStudentsAPI.Models.Mails;

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
        private readonly IHubContext<SignalRHub> _hubContext;

        public RequestController(DatabaseContext context, IUserService userService, UserManager<User> userManager, IMailService mailService, IHubContext<SignalRHub> hubContext)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
            _mailService = mailService;
            _hubContext = hubContext;
        }

        [HttpDelete("{requestId}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteRequest(int requestId)
        {
            if (!_userService.IsTokenValid()) return Unauthorized();
            var request = _context.Requests.Include(r => r.Sender).Where(r => r.Id == requestId).FirstOrDefault();
            if (request == null) return NotFound(new ErrorViewModel(true, "La demande n'existe pas"));

            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            if (user == null) return NotFound(new ErrorViewModel(true, "L'utilisateur n'existe pas"));
            if (!request.Sender.Id.Equals(user.Id)) return BadRequest(new ErrorViewModel(true, "Vous n'avez pas le droit de supprimer une demande qui vous n'appartient pas"));
            if (request.Status) return BadRequest(new ErrorViewModel(true, "Vous ne pouvez pas supprimer une demande acceptée"));

            _mailService.SendMail(new DeleteRequestMail($"Suppression de la demande \"{request.Name}\"", user.Email, null, new []{ user.UserName, request.Name }));
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("updateRequests");
            
            return Ok(new SuccessViewModel(false, "Demande supprimée avec succès"));
        }

        [HttpPut("{requestId}")]
        [Authorize(Roles = "Member,Admin")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateRequest(int requestId)
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
                
                _mailService.SendMail(new UpdateSenderRequestMail($"Demande \"{request.Name}\" acceptée", request.Sender.Email, null, new []{ request.Sender.UserName, request.Name, request.Handler.UserName }));
                _mailService.SendMail(new UpdateHandlerRequestMail($"Demande \"{request.Name}\" acceptée", request.Handler.Email, null, new []{ request.Handler.UserName, request.Name, request.Sender.UserName }));
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("updateRequestStatus", request.Name, request.Sender.UserName, request.Handler.UserName);
                
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
                    .Where(r => !r.Sender.Id.Equals(_userService.GetUserIdFromToken()) && !r.Status)
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
                    .Where(r => r.Sender.Id.Equals(_userService.GetUserIdFromToken()) || r.Handler.Id.Equals(_userService.GetUserIdFromToken()))
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
        public async Task<ActionResult> CreateRequest(RequestViewModel request)
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
                
                _mailService.SendMail(new AddRequestMail($"Création de la demande \"{newRequest.Name}\"", user.Email, null, new []{ user.UserName, newRequest.Name }));
                _context.Requests.Add(newRequest);
                _context.SaveChanges();

                await _hubContext.Clients.All.SendAsync("updateRequests");

                return Ok(new SuccessViewModel(false, "Demande créée avec succès"));
            } catch(Exception)
            {
                return BadRequest(new ErrorViewModel(true, "Informations invalides"));
            }
        }
    }
}
