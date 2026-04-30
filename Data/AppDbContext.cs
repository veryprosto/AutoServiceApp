using AutoServiceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Data; // Добавьте эту строку!

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
}