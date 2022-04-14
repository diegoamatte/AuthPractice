using AuthPractice.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccesor;

        public AuthController(IConfiguration configuration, UserManager<User> userManager, IHttpContextAccessor httpContextAccesor)
        {
            _configuration = configuration;
            _userManager = userManager;
            _httpContextAccesor = httpContextAccesor;
        }

        [HttpPost("/[controller]/login")]
        public async Task<IActionResult> LoginAsync([FromBody]AuthRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if(user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Forbid();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}")
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(
                issuer : _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            return Ok(new { AccessToken = jwt});
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("/[controller]/me")]
        public IActionResult GetUserInfo()
        {
            var user = _httpContextAccesor.HttpContext.User;

            return Ok(new
            {
                Claims = user.Claims.Select(s => new
                {
                    s.Type,
                    s.Value
                }).ToList(),
                user.Identity.Name,
                user.Identity.IsAuthenticated,
                user.Identity.AuthenticationType
            });
        }

        [HttpPost("/[controller]/register")]
        public async Task<IActionResult> Register([FromBody]RegisterRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Username
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new
                {
                    Reason = result.Errors
                }) ;
            return Ok();
        }
    }
}
