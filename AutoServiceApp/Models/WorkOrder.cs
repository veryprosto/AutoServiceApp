using Microsoft.AspNetCore.Identity;

namespace AutoServiceApp.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int CarId { get; set; }
    public Car? Car { get; set; }

    //для настройки связи «один ко многим» (у одного заказа один мастер) принято указывать оба поля: и голый ID (строку), и сам объект пользователя [INDEX].
    public string EmployeeId { get; set; } = string.Empty;// Голый ID для быстрой валидации
    public IdentityUser? Employee { get; set; } // Навигационное свойство для подгрузки данных мастера через .Include()

    public string Description { get; set; } = string.Empty;

    public decimal LaborCost { get; set; } // Стоимость работ
    public decimal PartsCost { get; set; } // Стоимость запчастей
    public decimal TotalCost => LaborCost + PartsCost;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletionDate { get; set; } //дата-время завершения работы
    public OrderStatus Status { get; set; } = OrderStatus.New;
}