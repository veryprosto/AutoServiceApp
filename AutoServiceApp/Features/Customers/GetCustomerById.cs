using AutoServiceApp.Data;
using AutoServiceApp.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Features.Customers;

public record GetCustomerByIdQuery(int Id) : IRequest<Customer?>;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Customer?>
{
    private readonly AppDbContext _context;

    public GetCustomerByIdHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
    }
}