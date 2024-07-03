using Configuration;
using Controllers.Dtos;
using Entities;
using Enums;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;
using Security;
using Services;

namespace Controllers;

public sealed class UserController(
    UserRepository userRepository,
    SessionRepository sessionRepository,
    PasswordHasher passwordHasher
) : BaseController
{
    private readonly UserRepository _userRepository = userRepository;
    private readonly SessionRepository _sessionRepository = sessionRepository;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

    [HttpPost("profile")]
    [SessionAuth]
    public async Task<IActionResult> Profile(SessionId? sessionId)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (sessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync(sessionId.Value);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        User? user = await _userRepository.FindByIdAsync(session.UserId);
        if (user is null)
        {
            return BadRequest(new { message = "User not found" });
        }

        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.SocialSecurityNumber,
            gender = user.Gender.ToString(),
            bloodType = user.BloodType.ToString(),
            user.City,
            user.District,
            user.DonationDescription,
            dateOfBirth = user.DateOfBirth.ToShortDateString(),
            user.PhoneNumber,
        });
    }

    [HttpPost("update-profile")]
    [SessionAuth]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (request.SessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync((Guid)request.SessionId);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        User? user = await _userRepository.FindByIdAsync(session.UserId);
        if (user is null)
        {
            return BadRequest(new { message = "User not found" });
        }

        if (request.FirstName is not null)
        {
            user.FirstName = request.FirstName;
        }

        if (request.LastName is not null)
        {
            user.LastName = request.LastName;
        }

        if (request.Email is not null)
        {
            user.Email = request.Email;
        }

        if (request.Password is not null)
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        if (request.SocialSecurityNumber is not null)
        {
            user.SocialSecurityNumber = request.SocialSecurityNumber;
        }

        if (request.Gender is not null)
        {
            if (!Enum.TryParse(request.Gender, true, out Gender gender))
            {
                ModelState.AddModelError(nameof(request.Gender), "Gender can only be male or female.");
                return ValidationProblem(detail: "Invalid gender", modelStateDictionary: ModelState);
            }

            user.Gender = gender;
        }

        if (request.BloodType is not null)
        {
            if (!Enum.TryParse(request.BloodType, true, out BloodType bloodType))
            {
                ModelState.AddModelError(nameof(request.BloodType), "Blood type can only be onegative, opositive, anegative, apositive, bnegative, bpositive, abnegative, abpositive.");
                return ValidationProblem(detail: "Invalid blood type", modelStateDictionary: ModelState);
            }

            user.BloodType = bloodType;
        }

        if (request.City is not null)
        {
            user.City = request.City;
        }

        if (request.District is not null)
        {
            user.District = request.District;
        }

        if (request.DonationDescription is not null)
        {
            user.DonationDescription = request.DonationDescription;
        }

        if (request.DateOfBirth is not null)
        {
            if (!DateTime.TryParse(request.DateOfBirth, out DateTime dateOfBirth))
            {
                ModelState.AddModelError(nameof(request.DateOfBirth), "Date can be in the format of yyyy-MM-dd.");
                return ValidationProblem(detail: "Invalid date of birth", modelStateDictionary: ModelState);
            }

            if (dateOfBirth.Kind != DateTimeKind.Utc)
            {
                dateOfBirth = dateOfBirth.ToUniversalTime();
            }

            user.DateOfBirth = dateOfBirth;
        }

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "Profile updated successfully" });
    }

    [HttpPost("get-users-by-blood-type")]
    [SessionAuth]
    public async Task<IActionResult> GetUsersByBloodType([FromQuery] string bloodType, [FromBody] SessionId? sessionId)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (sessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync(sessionId.Value);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        if (!Enum.TryParse(bloodType, true, out BloodType bloodTypeEnum))
        {
            ModelState.AddModelError(nameof(bloodType), "Blood type can only be onegative, opositive, anegative, apositive, bnegative, bpositive, abnegative, abpositive.");
            return ValidationProblem(detail: "Invalid blood type", modelStateDictionary: ModelState);
        }

        List<User> users = await _userRepository.FindByBloodTypeAsync(bloodTypeEnum);
        return Ok(users.Select(user => new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.City,
            user.District,
            user.DonationDescription,
            user.PhoneNumber
        }));
    }

    [HttpPost("get-users-by-city")]
    [SessionAuth]
    public async Task<IActionResult> GetUsersByCity([FromQuery] string city, [FromBody] SessionId? sessionId)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (sessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync(sessionId.Value);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        List<User> users = await _userRepository.FindByCityAsync(city);
        return Ok(users.Select(user => new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            BloodType = user.BloodType.ToString()
        }));
    }

    [HttpPost("get-users-by-district")]
    [SessionAuth]
    public async Task<IActionResult> GetUsersByDistrict([FromQuery] string district, [FromBody] SessionId? sessionId)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (sessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync(sessionId.Value);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        List<User> users = await _userRepository.FindByDistrictAsync(district);
        return Ok(users.Select(user => new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            BloodType = user.BloodType.ToString()
        }));
    }

    [HttpPost("get-donors")]
    [SessionAuth]
    public async Task<IActionResult> GetDonors([FromBody] SessionId? sessionId)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (sessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync(sessionId.Value);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        List<User> users = await _userRepository.GetDonorsAsync();
        return Ok(users.Select(user => new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.City,
            user.District,
            user.DonationDescription,
            BloodType = user.BloodType.ToString()
        }));
    }

    [HttpPost("get-beneficiaries")]
    [SessionAuth]
    public async Task<IActionResult> GetBeneficiaries([FromBody] SessionId? sessionId)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (sessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync(sessionId.Value);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        List<User> users = await _userRepository.GetBeneficiariesAsync();
        return Ok(users.Select(user => new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.City,
            user.District,
            user.DonationDescription,
            BloodType = user.BloodType.ToString()
        }));
    }

    [HttpPost("set-donation-description")]
    [SessionAuth]
    public async Task<IActionResult> SetDonationDescription([FromBody] SetDonationDescriptionRequest request)
    {
        Session? session = HttpContext.Items[Configurations.SessionItemsKey] as Session;
        if (session is null)
        {
            if (request.SessionId is null)
            {
                return BadRequest(new { message = "Session not found" });
            }

            session = await _sessionRepository.GetSessionByIdAsync((Guid)request.SessionId);
            if (session is null)
            {
                return BadRequest(new { message = "Session not found" });
            }
        }

        User? user = await _userRepository.FindByIdAsync(session.UserId);
        if (user is null)
        {
            return BadRequest(new { message = "User not found" });
        }

        user.DonationDescription = request.DonationDescription;
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "Donation description updated successfully" });
    }
}
