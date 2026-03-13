using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Tokens;
using Owin;
using System.Text;
using System.Configuration;

namespace passport
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];
            var secret = ConfigurationManager.AppSettings["JwtSecret"];

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

                        ValidateLifetime = true,
                        ClockSkew = System.TimeSpan.Zero
                    }
                }
            );
        }
    }
}
