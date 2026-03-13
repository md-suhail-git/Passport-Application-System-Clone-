using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace passport.Models
{
    //DTO for all Controller
    public class DTO
    {

    }
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        public string RegistrationType { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Nationality {  get; set; }    

    }
}