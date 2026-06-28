using AutoServiceApp.Data;
using AutoServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Car>> GetById(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();
        return Ok(car);
    }

    [HttpGet("by-owner/{ownerId}")]
    public async Task<ActionResult<IEnumerable<Car>>> GetByOwner(int ownerId)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == ownerId);

        if (!customerExists)
        {
            return NotFound($"Клиент с ID {ownerId} не найден в системе");
        }

        var ownerCars = await _context.Cars
            .Where(c => c.OwnerId == ownerId)
            .ToListAsync();

        return Ok(ownerCars); //может вернуть пустой список.
    }

    [HttpGet("search/{plate}")]
    public async Task<ActionResult<Car>> GetByPlate(string plate)
    {
        var car = await _context.Cars
            .FirstOrDefaultAsync(c => c.LicensePlate.ToUpper() == plate.ToUpper());

        if (car == null) return NotFound($"Машина с номером {plate} не найдена");

        return Ok(car);
    }

    [HttpGet("search-by-vin/{vin}")]
    public async Task<ActionResult<Car>> GetByVin(string vin)
    {
        var searchVin = vin.ToUpper();

        var car = await _context.Cars.FirstOrDefaultAsync(c => c.Vin.ToUpper() == searchVin);

        if (car == null) return NotFound($"Машина с VIN {vin} не найдена");

        return Ok(car);
    }


    [HttpPost]
    public async Task<ActionResult<Car>> Create(Car newCar)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == newCar.OwnerId)) return BadRequest("Указанный владелец не существует");

        _context.Cars.Add(newCar);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newCar.Id }, newCar);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Car car)
    {
        if (id != car.Id)
        {
            return BadRequest("ID не совпадает");
        }

        _context.Entry(car).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Cars.AnyAsync(c => c.Id == id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();

        return NoContent();
    }






    //TODO  Добавить поиск машины по VIN
}