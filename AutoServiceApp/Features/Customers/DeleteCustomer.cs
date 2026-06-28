using MediatR;
using AutoServiceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Features.Customers;

// Команда принимает Id и возвращает bool (удалено или нет)
public record DeleteCustomerCommand(int Id) : IRequest<bool>;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly AppDbContext _context;

    public DeleteCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}