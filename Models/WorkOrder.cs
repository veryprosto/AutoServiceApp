namespace AutoServiceApp.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int CarId { get; set; }
    public Car? Car { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal LaborCost { get; set; } // Стоимость работ
    public decimal PartsCost { get; set; } // Стоимость запчастей
    public decimal TotalCost => LaborCost + PartsCost;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletionDate { get; set; } //дата-время завершения работы
    public OrderStatus Status { get; set; } = OrderStatus.New;
}