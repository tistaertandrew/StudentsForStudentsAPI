using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.DbModels;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet de récupérer les informations de l'école associée à l'application
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly DatabaseContext _context;

        /// <summary>
        /// Constructeur du controller qui permet de récupérer les informations de l'école associée à l'application
        /// </summary>
        /// <param name="context">Un objet permettant d'intéragir avec la base de données</param>
        public SchoolController(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer les sections de l'école via un identifiant
        /// </summary>
        /// <param name="id">L'identifiant de la section à récupérer</param>
        /// <returns>La section ciblée</returns>
        [AllowAnonymous]
        [HttpGet("Section/{id:int}")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère une section de l'école via son identifiant")]
        [SwaggerResponse(StatusCodes.Status200OK, "La section a été récupérée", typeof(Section))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "La section n'a pas été trouvée")]
        public ActionResult<Section> GetSectionById(int id) => Ok(_context.Sections.FirstOrDefault(s => s.Id == id));

        /// <summary>
        /// Route (GET) qui permet de récupérer les sections de l'école
        /// </summary>
        /// <returns>Une liste de sections</returns>
        [AllowAnonymous]
        [HttpGet("Section")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère toutes les sections de l'école")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les sections ont été récupérées", typeof(List<Section>))]
        public ActionResult<List<Section>> GetSections() => Ok(_context.Sections.ToList());

        /// <summary>
        /// Route (GET) qui permet de récupérer les cursus de l'école via l'identifiant de la section associée
        /// </summary>
        /// <param name="id">L'identifiant de la section associée</param>
        /// <returns>Une liste de cursus</returns>
        [AllowAnonymous]
        [HttpGet("Cursus/{id:int}")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère tous les cursus de l'école via l'identifiant de la section associée")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les cursus ont été récupérés", typeof(List<Cursus>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "La section n'a pas été trouvée")]
        public ActionResult<List<Cursus>> GetCursusBySectionId(int id) => Ok(_context.Cursus
            .Include(c => c.Section)
            .Where(c => c.Section.Id == id)
            .ToList());

        /// <summary>
        /// Route (GET) qui permet de récupérer les cursus de l'école
        /// </summary>
        /// <returns>Une liste de cursus</returns>
        [AllowAnonymous]
        [HttpGet("Cursus")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère tous les cursus de l'école")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les cursus ont été récupérés", typeof(List<Cursus>))]
        public ActionResult<List<Cursus>> GetCursus() => Ok(_context.Cursus
            .Include(c => c.Section)
            .ToList());

        /// <summary>
        /// Route (GET) qui permet de récupérer les cours de l'école via l'identifiant du cursus associé
        /// </summary>
        /// <param name="id">L'identifiant du cursus associé</param>
        /// <returns>Une liste de cours</returns>
        [AllowAnonymous]
        [HttpGet("Course/{id:int}")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère tous les cours de l'école via l'identifiant du cursus associé")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les cours ont été récupérés", typeof(List<Course>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Le cursus n'a pas été trouvé")]
        public ActionResult<List<Course>> GetCoursesByCursusId(int id) => Ok(_context.Courses
            .Include(c => c.Cursus)
            .ThenInclude(c => c.Section)
            .Where(c => c.Cursus.Id == id)
            .ToList());

        /// <summary>
        /// Route (GET) qui permet de récupérer les cours de l'école
        /// </summary>
        /// <returns>Une liste de cours</returns>
        [AllowAnonymous]
        [HttpGet("Course")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Récupère tous les cours de l'école")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les cours ont été récupérés", typeof(List<Course>))]
        public ActionResult<List<Course>> GetCourses() => Ok(_context.Courses
            .Include(c => c.Cursus)
            .ThenInclude(c => c.Section)
            .ToList());
    }
}