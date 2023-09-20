using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

using API.FurnitureStore.Shared.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace API.FurnitureStore.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly JwtConfig _jwtConfig;

    public AuthenticationController(UserManager<IdentityUser> userManager,
                                    IOptions<JwtConfig> jwtConfig,
                                    IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _jwtConfig = jwtConfig.Value;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
    {
        // Validate the parameters of the request
        if (ModelState.IsValid is false) return BadRequest();

        // Check if the user exist
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is null)
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>() { "Invalid Credentials" }
            });

        if (existingUser.EmailConfirmed is false)
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>() { "Email needs to be confirmed" }
            });

        // Check the user and password are okey
        var checkUserAndPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);

        if (checkUserAndPassword is false)
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>() { "Invalid Credentials" }
            });

        // Generate token
        var token = GenerateToken(existingUser);

        return Ok(new AuthResult()
        {
            Result = true,
            Token = token
        });
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        // Validate userId and code
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>() { "Invalid email confirmation url" }
            });

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>() { "User not found" }
            });

        // User exist go on

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var result = await _userManager.ConfirmEmailAsync(user, code);

        var status = result.Succeeded ? "Email confirmed successfully" : "Error confirming your email";

        return Ok(status);
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
            UserName = request.EmailAddress,
            EmailConfirmed = false
        };

        var isCreated = await _userManager.CreateAsync(user, request.Password);

        if (isCreated.Succeeded)
        {
            await SendVerificationEmail(user);

            return Ok(new AuthResult()
            {
                Result = true
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
    }

    private async Task SendVerificationEmail(IdentityUser user)
    {
        var verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationCode));

        //                          server           controller     endpoint      query string = parameters
        // example link: https//localhost:8080/api/authentication/ConfirmEmail/userId=exampleUserId&code=exampleCode
        var callbackUrl = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail",
                                controller: "Authentication", new { userId = user.Id, code = verificationCode })}";

        var emailBody = $"Please configm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> clicking here! </a>";

        await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);
    }

    private string GenerateToken(IdentityUser user)
    {
        var jwtHandler = new JwtSecurityTokenHandler();

        // Get key
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
            Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime),

            // Sign the token
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtHandler.CreateToken(tokenDescriptor);

        // token to string
        return jwtHandler.WriteToken(token);
    }
}
