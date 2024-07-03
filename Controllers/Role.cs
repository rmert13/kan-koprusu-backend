using Configuration;
using Controllers.Dtos;
using Entities;
using Enums;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;
using Security;

namespace Controllers;

public sealed class RoleController(
    UserRepository userRepository,
    SessionRepository sessionRepository
) : BaseController
{
    private readonly UserRepository _userRepository = userRepository;
    private readonly SessionRepository _sessionRepository = sessionRepository;

    [HttpPost("become-donor")]
    [SessionAuth]
    public async Task<IActionResult> BecomeDonor([FromBody] SessionId? sessionId)
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

        user.Roles |= (1 << (int)Role.Donor);
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "User is now a donor" });
    }

    [HttpPost("become-beneficiary")]
    [SessionAuth]
    public async Task<IActionResult> BecomeBeneficiary([FromBody] SessionId? sessionId)
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

        user.Roles |= (1 << (int)Role.Beneficiary);
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "User is now a beneficiary" });
    }

    [HttpPost("drop-donor")]
    [SessionAuth]
    public async Task<IActionResult> DropDonor([FromBody] SessionId? sessionId)
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

        user.Roles &= ~(uint)(1 << (int)Role.Donor);
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "User is no longer a donor" });
    }

    [HttpPost("drop-beneficiary")]
    [SessionAuth]
    public async Task<IActionResult> DropBeneficiary([FromBody] SessionId? sessionId)
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

        user.Roles &= ~(uint)(1 << (int)Role.Beneficiary);
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "User is no longer a beneficiary" });
    }
}
