using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Owin;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;

[assembly: OwinStartup(typeof(Passsport.Startup))]

namespace Passsport
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            var secret = ConfigurationManager.AppSettings["JwtSecret"];
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];

            var key = Encoding.UTF8.GetBytes(secret);

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
            });
        }
    }
}
