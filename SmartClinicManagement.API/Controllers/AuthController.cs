using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Authentication;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto);

        return Ok(result);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] int userId)
    {
        await _authService.LogoutAsync(userId);

        return Ok(new
        {
            Message = "Logout Successful."
        });
    }
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var result = await _authService.ChangePasswordAsync(dto);

        if (!result)
        {
            return BadRequest(new
            {
                Message = "Current password is incorrect or passwords do not match."
            });
        }

        return Ok(new
        {
            Message = "Password changed successfully."
        });
    }
}