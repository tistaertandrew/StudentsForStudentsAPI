﻿using Microsoft.AspNetCore.Identity;

namespace StudentsForStudentsAPI.Models
{
    public class User : IdentityUser 
    {
        public Cursus Cursus { get; set; }
        public string? CalendarLink { get; set; } = null;
    }
}
