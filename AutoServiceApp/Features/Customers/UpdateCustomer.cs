using MediatR;
using AutoServiceApp.Data;
using AutoServiceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Features.Customers;

public record UpdateCustomerCommand(int Id, string Name, string Phone) : IRequest<Customer?>; // возвращает обновленного клиента (или null, если не найден) поэтому вопрос

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Customer?>//т.к. можем вернуть null? не забываем про вопрос
{
    private readonly AppDbContext _context;

    public UpdateCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)//т.к. можем вернуть null? не забываем про вопрос
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return null;

        customer.Name = request.Name;
        customer.Phone = request.Phone;

        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }
}