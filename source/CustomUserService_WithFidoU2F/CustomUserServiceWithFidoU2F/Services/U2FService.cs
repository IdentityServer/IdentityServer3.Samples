using System;
using System.Collections.Generic;
using System.Linq;
using SampleApp.Models.U2F;
using SampleApp.Repositories;
using u2flib;
using u2flib.Data;
using u2flib.Data.Messages;
using u2flib.Util;

namespace SampleApp.Services
{
    public class U2FService
    {
        // This must be the host origin of the site (idsrv) - make sure it does not contain a trailing /
        private const string AppId = "https://localhost:44333";

        private readonly IUserRepository _userRepository;

        public U2FService()
        {
            _userRepository = new InMemoryUserRepository();
        }


        public ServerRegisterResponse GenerateServerChallenge(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var startedRegistration = U2F.StartRegistration(AppId);

            // We need to temporarily store the authentication request for retrieval upon completion
            _userRepository.SaveUserAuthenticationRequest(username, startedRegistration.AppId, startedRegistration.Challenge, startedRegistration.Version);

            return new ServerRegisterResponse
            {
                AppId = startedRegistration.AppId,
                Challenge = startedRegistration.Challenge,
                Version = startedRegistration.Version
            };
        }

        public bool CompleteRegistration(string userName, string deviceResponse)
        {
            if (string.IsNullOrWhiteSpace(deviceResponse))
            {
                return false;
            }

            // Retrieve user from repository
            var user = _userRepository.FindUser(userName);

            if (user == null 
                || user.DeviceAuthenticationRequests == null 
                ||
                user.DeviceAuthenticationRequests.Count == 0)
            {
                return false;
            }

            var registerResponse = DataObject.FromJson<RegisterResponse>(deviceResponse);

            // When the user is registration they should only ever have one auth request.
            var authenticationRequest = user.DeviceAuthenticationRequests.First();

            var startedRegistration = new StartedRegistration(authenticationRequest.Challenge, authenticationRequest.AppId);
            var registration = U2F.FinishRegistration(startedRegistration, registerResponse);

            _userRepository.RemoveUsersAuthenticationRequests(userName);
            _userRepository.AddDeviceRegistration(userName, registration.AttestationCert, registration.Counter, registration.KeyHandle, registration.PublicKey);

            return true;
        }

        public bool AuthenticateUser(string userName, string deviceResponse)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(deviceResponse))
                return false;

            var user = _userRepository.FindUser(userName);
            if (user == null)
                return false;

            var authenticateResponse = DataObject.FromJson<AuthenticateResponse>(deviceResponse);

            var device = user.Devices.FirstOrDefault(f => f.KeyHandle.SequenceEqual(Utils.Base64StringToByteArray(authenticateResponse.KeyHandle)));

            if (device == null || user.DeviceAuthenticationRequests == null)
                return false;

            // User will have a authentication request for each device they have registered so get the one that matches the device key handle
            var authenticationRequest = user.DeviceAuthenticationRequests.First(f => f.KeyHandle.Equals(authenticateResponse.KeyHandle));
            var registration = new DeviceRegistration(device.KeyHandle, device.PublicKey, device.AttestationCert, Convert.ToUInt32(device.Counter));

            var authentication = new StartedAuthentication(authenticationRequest.Challenge, authenticationRequest.AppId, authenticationRequest.KeyHandle);

            U2F.FinishAuthentication(authentication, authenticateResponse, registration);

            _userRepository.RemoveUsersAuthenticationRequests(user.Username);

            return true;
        }

        public List<ServerChallenge> GenerateServerChallenges(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            var user = _userRepository.FindUser(userName);

            if (user == null)
            {
                return null;
            }

            // We only want to generate challenges for un-compromised devices
            var device = user.Devices.Where(w => w.IsCompromised == false).ToList();

            if (device.Count == 0)
            {
                return null;
            }

            _userRepository.RemoveUsersAuthenticationRequests(userName);

            var serverChallenges = new List<ServerChallenge>();
            foreach (var registeredDevice in device)
            {
                var registration = new DeviceRegistration(registeredDevice.KeyHandle, registeredDevice.PublicKey, registeredDevice.AttestationCert, Convert.ToUInt32(registeredDevice.Counter));
                var startedAuthentication = U2F.StartAuthentication(AppId, registration);

                serverChallenges.Add(new ServerChallenge
                {
                    appId = startedAuthentication.AppId,
                    challenge = startedAuthentication.Challenge,
                    keyHandle = startedAuthentication.KeyHandle,
                    version = startedAuthentication.Version
                });

                _userRepository.SaveUserAuthenticationRequest(userName, startedAuthentication.AppId, startedAuthentication.Challenge,
                                                              startedAuthentication.KeyHandle);
            }

            return serverChallenges;
        }
    }
}