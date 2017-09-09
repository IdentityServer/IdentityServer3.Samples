using SampleApp.Models;

namespace SampleApp.Repositories
{
    public interface IUserRepository
    {
        U2FUser FindUserById(string id);

        U2FUser FindUser(string username);

        void RemoveUsersAuthenticationRequests(string username);

        void SaveUserAuthenticationRequest(string username, string appId, string challenge, string keyHandle);

        void AddDeviceRegistration(string userName, byte[] attestationCert, uint counter, byte[] keyHandle, byte[] publicKey);
    }
}