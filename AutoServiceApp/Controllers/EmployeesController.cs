using AutoServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using AutoServiceApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AutoServiceApp.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class EmployeesController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public EmployeesController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDisplayDto>>> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var employeeList = new List<EmployeeDisplayDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            employeeList.Add(new EmployeeDisplayDto(
                user.Id,
                user.UserName ?? "Без логина", // это типа coalesce в SQL при чем можно ставить несколько ?? и выберется первое не null значение - user.UserName ?? user.UserNick ?? "Без логина",
                user.PhoneNumber ?? "Нет телефона",
                roles.FirstOrDefault() ?? "Без роли"
            ));
        }

        return Ok(employeeList);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDisplayDto>> GetById(string id) //Microsoft Identity все ид по умолчанию строки (GUID)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("Сотрудник не найден");

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new EmployeeDisplayDto(
            user.Id,
            user.UserName ?? "Без логина",
            user.PhoneNumber ?? "Нет телефона",
            roles.FirstOrDefault() ?? "Без роли"
        );

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("Сотрудник не найден");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEmployeeDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("Сотрудник не найден");

        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // Работаем с ролями (смена роли сотрудника)
        var currentRoles = await _userManager.GetRolesAsync(user); //у сотрудника только одна роль!!!
        var currentRole = currentRoles.FirstOrDefault();

        // Если роль из запроса отличается от текущей — перезаписываем её
        if (currentRole != dto.Role)
        {
            // Удаляем старую роль, если она была
            if (!string.IsNullOrEmpty(currentRole))
            {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            }

            // предполагается, что базовые роли Owner, Master, Mechanic уже созданы при регистрации), 
            // т.е. нельзя сменить роль на такую которой не было ещё, новую роль можно создать только при регистрации сотрудника

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);

            // 🔥 Если БД такой роли не знает, Succeeded будет равен false
            if (!roleResult.Succeeded)
            {
                // Возвращаем клиенту ошибку 400 Bad Request с описанием
                return BadRequest(new
                {
                    message = $"Не удалось назначить роль '{dto.Role}'. Возможно, она еще не создана в системе.",
                    errors = roleResult.Errors
                });
            }
        }
        return Ok(new { message = "Данные сотрудника успешно обновлены" });
    }
}

public record EmployeeDisplayDto(string Id, string Username, string PhoneNumber, string Role);

public record UpdateEmployeeDto(string PhoneNumber, string Role);

