using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Auth.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        [Route("createtoken")]
        public async Task<IActionResult> CreateToken([FromBody] AuthViewModel authViewModel)
        {
            if (ModelState.IsValid)
            {
                //var user = await HttpContext.AuthenticateAsync("MainCookie");
                //// var result = await _signinManager.CheckPasswordSignInAsync(user, authViewModel.Password, false);
                //if (user.Succeeded)
                //{
                    // get the informaton need from the cookie from the SSO server and build out what we need
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, authViewModel.UserName),
                        // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.UniqueName, authViewModel.UserName),
                        // new Claim("PrimaryAccount", user.PrimaryAccount.AccountCode)
                        new Claim("user_roles", "user,account_admin")
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(_configuration["Tokens:Issuer"], _configuration["Tokens:Audience"], claims,
                        expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: creds);

                    var results = new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expires = token.ValidTo
                    };

                    return Created("", results);
                // }
            }
            return BadRequest();
        }
    }
    public class AuthViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
