using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OKRFeedbackService.Common;

namespace OKRFeedbackService.WebCore.Middleware
{
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly IConfiguration configuration;

        public TokenManagerMiddleware(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string authorization = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                authorization = context.Request.Headers["Token"];
                if (string.IsNullOrEmpty(authorization))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }

            var token = string.Empty;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }
            // If no token found, no further work possible
            if (string.IsNullOrEmpty(token))
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);

            if (jsonToken is JwtSecurityToken principal)
            {
                var expiryDateUnix = long.Parse(principal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDateUnix);

                string email = string.Empty;
                if (principal.Claims.FirstOrDefault(x => x.Type == "preferred_username") != null)
                    email = principal.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
                else if (principal.Claims.FirstOrDefault(x => x.Type == "email") != null)
                    email = principal.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                else if (principal.Claims.FirstOrDefault(x => x.Type == "unique_name") != null)
                    email = principal.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;

                string name = string.Empty;
                if (principal.Claims.FirstOrDefault(x => x.Type == "name") != null)
                    name = principal.Claims.FirstOrDefault(x => x.Type == "name")?.Value;


                var hasTenant = context.Request.Headers.TryGetValue("TenantId", out var tenantId);
                if (!hasTenant && context.Request.Host.Value.Contains("localhost"))
                    tenantId = configuration.GetValue<string>("TenantId");

                if (hasTenant)
                    tenantId = Encryption.DecryptRijndael(tenantId, configuration.GetValue<string>("Encryption:PrivateKey"));

                var claimList = new List<Claim>
                {
                     new Claim("email", email),
                     new Claim("token", token),
                     new Claim("name", name),
                     new Claim("tenantId", tenantId)
                };
                var claimIdentity = new ClaimsIdentity(claimList);
                context.User = new ClaimsPrincipal(claimIdentity);

                if (expiryDateTimeUtc < DateTime.UtcNow)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var myByteArray = Encoding.UTF8.GetBytes("TokenExpired");
                    await context.Response.Body.WriteAsync(myByteArray, 0, myByteArray.Length);
                    return;
                }
            }

            await next(context);
        }
    }
}
