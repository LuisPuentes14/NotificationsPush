using Microsoft.IdentityModel.Tokens;
using signalR.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace signalR.Utils.JWT
{
    public class JWT: IJWT
    {
        private readonly IConfiguration _configuration;
        public JWT(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string generateToken(User user)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]);


            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.NameIdentifier,user.login ),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Name, user.name)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _configuration["JwtSettings:Audience"],
                Issuer = _configuration["JwtSettings:Issuer"],
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TimeLifeMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }


    }
}
