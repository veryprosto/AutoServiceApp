using AutoServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using AutoServiceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer newCustomer)
    {
        _context.Customers.Add(newCustomer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = newCustomer.Id }, newCustomer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

}