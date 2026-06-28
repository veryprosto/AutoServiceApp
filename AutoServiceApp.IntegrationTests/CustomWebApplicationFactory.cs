using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using AutoServiceApp;

namespace AutoServiceApp.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Внедряем тестовую конфигурацию прямо в память приложения перед его стартом
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseInMemoryDatabase", "true" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Отключаем реальную аутентификацию (JWT)
            var authDescriptors = services.Where(d => d.ServiceType == typeof(IAuthenticationService) ||
                                                      d.ServiceType.Name.Contains("Authentication") ||
                                                      d.ServiceType.Name.Contains("JwtBearer")).ToList();
            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("TestScheme", options => { });
        });
    }
}
