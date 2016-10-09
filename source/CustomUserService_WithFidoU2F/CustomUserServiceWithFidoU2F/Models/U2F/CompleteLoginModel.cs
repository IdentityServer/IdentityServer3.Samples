using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models.U2F
{
    public class CompleteLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "App id")]
        public string AppId { get; set; }

        [Required]
        [Display(Name = "Version")]
        public string Version { get; set; }

        [Required]
        [Display(Name = "Device Response")]
        public string DeviceResponse { get; set; }

        [Display(Name = "Challenges")]
        public string Challenges { get; set; }
    }
}