/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Extensions;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using ClaimHelper = BrockAllen.MembershipReboot.ClaimsExtensions;
using IdSvr3 = IdentityServer3.Core;

namespace SelfHost.IdentityServer3.MembershipReboot
{
    public class MembershipRebootUserService<TAccount> : IUserService
        where TAccount : UserAccount
    {
        public string DisplayNameClaimType { get; set; }
        
        protected readonly UserAccountService<TAccount> userAccountService;

        public MembershipRebootUserService(UserAccountService<TAccount> userAccountService)
        {
            if (userAccountService == null) throw new ArgumentNullException("userAccountService");

            this.userAccountService = userAccountService;
        }

        public virtual Task<IEnumerable<Claim>> GetProfileDataAsync(
            ClaimsPrincipal subject,
            IEnumerable<string> requestedClaimTypes = null)
        {
            var acct = userAccountService.GetByID(subject.GetSubjectId().ToGuid());
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = GetClaimsFromAccount(acct);
            if (requestedClaimTypes != null && requestedClaimTypes.Any())
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }

        protected virtual IEnumerable<Claim> GetClaimsFromAccount(TAccount account)
        {
            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, GetSubjectForAccount(account)),
                new Claim(Constants.ClaimTypes.UpdatedAt, account.LastUpdated.ToEpochTime().ToString(), ClaimValueTypes.Integer),
                new Claim("tenant", account.Tenant),
                new Claim(Constants.ClaimTypes.PreferredUserName, account.Username),
            };

