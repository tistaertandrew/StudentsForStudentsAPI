﻿using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;

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
        [HttpGet("Sections/{id}")]
        [Produces("application/json")]
        public IActionResult GetSection(int id)
        {
            var section = _context.Sections.Where(s => s.Id == id).ToList();
            return Ok(section);
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
        [HttpGet("Cursus")]
        [Produces("application/json")]
        public IActionResult GetCursus()
        {
            var cursus = _context.Cursus.ToList();
            return Ok(cursus);
        }
    }
}
