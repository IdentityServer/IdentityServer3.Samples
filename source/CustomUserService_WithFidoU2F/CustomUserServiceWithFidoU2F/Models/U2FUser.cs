using System;
using System.Collections.Generic;
using System.Security.Claims;
using SampleApp.Models.U2F;

namespace SampleApp.Models
{
    public class U2FUser
    {
        public U2FUser()
        {
            Devices = new List<Device>();
            DeviceAuthenticationRequests = new List<DeviceAuthenticationRequest>();
        }

        public string Subject { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool AcceptedEula { get; set; }
        public List<Claim> Claims { get; set; }
        public List<Device> Devices { get; set; }
        public List<DeviceAuthenticationRequest> DeviceAuthenticationRequests { get; set; }
    }
}