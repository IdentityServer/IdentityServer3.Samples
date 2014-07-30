using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SampleApp.Models
{
    public class LocalRegistrationModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string First { get; set; }
        [Required]
        public string Last { get; set; }
    }
}