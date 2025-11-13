using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Security.Claims;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly QuizzlerDbContext _context;
    private readonly UserService _userService;

    public UserController(QuizzlerDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<User>> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await GetUserProfile(Convert.ToInt32(userId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("{id}/profile")]
    public async Task<ActionResult<User>> GetUserProfile(int id)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();
        var result = new User
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LastSeen = user.LastSeen,
            DateRegistered = user.DateRegistered,
        };
        return result;
    }

    [Authorize]
    [HttpPatch("update")]
    public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

        if (!await _userService.AreCredentialsCorrect(new LoginDto { Email = user.Email, Password = userUpdateDto.CurrentPassword })) 
            return StatusCode(400, $"Wrong credentials");

        if (userUpdateDto.Email is not null)
        {
            if (await _userService.EmailExists(userUpdateDto.Email) && !(userUpdateDto.Email == user.Email)) 
                return StatusCode(409, $"Email {userUpdateDto.Email} already registered");
        }
        if (userUpdateDto.Username is not null)
        {
            if (await _userService.UsernameExists(userUpdateDto.Username) && !(userUpdateDto.Username == user.Username)) 
                return StatusCode(409, $"Username {userUpdateDto.Username} already registered");
        }
        if (userUpdateDto.Email is not null)
        {
            if (!_userService.IsEmailCorrect(userUpdateDto.Email)) 
                return StatusCode(422, $"Email {userUpdateDto.Email} is not a proper email address");
        }
        if (userUpdateDto.Password is not null)
        {
            if (!_userService.IsPasswordGoodEnough(userUpdateDto.Password)) 
                return StatusCode(422, $"Password does not meet the requirements");
        }

        user.Username = userUpdateDto.Username ?? user.Username;
        user.Email = userUpdateDto.Email ?? user.Email;
        user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
        user.LastName = userUpdateDto.LastName ?? user.LastName;
        user.LoginInfo.PasswordHash = userUpdateDto.Password != null ? _userService.HashPassword(userUpdateDto.Password, user.LoginInfo.Salt) : user.LoginInfo.PasswordHash;
        user.Avatar = userUpdateDto.Avatar ?? user.Avatar;

        _context.SaveChanges();

        return Ok("Updated");
    }

    [Authorize]
    [HttpGet("check")]
    public async Task<ActionResult<User>> CheckAuth()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        user.LastSeen = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok("You are authorized!");
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
    {
        if (await _userService.EmailExists(userRegisterDto.Email))
        {
            return StatusCode(409, $"Email {userRegisterDto.Email} already registered");
        }
        if (await _userService.UsernameExists(userRegisterDto.Username))
        {
            return StatusCode(409, $"Username {userRegisterDto.Username} already registered");
        }

        if (!_userService.IsEmailCorrect(userRegisterDto.Email))
        {
            return StatusCode(422, $"Email {userRegisterDto.Email} is not a proper email address");
        }

        if (!_userService.IsPasswordGoodEnough(userRegisterDto.Password))
        {
            return StatusCode(422, $"Password does not meet the requirements");
        }

        var user = await _userService.CreateUser(userRegisterDto);
        _context.User.Add(user);

        await _context.SaveChangesAsync();

        return new CreatedAtActionResult(nameof(GetUserProfile), "User", new { id = user.UserId }, "Created user");

    }

    [HttpPost("login")]
    public async Task<ActionResult<User>> Login(LoginDto loginDto)
    {
        if (!await _userService.DoesExist(loginDto.Email))
        {
            return StatusCode(409, $"{loginDto.Email} is not registered");
        }

        if (await _userService.AreCredentialsCorrect(loginDto))
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            var token = _userService.GenerateJwtToken(user);
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(token);
        }
        else
        {
            return StatusCode(400, $"Wrong credentials");
        }
    }
    [Authorize]
    [HttpDelete("delete")]
    public async Task<ActionResult<User>> Delete(string userPassword)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        try
        {
            if (await _userService.AreCredentialsCorrect(new LoginDto { Email = user.Email, Password = userPassword }))
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                return Ok("User deleted successfully.");
            }
        }
        catch (Exception ex)
        {
            return NotFound();
        }

        return StatusCode(403, "Invalid credentials");
    }
}

