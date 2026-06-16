using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProSpace.Application.Common.Interfaces;

namespace ProSpace.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        /// <summary>
        /// App configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public (string Token, DateTime Expiration) GenerateJwtToken(string userId, string userName, IList<string> roles)
        {
            var secretKey = _configuration["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured in the configuration framework storage.");

            var issuer = _configuration["JWT:ValidIssuer"];
            var audience = _configuration["JWT:ValidAudience"];
            var expiryInMinutes = double.Parse(_configuration["JWT:ExpiryInMinutes"] ?? "60");

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var expiration = DateTime.UtcNow.AddMinutes(expiryInMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: expiration,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiration);
        }
    }
}