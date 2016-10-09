using System;
using System.Collections.Generic;
using System.Linq;
using SampleApp.Models;
using SampleApp.Models.U2F;

namespace SampleApp.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private static readonly IEnumerable<U2FUser> Users;

        static InMemoryUserRepository()
        {
            Users = IdentityServer3.Host.Config.Users.Get();
        }

        public U2FUser FindUser(string username)
        {
            return Users.FirstOrDefault(f => f.Username.Equals(username));
        }

        public U2FUser FindUserById(string id)
        {
            return Users.FirstOrDefault(f => f.Subject.Equals(id));
        }

        public void RemoveUsersAuthenticationRequests(string username)
        {
            var user = Users.FirstOrDefault(f => f.Username.Equals(username.Trim()));
            if (user == null)
            {
                return;
            }

            user.DeviceAuthenticationRequests.Clear();
        }

        public void SaveUserAuthenticationRequest(string username, string appId, string challenge, string keyHandle)
        {
            var user = Users.FirstOrDefault(f => f.Username.Equals(username.Trim()));

            if (user == null)
            {
                return;
            }

            if (user.DeviceAuthenticationRequests == null)
            {
                user.DeviceAuthenticationRequests = new List<DeviceAuthenticationRequest>();
            }

            user.DeviceAuthenticationRequests.Add(
                new DeviceAuthenticationRequest
                {
                    AppId = appId,
                    Challenge = challenge,
                    KeyHandle = keyHandle
                });
        }

        public void AddDeviceRegistration(string username, byte[] attestationCert, uint counter, byte[] keyHandle, byte[] publicKey)
        {
            var user = Users.FirstOrDefault(f => f.Username.Equals(username));

            if (user == null)
            {
                return;
            }

            user.DeviceAuthenticationRequests.Clear();
            user.Devices.Add(new Device
            {
                AttestationCert = attestationCert,
                Counter = Convert.ToInt32(counter),
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                KeyHandle = keyHandle,
                PublicKey = publicKey
            });
        }
    }
}