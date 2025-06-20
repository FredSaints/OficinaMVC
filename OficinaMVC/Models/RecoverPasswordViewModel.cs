﻿using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}