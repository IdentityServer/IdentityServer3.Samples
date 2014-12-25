using Sample;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcFormPostClient.Controllers
{
	public class AccountController : Controller
	{
        public ActionResult SignIn()
        {
	        var state = Guid.NewGuid().ToString("N");
	        var nonce = Guid.NewGuid().ToString("N");

            var url = Constants.AuthorizeEndpoint +
                "?client_id=implicitclient" +
                "&response_type=id_token" +
                "&scope=openid email" +
                "&redirect_uri=http://localhost:11716/account/signInCallback" +
                "&response_mode=form_post" +
                "&state=" + state +
                "&nonce=" + nonce;
            
	        SetTempCookie(state, nonce);
	        return Redirect(url);
        }

        [HttpPost]
        public async Task<ActionResult> SignInCallback()
        {
	        var token = Request.Form["id_token"];
	        var state = Request.Form["state"];

	        var claims = await ValidateIdentityTokenAsync(token, state);

	        var id = new ClaimsIdentity(claims, "Cookies");
	        Request.GetOwinContext().Authentication.SignIn(id);

	        return Redirect("/");
        }

        private async Task<IEnumerable<Claim>> ValidateIdentityTokenAsync(string token, string state)
        {
            var certString = "MIIDBTCCAfGgAwIBAgIQNQb+T2ncIrNA6cKvUA1GWTAJBgUrDgMCHQUAMBIxEDAOBgNVBAMTB0RldlJvb3QwHhcNMTAwMTIwMjIwMDAwWhcNMjAwMTIwMjIwMDAwWjAVMRMwEQYDVQQDEwppZHNydjN0ZXN0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqnTksBdxOiOlsmRNd+mMS2M3o1IDpK4uAr0T4/YqO3zYHAGAWTwsq4ms+NWynqY5HaB4EThNxuq2GWC5JKpO1YirOrwS97B5x9LJyHXPsdJcSikEI9BxOkl6WLQ0UzPxHdYTLpR4/O+0ILAlXw8NU4+jB4AP8Sn9YGYJ5w0fLw5YmWioXeWvocz1wHrZdJPxS8XnqHXwMUozVzQj+x6daOv5FmrHU1r9/bbp0a1GLv4BbTtSh4kMyz1hXylho0EvPg5p9YIKStbNAW9eNWvv5R8HN7PPei21AsUqxekK0oW9jnEdHewckToX7x5zULWKwwZIksll0XnVczVgy7fCFwIDAQABo1wwWjATBgNVHSUEDDAKBggrBgEFBQcDATBDBgNVHQEEPDA6gBDSFgDaV+Q2d2191r6A38tBoRQwEjEQMA4GA1UEAxMHRGV2Um9vdIIQLFk7exPNg41NRNaeNu0I9jAJBgUrDgMCHQUAA4IBAQBUnMSZxY5xosMEW6Mz4WEAjNoNv2QvqNmk23RMZGMgr516ROeWS5D3RlTNyU8FkstNCC4maDM3E0Bi4bbzW3AwrpbluqtcyMN3Pivqdxx+zKWKiORJqqLIvN8CT1fVPxxXb/e9GOdaR8eXSmB0PgNUhM4IjgNkwBbvWC9F/lzvwjlQgciR7d4GfXPYsE1vf8tmdQaY8/PtdAkExmbrb9MihdggSoGXlELrPA91Yce+fiRcKY3rQlNWVd4DOoJ/cPXsXwry8pWjNCo5JD8Q+RQ5yZEy7YPoifwemLhTdsBz3hlZr28oCGJ3kbnpW0xGvQb3VHSTVVbeei0CfXoW6iz1";
            var cert = new X509Certificate2(Convert.FromBase64String(certString));

	        var result = await Request
                .GetOwinContext()
                .Authentication
                .AuthenticateAsync("TempCookie");
	        
            if (result == null)
	        {
		        throw new InvalidOperationException("No temp cookie");
	        }

	        if (state != result.Identity.FindFirst("state").Value)
	        {
		        throw new InvalidOperationException("invalid state");
	        }

	        var parameters = new TokenValidationParameters
	        {
		        ValidAudience = "implicitclient",
		        ValidIssuer = Constants.BaseAddress,
		        IssuerSigningToken = new X509SecurityToken(cert)
	        };

            var handler = new JwtSecurityTokenHandler();
            SecurityToken jwt;
	        var id = handler.ValidateToken(token, parameters, out jwt);

	        if (id.FindFirst("nonce").Value != 
                result.Identity.FindFirst("nonce").Value)
	        {
		        throw new InvalidOperationException("Invalid nonce");
	        }

	        Request
                .GetOwinContext()
                .Authentication
                .SignOut("TempCookie");
	        
            return id.Claims;
        }

		public ActionResult SignOut()
		{
			Request.GetOwinContext().Authentication.SignOut();
			return Redirect(Constants.LogoutEndpoint);
		}

		private void SetTempCookie(string state, string nonce)
		{
			var tempId = new ClaimsIdentity("TempCookie");
			tempId.AddClaim(new Claim("state", state));
			tempId.AddClaim(new Claim("nonce", nonce));

			Request.GetOwinContext().Authentication.SignIn(tempId);
		}
	}
}