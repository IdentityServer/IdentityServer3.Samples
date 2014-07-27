using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SampleApp.Models
{
    public class RegisterModel
    {
        [Required]
        public string First { get; set; }
        [Required]
        public string Last { get; set; }
    }
}