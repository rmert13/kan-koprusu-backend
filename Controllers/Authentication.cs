using Configuration;
using Controllers.Dtos;
using Controllers.Dtos.Login;
using Controllers.Dtos.Register;
using Entities;
using Enums;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;
using Services;

namespace Controllers;

public sealed class AuthenticationController(
    UserRepository userRepository,
    PasswordHasher passwordHasher,
    SessionRepository sessionRepository
) : BaseController
{
    private readonly UserRepository _userRepository = userRepository;
    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly SessionRepository _sessionRepository = sessionRepository;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        if (!Enum.TryParse(registerRequest.Gender, true, out Gender gender))
        {
            ModelState.AddModelError(nameof(registerRequest.Gender), "Gender can only be male or female.");
            return ValidationProblem(detail: "Invalid gender", modelStateDictionary: ModelState);
        }

        if (!Enum.TryParse(registerRequest.BloodType, true, out BloodType bloodType))
        {
            ModelState.AddModelError(nameof(registerRequest.BloodType), "Blood type can only be onegative, opositive, anegative, apositive, bnegative, bpositive, abnegative, abpositive.");
            return ValidationProblem(detail: "Invalid blood type", modelStateDictionary: ModelState);
        }

        User? existingUser = await _userRepository.FindByEmailAsync(registerRequest.Email);
        if (existingUser is not null)
        {
            return Conflict(new { message = "User already exists" });
        }


        if (!DateTime.TryParse(registerRequest.DateOfBirth, out DateTime dateOfBirth))
        {
            ModelState.AddModelError(nameof(registerRequest.DateOfBirth), "Date can be in the format of yyyy-MM-dd.");
            return ValidationProblem(detail: "Invalid date of birth", modelStateDictionary: ModelState);
        }

        if (dateOfBirth.Kind != DateTimeKind.Utc)
        {
            dateOfBirth = dateOfBirth.ToUniversalTime();
        }

        uint role = 1 << (int)Role.Basic;
        User user = new()
        {
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            Email = registerRequest.Email,
            PasswordHash = _passwordHasher.HashPassword(registerRequest.Password),
            SocialSecurityNumber = registerRequest.SocialSecurityNumber,
            Gender = gender,
            BloodType = bloodType,
            City = registerRequest.City,
            District = registerRequest.District,
            Roles = role,
            DateOfBirth = dateOfBirth,
            PhoneNumber = registerRequest.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return Ok(new { message = "User registered" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        User? user = await _userRepository.FindByEmailAsync(loginRequest.Email);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            return BadRequest(new { message = "Invalid password" });
        }

        // ---- user is authenticated ----

        Session newSession = new()
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = user.Roles.ToRoleList(),
            IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
        };

        Guid sessionId;
        Session? existingSession = await _sessionRepository.GetSessionByUserIdAsync(user.Id);
        if (existingSession is null)
        {
            sessionId = await _sessionRepository.SaveSessionAsync(newSession);
        }
        else
        {
            sessionId = existingSession.Id;
            newSession.CreatedAt = existingSession.CreatedAt;
            newSession.ExpireAt = existingSession.ExpireAt;
            await _sessionRepository.UpdateSessionByIdAsync(existingSession.Id, newSession);
        }

        Response.Cookies.Append(Configurations.SessionIdCookieKey, sessionId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true,
            Expires = DateTime.UtcNow + Configurations.SessionExpiry
        });

        return Ok(new
        {
            sessionId,
            user.Email,
            user.FirstName,
            user.LastName,
            Roles = user.Roles.ToRoleList().Select(role => role.ToString()),
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] SessionId? sessionId)
    {
        Guid? sessionIdGuid = null;

        Request.Cookies.TryGetValue(Configurations.SessionIdCookieKey, out string? sessionIdStr);

        if (Guid.TryParse(sessionIdStr, out Guid sessionGuid))
        {
            sessionIdGuid = sessionGuid;
        }

        if (sessionIdGuid is null && sessionId is not null)
        {
            sessionIdGuid = sessionId.Value;
        }

        if (sessionIdGuid is null)
        {
            return BadRequest(new { message = "Session not found" });
        }

        Session? session = await _sessionRepository.GetSessionByIdAsync((Guid)sessionIdGuid);
        if (session is null)
        {
            return BadRequest(new { message = "Session not found" });
        }

        await _sessionRepository.DeleteSessionByIdAsync(sessionGuid);

        Response.Cookies.Delete(Configurations.SessionIdCookieKey);

        return Ok(new { message = "User logged out" });
    }
}
