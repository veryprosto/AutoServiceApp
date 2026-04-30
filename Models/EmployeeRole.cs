namespace AutoServiceApp.Models;

public enum EmployeeRole
{
    Owner,      // Владелец: видит всё, включая доходы
    Manager,    // Менеджер: управляет записями и клиентами
    Master,     // Мастер: назначает задачи механикам
    Mechanic    // Механик: видит только свои задачи
}