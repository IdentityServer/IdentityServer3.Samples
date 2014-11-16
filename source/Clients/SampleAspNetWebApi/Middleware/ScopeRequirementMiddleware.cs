using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class ScopeRequirementMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly IEnumerable<string> _scopes;

        public ScopeRequirementMiddleware(Func<IDictionary<string, object>, Task> next, params string[] scopes)
        {
            _next = next;
            _scopes = scopes;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);

            // if no token was sent - no need to validate scopes
            var principal = context.Authentication.User;
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                await _next(env);
                return;
            }

            if (ScopesFound(context))
            {
                await _next(env);
                return;
            }

            context.Response.StatusCode = 403;
            context.Response.Headers.Add("WWW-Authenticate", new[] { "Bearer error=\"insufficient_scope\"" });

            return;
        }

        private bool ScopesFound(OwinContext context)
        {
            var scopeClaims = context.Authentication.User.FindAll("scope");

            if (scopeClaims == null || scopeClaims.Count() == 0)
            {
                return false;
            }

            foreach (var scope in scopeClaims)
            {
                if (_scopes.Contains(scope.Value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}