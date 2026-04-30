using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceApp.Data;
using AutoServiceApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace AutoServiceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkOrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrder>>> GetAll()
    {
        return await _context.WorkOrders
        .Include(o => o.Customer) //Include достает объект
        .Include(o => o.Car)
        .Include(o => o.Employee)
        .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrder>> GetById(int id)
    {
        var order = await _context.WorkOrders
            .Include(o => o.Customer)
            .Include(o => o.Car)
            .Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.Id == id); // FindAsync не работает с Include

        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrder>> Create(WorkOrder order)
    {
        // Простая валидация: существуют ли связанные сущности
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == order.CustomerId);
        var carExists = await _context.Cars.AnyAsync(c => c.Id == order.CarId);
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == order.EmployeeId);

        if (!customerExists || !carExists || !employeeExists)
        {
            return BadRequest("Ошибка: Неверный ID клиента, машины или сотрудника.");
        }

        _context.WorkOrders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Owner, Master")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus newStatus)
    {
        var order = await _context.WorkOrders.FindAsync(id);

        if (order == null) return NotFound("Заказ не найден");

        order.Status = newStatus;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("history/{plate}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WorkOrder>>> GetHistoryByPlate(string plate)
    {
        var car = await _context.Cars.FirstOrDefaultAsync(c => c.LicensePlate.ToUpper() == plate.ToUpper());

        if (car == null)
        {
            return NotFound($"Машина с номером {plate} не найдена");
        }

        var history = await _context.WorkOrders
        .Include(o => o.Customer)  // Чтобы знать, кто владел машиной тогда, возможно владелец менялся без изменения номера
        .Include(o => o.Employee)
        .Where(o => o.CarId == car.Id)
        .OrderByDescending(o => o.CreatedAt) //сортировка
        .ToListAsync();

        return Ok(history);
    }

    [HttpGet("revenue")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> GetTotalRevenue() //TODO сделать отчет с полями - например месяц или период
    {
        var orders = await _context.WorkOrders.Where(o => o.Status == OrderStatus.Completed).ToListAsync();

        var totalLabor = orders.Sum(o => o.LaborCost);
        var totalParts = orders.Sum(o => o.PartsCost);

        return Ok(new
        {
            TotalRevenue = totalLabor + totalParts,
            LaborRevenue = totalLabor,
            PartsRevenue = totalParts,
            CompletedOrdersCount = orders.Count
        });

    }

    [HttpGet("top-employees")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> GetTopEmployees()
    {
        var topEmployees = await _context.WorkOrders
            .Where(o => o.Status == OrderStatus.Completed)
            .GroupBy(o => o.Employee.FullName) // Группируем по имени мастера
            .Select(g => new
            {
                EmployeeName = g.Key,
                TotalEarned = g.Sum(o => o.LaborCost), // Считаем только стоимость работ
                OrdersCount = g.Count()
            })
            .OrderByDescending(x => x.TotalEarned)
            .ToListAsync();

        return Ok(topEmployees);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Master,Manager")] // Ограничение ролей
    public async Task<IActionResult> DeleteWorkOrder(int id)
    {
        var order = await _context.WorkOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound("Заказ не найден");
        }

        _context.WorkOrders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent(); // Успешное удаление без возврата данных
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Master")]
    public async Task<IActionResult> UpdateWorkOrder(int id, WorkOrder order)
    {
        if (id != order.Id) return BadRequest("ID в адресе и в теле запроса не совпадают");

        var existingOrder = await _context.WorkOrders.AnyAsync(o => o.Id == id);
        if (!existingOrder) return NotFound("Заказ не найден");

        _context.Entry(order).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatusCode(500, "Ошибка при обновлении базы данных");
        }

        return NoContent();
    }
}