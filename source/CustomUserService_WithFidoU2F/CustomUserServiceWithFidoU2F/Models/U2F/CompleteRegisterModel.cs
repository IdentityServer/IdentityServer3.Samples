using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models.U2F
{
    public class CompleteRegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Display(Name = "Challenge")]
        public string Challenge { get; set; }

        [Display(Name = "Version")]
        public string Version { get; set; }

        [Display(Name = "App ID")]
        public string AppId { get; set; }

        public string DeviceResponse { get; set; }
    }
}