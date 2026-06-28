using MediatR;
using AutoServiceApp.Data;
using AutoServiceApp.Models;

namespace AutoServiceApp.Features.Customers;

public record CreateCustomerCommand(string Name, string Phone) : IRequest<Customer>; // Это "Команда" - просто набор данных, которые нужны для создания

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Customer> // Это "Обработчик" - здесь живет вся логика, которая раньше была в контроллере
{
    private readonly AppDbContext _context;

    public CreateCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer;

    }
}