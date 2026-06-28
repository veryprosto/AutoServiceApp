using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AutoServiceApp.Data;
using AutoServiceApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace AutoServiceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    // Внедряем UserManager и RoleManager вместо AppDbContext
    public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        // Проверяем, существует ли уже такой пользователь
        var userExists = await _userManager.FindByNameAsync(model.Username);

        if (userExists != null) return BadRequest("Пользователь с таким логином уже существует");

        // Создаем объект IdentityUser (наш новый сотрудник)
        IdentityUser user = new()
        {
            UserName = model.Username,
            SecurityStamp = Guid.NewGuid().ToString() // Нужен для безопасности сессий
        };

        // Создаем пользователя в базе. Identity САМА захэширует пароль!
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors); // Если пароль слишком простой (например, нет цифр), Identity вернет ошибку
        }

        // Проверяем, существует ли роль, если нет — создаем её
        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        // Привязываем роль к пользователю
        await _userManager.AddToRoleAsync(user, model.Role);

        return Ok("Сотрудник успешно зарегистрирован");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);

        // Проверяем пароль с помощью встроенного метода
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            // Формируем клэймы (паспортные данные токена)
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // Генерируем JWT токен
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(token);

            return Ok(new
            {
                token = tokenHandler.WriteToken(createdToken),
                userId = user.Id // добавил возврат id пользователя чтобы фронт(login.html) его запомнил, нужно при проверке роли/доступа к разным функциям
            });
        }

        return Unauthorized("Неверный логин или пароль");
    }
}

public record RegisterDto(string Username, string Password, string Role);
public record LoginDto(string Username, string Password);