using AutoServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using AutoServiceApp.Data;
using Microsoft.EntityFrameworkCore;
using MediatR;
using AutoServiceApp.Features.Customers;
using Microsoft.AspNetCore.Authorization;

namespace AutoServiceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAll()
    {
        var customers = await _mediator.Send(new GetCustomersQuery());
        return Ok(customers);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _mediator.Send(new GetCustomerByIdQuery(id));

        if (customer == null) return NotFound();
        return Ok(customer);
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(id));

        if (!result) return NotFound();
        return NoContent(); //204
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Customer>> Update(int id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID в адресе и в теле запроса должны совпадать");
        }

        var updatedCustomer = await _mediator.Send(command);

        if (updatedCustomer == null) return NotFound("Клиент не найден");

        return Ok(updatedCustomer);
    }
}