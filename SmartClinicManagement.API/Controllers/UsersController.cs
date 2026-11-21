using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Users;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        AccountCreationResult result;

        try
        {
            result = await _userService.CreateAsync(dto);
        }
        catch (DuplicateEmailException ex)
        {
            // Clean, expected validation failure — never a raw exception.
            return BadRequest(new { Message = ex.Message });
        }

        // The user account itself is always created at this point — only
        // the email outcome varies, and we report it honestly instead of
        // always claiming "email sent".
        if (!result.Email.Success)
        {
            return Ok(new
            {
                UserID = result.UserId,
                Message = $"User created successfully, but the welcome email could not be sent: {result.Email.ErrorMessage}",
                EmailSent = false
            });
        }

        return Ok(new
        {
            UserID = result.UserId,
            Message = "User created successfully and the welcome email was sent.",
            EmailSent = true
        });
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        await _userService.UpdateAsync(id, dto);

        return Ok(new
        {
            Message = "User updated successfully."
        });
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteAsync(id);

        return Ok(new
        {
            Message = "User deleted successfully."
        });
    }

}
