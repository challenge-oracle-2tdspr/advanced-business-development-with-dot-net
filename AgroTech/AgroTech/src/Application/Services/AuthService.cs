using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using AgroTech.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AgroTech.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtOptions _jwtOptions;

        public AuthService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public Task<LoginResponseDTO?> LoginAsync(LoginRequestDto request)
        {
            if (request is null)
                return Task.FromResult<LoginResponseDTO?>(null);

            if (request.Username != "admin" || request.Password != "123456")
                return Task.FromResult<LoginResponseDTO?>(null);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                new Claim(JwtRegisteredClaimNames.UniqueName, request.Username),
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();

            return Task.FromResult<LoginResponseDTO?>(new LoginResponseDTO
            {
                Token = tokenHandler.WriteToken(token),
                ExpiresAt = expiresAt
            });
        }
    }
}