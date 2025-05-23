﻿using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs.User
{
    public class LoginUserDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
