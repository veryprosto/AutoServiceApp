using AutoServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using AutoServiceApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize] //только авторизованные пользователи могут пользоваться методами этого контроллера. Чтобы создать первого пользователя админа например изменить на [AllowAnonymous]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
    {
        var employees = await _context.Employees.AsNoTracking().ToListAsync();//AsNoTracking() ускоряет запрос, если данные для чтения.
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetById(int id)//добавил асинхронности(async Task)
    {
        var employee = await _context.Employees.FindAsync(id);//добавил асинхронности(await, FindAsync)

        if (employee == null) return NotFound();
        return Ok(employee);
    }

    // ActionResult (для GET и POST) IActionResult (для PUT и DELETE). 
    // Это потому, что в гет и пост возвращается как результат выполнения Ok, NotFound так и объект. 
    // А в методах пут и делит только статус результата (204 No Content, 404 Not Found и т.д.) без привязки к конкретному типу данных в теле ответа.
    [HttpPost]
    public async Task<ActionResult<Employee>> Create(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest("ID не совпадает");
        }

        _context.Entry(employee).State = EntityState.Modified;
        //этот метод запихивает объект в память и помечает его поля как измененные и форминует в памяти будущий update который сработает в SaveChangesAsync
        //Плюс этого метода - не нужно делать предварительный селект. Минус - меняются все поля объекта.

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Employees.Any(e => e.Id == id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