            if (!String.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(Constants.ClaimTypes.Email, account.Email));
                claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, account.IsAccountVerified ? "true" : "false"));
            }

            if (!String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, account.MobilePhoneNumber));
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, !String.IsNullOrWhiteSpace(account.MobilePhoneNumber) ? "true" : "false"));
            }

            claims.AddRange(account.Claims.Select(x => new Claim(x.Type, x.Value)));
            claims.AddRange(userAccountService.MapClaims(account));

            return claims;
        }

        protected virtual string GetSubjectForAccount(TAccount account)
        {
            return account.ID.ToString("D");
        }

        protected virtual string GetDisplayNameForAccount(Guid accountID)
        {
            var acct = userAccountService.GetByID(accountID);
            var claims = GetClaimsFromAccount(acct);

            string name = null;
            if (DisplayNameClaimType != null)
            {
                name = acct.Claims.Where(x => x.Type == DisplayNameClaimType).Select(x => x.Value).FirstOrDefault();
            }
            if (name == null) name = acct.Claims.Where(x => x.Type == Constants.ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();
            if (name == null) name = acct.Claims.Where(x => x.Type == ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();
            if (name == null) name = acct.Username;

            return name;
        }

        protected virtual Task<IEnumerable<Claim>> GetClaimsForAuthenticateResultAsync(TAccount account)
        {
            return Task.FromResult((IEnumerable<Claim>)null);
        }
        
        public virtual Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }
        
        public virtual async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
        {
            try
            {
                TAccount account;
                if (ValidateLocalCredentials(username, password, message, out account))
                {
                    var result = await PostAuthenticateLocalAsync(account, message);
                    if (result != null) return result;

                    var subject = GetSubjectForAccount(account);
                    var name = GetDisplayNameForAccount(account.ID);

                    var claims = await GetClaimsForAuthenticateResultAsync(account);
                    return new AuthenticateResult(subject, name, claims);
                }

                if (account != null)
                {
                    if (!account.IsLoginAllowed)
                    {
                        return new AuthenticateResult("Account is not allowed to login");
                    }

                    if (account.IsAccountClosed)
                    {
                        return new AuthenticateResult("Account is closed");
                    }
                }

                return null;
            }
            catch(ValidationException ex)
            {
                return new AuthenticateResult(ex.Message);
            }
        }

        protected virtual Task<AuthenticateResult> PostAuthenticateLocalAsync(TAccount account, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        protected virtual bool ValidateLocalCredentials(string username, string password, SignInMessage message, out TAccount account)
        {
            var tenant = String.IsNullOrWhiteSpace(message.Tenant) ? userAccountService.Configuration.DefaultTenant : message.Tenant;
            return userAccountService.Authenticate(tenant, username, password, out account);
        }

        public virtual async Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
        {
            if (externalUser == null)
            {
                throw new ArgumentNullException("externalUser");
            }

            try
            {
                var tenant = String.IsNullOrWhiteSpace(message.Tenant) ? userAccountService.Configuration.DefaultTenant : message.Tenant;
                var acct = this.userAccountService.GetByLinkedAccount(tenant, externalUser.Provider, externalUser.ProviderId);
                if (acct == null)
                {
                    return await ProcessNewExternalAccountAsync(tenant, externalUser.Provider, externalUser.ProviderId, externalUser.Claims);
                }
                else
                {
                    return await ProcessExistingExternalAccountAsync(acct.ID, externalUser.Provider, externalUser.ProviderId, externalUser.Claims);
                }
            }
            catch (ValidationException ex)
            {
                return new AuthenticateResult(ex.Message);
            }
        }

        protected virtual async Task<AuthenticateResult> ProcessNewExternalAccountAsync(string tenant, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = await InstantiateNewAccountFromExternalProviderAsync(provider, providerId, claims);
            user = userAccountService.CreateAccount(
                tenant,
                Guid.NewGuid().ToString("N"), 
                null, null,
                null, null, 
                user);
            
            userAccountService.AddOrUpdateLinkedAccount(user, provider, providerId);

            var result = await AccountCreatedFromExternalProviderAsync(user.ID, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(user.ID, provider);
        }

        protected virtual Task<TAccount> InstantiateNewAccountFromExternalProviderAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            // we'll let the default creation happen, but can override to initialize properties if needed
            return Task.FromResult<TAccount>(null);
        }

        protected virtual async Task<AuthenticateResult> AccountCreatedFromExternalProviderAsync(Guid accountID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            SetAccountEmail(accountID, ref claims);
            SetAccountPhone(accountID, ref claims);

            return await UpdateAccountFromExternalClaimsAsync(accountID, provider, providerId, claims);
        }

        protected virtual async Task<AuthenticateResult> SignInFromExternalProviderAsync(Guid accountID, string provider)
        {
            var account = userAccountService.GetByID(accountID);
            var claims = await GetClaimsForAuthenticateResultAsync(account);
            
            return new AuthenticateResult(
                subject: accountID.ToString("D"),
                name: GetDisplayNameForAccount(accountID),
                claims:claims,
                identityProvider: provider,
                authenticationMethod: IdSvr3.Constants.AuthenticationMethods.External);
        }

        protected virtual Task<AuthenticateResult> UpdateAccountFromExternalClaimsAsync(Guid accountID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            userAccountService.AddClaims(accountID, new UserClaimCollection(claims));
            return Task.FromResult<AuthenticateResult>(null);
        }

        protected virtual async Task<AuthenticateResult> ProcessExistingExternalAccountAsync(Guid accountID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            return await SignInFromExternalProviderAsync(accountID, provider);
        }

        protected virtual void SetAccountEmail(Guid accountID, ref IEnumerable<Claim> claims)
        {
            var email = ClaimHelper.GetValue(claims, Constants.ClaimTypes.Email);
            if (email != null)
            {
                var acct = userAccountService.GetByID(accountID);
                if (acct.Email == null)
                {
                    try
                    {
                        var email_verified = ClaimHelper.GetValue(claims, Constants.ClaimTypes.EmailVerified);
                        if (email_verified != null && email_verified == "true")
                        {
                            userAccountService.SetConfirmedEmail(acct.ID, email);
                        }
                        else
                        {
                            userAccountService.ChangeEmailRequest(acct.ID, email);
                        }

                        var emailClaims = new string[] { Constants.ClaimTypes.Email, Constants.ClaimTypes.EmailVerified };
                        claims = claims.Where(x => !emailClaims.Contains(x.Type));
                    }
                    catch (ValidationException)
                    {
                        // presumably the email is already associated with another account
                        // so eat the validation exception and let the claim pass thru
                    }
                }
            }
        }

        protected virtual void SetAccountPhone(Guid accountID, ref IEnumerable<Claim> claims)
        {
            var phone = ClaimHelper.GetValue(claims, Constants.ClaimTypes.PhoneNumber);
            if (phone != null)
            {
                var acct = userAccountService.GetByID(accountID);
                if (acct.MobilePhoneNumber == null)
                {
                    try
                    {
                        var phone_verified = ClaimHelper.GetValue(claims, Constants.ClaimTypes.PhoneNumberVerified);
                        if (phone_verified != null && phone_verified == "true")
                        {
                            userAccountService.SetConfirmedMobilePhone(acct.ID, phone);
                        }
                        else
                        {
                            userAccountService.ChangeMobilePhoneRequest(acct.ID, phone);
                        }

                        var phoneClaims = new string[] { Constants.ClaimTypes.PhoneNumber, Constants.ClaimTypes.PhoneNumberVerified };
                        claims = claims.Where(x => !phoneClaims.Contains(x.Type));
                    }
                    catch (ValidationException)
                    {
                        // presumably the phone is already associated with another account
                        // so eat the validation exception and let the claim pass thru
                    }
                }
            }
        }

        public virtual Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            var acct = userAccountService.GetByID(subject.GetSubjectId().ToGuid());
            if (acct == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(!acct.IsAccountClosed && acct.IsLoginAllowed);
        }

        public virtual Task SignOutAsync(ClaimsPrincipal subject)
        {
            return Task.FromResult<object>(null);
        }
    }
    
    static class Extensions
    {
        public static Guid ToGuid(this string s)
        {
            Guid g;
            if (!String.IsNullOrWhiteSpace(s) &&
                Guid.TryParse(s, out g))
            {
                return g;
            }

            return Guid.Empty;
        }
    }
}
