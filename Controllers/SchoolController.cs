using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.DbModels;

namespace StudentsForStudentsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public SchoolController(DatabaseContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("Section/{id:int}")]
        [Produces("application/json")]
        public ActionResult<Section> GetSectionById(int id) => Ok(_context.Sections.FirstOrDefault(s => s.Id == id));

        [AllowAnonymous]
        [HttpGet("Section")]
        [Produces("application/json")]
        public ActionResult<List<Section>> GetSections() => Ok(_context.Sections.ToList());

        [AllowAnonymous]
        [HttpGet("Cursus/{id:int}")]
        [Produces("application/json")]
        public ActionResult<List<Cursus>> GetCursusBySectionId(int id) => Ok(_context.Cursus
            .Include(c => c.Section)
            .Where(c => c.Section.Id == id)
            .ToList());

        [AllowAnonymous]
        [HttpGet("Cursus")]
        [Produces("application/json")]
        public ActionResult<List<Cursus>> GetCursus() => Ok(_context.Cursus
            .Include(c => c.Section)
            .ToList());

        [AllowAnonymous]
        [HttpGet("Course/{id:int}")]
        [Produces("application/json")]
        public ActionResult<List<Course>> GetCoursesByCursusId(int id) => Ok(_context.Courses
            .Include(c => c.Cursus)
            .ThenInclude(c => c.Section)
            .Where(c => c.Cursus.Id == id)
            .ToList());

        [AllowAnonymous]
        [HttpGet("Course")]
        [Produces("application/json")]
        public ActionResult<List<Course>> GetCourses() => Ok(_context.Courses
            .Include(c => c.Cursus)
            .ThenInclude(c => c.Section)
            .ToList());
    }
}