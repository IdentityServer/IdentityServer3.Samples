namespace SampleApp.Models.U2F
{
    public class DeviceAuthenticationRequest
    {
        public string KeyHandle { get; set; }

        public string Challenge { get; set; }

        public string AppId { get; set; }

        public string Version { get; set; }
    }
}