using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using API.FurnitureStore.Shared.Auth;
using API.FurnitureStore.Shared.DTOs;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace API.FurnitureStore.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtConfig _jwtConfig;

    public AuthenticationController(UserManager<IdentityUser> userManager,
                                    IOptions<JwtConfig> jwtConfig)
    {
        _userManager = userManager;
        _jwtConfig = jwtConfig.Value;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
    {
        if (ModelState.IsValid is false) return BadRequest();

        // Verify if the email is already taken
        var emailExistis = await _userManager.FindByEmailAsync(request.EmailAddress);

        // email already taken
        if (emailExistis is not null) return BadRequest(new AuthResult()
        {
            Result = false,
            Errors = new List<string>() { "Email already in use" }
        });

        // Create new user
        var user = new IdentityUser()
        {
            Email = request.EmailAddress,
            UserName = request.EmailAddress
        };

        var isCreated = await _userManager.CreateAsync(user);

        if (isCreated.Succeeded)
        {
            var token = GenerateToken(user);
            return Ok(new AuthResult()
            {
                Result = true,
                Token = token
            });
        }
        else
        {
            var error = new List<string>();
            foreach (var err in isCreated.Errors)
                error.Add(err.Description);

            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = error
            });
        }

        // Something went wrong
        return BadRequest(new AuthResult()
        {
            Result = false,
            Errors = new List<string>() { "User couldn´t be created" }
        });
    }

    private string GenerateToken(IdentityUser user)
    {
        var jwtHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
            {
                // Add claims for the user in the token
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()) // When the token was issued
            })),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtHandler.CreateToken(tokenDescriptor);

        return jwtHandler.WriteToken(token);    
    }
}
