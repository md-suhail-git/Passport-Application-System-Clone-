using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace passport.Helpers
{
    public class JwtTokenHelper
    {
        // Updated method to include
        //
        //
        //
        //
        // ID in the token
        public static string GenerateToken(int userId, string fullName, string role, string loginID)
        {
            var secret = ConfigurationManager.AppSettings["JwtSecret"];
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];
            var expiryMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["JwtExpireMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // optional: keep userId
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role),
                new Claim("loginID", loginID) // <-- added loginID claim
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
