using AutoServiceApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp.Data;

public class AppDbContext : IdentityDbContext //DbContext заменили в рамках перехода на Microsoft Identity
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Car> Cars { get; set; }
    //public DbSet<Employee> Employees { get; set; } ВАЖНО: Нам больше не нужен свой собственный DbSet<Employee>!
    // Вместо нашего класса Employee мы теперь будем использовать встроенный IdentityUser
    // (или позже можно расширить его, если понадобятся специфичные поля вроде "Телефон, Снилс").
    public DbSet<WorkOrder> WorkOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)// переопределенный метод IdentityDbContext
    {
        base.OnModelCreating(modelBuilder); // ЭТА СТРОКА ОБЯЗАТЕЛЬНА ДЛЯ IDENTITY!
    }
}