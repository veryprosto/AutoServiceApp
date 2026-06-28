using AutoServiceApp.Data;
using AutoServiceApp.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Features.Customers;

public record GetCustomersQuery() : IRequest<List<Customer>>;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, List<Customer>>
{
    private readonly AppDbContext _context;

    public GetCustomersHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers.ToListAsync(cancellationToken);
    }
}