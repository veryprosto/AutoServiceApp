using AutoServiceApp.IntegrationTests;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

public class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        // Принудительно выставляем заголовок авторизации по нашей тестовой схеме
        //_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme");
    }


    // Тест на получение списка
    [Fact]
    public async Task GetCustomers_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/customers");

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Fail($"GET /api/customers упал с кодом {response.StatusCode}. Ошибка: {errorContent}");
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Тест на создание клиента
    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsSuccessStatusCode()
    {
        var newCustomer = new
        {
            Name = "Михаил",
            Email = "mihail@example.com",
            Phone = "+79991112233"
        };

        var response = await _client.PostAsJsonAsync("/api/customers", newCustomer);

        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Fail($"POST /api/customers упал с кодом {response.StatusCode}. Ошибка: {errorContent}");
        }
        // Проверяем, что сервер успешно принял данные, может вернуть как OK так и Created
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);
    }

    // Тест на валидацию плохих данных
    [Fact]
    public async Task CreateCustomer_WithInvalidData_ReturnsBadRequest()
    {
        var invalidCustomer = new
        {
            Name = "" // Проверка валидатора пустым именем.
        };

        var response = await _client.PostAsJsonAsync("/api/customers", invalidCustomer);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}