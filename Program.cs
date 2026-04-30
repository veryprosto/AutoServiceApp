using AutoServiceApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;



var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); // Нужно для корректной работы Swagger с Minimal API
//builder.Services.AddSwaggerGen();  // Регистрируем генератор Swagger //это работает без авторизации, а ниже с авторизацией
builder.Services.AddSwaggerGen(options =>
{
    // Используем встроенные методы, которые не требуют сложных типов
    options.AddSecurityDefinition("Bearer", new()
    {
        Description = "Введите JWT токен: Bearer {ваш_токен}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddControllers().AddJsonOptions(options => //добавляем контроллеры, до .AddJsonOptions это базовое, все что после нужно для того чтобы енумы отображались словами
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=autoservice.db"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Генерирует сам JSON файл
    app.UseSwaggerUI(); // Включает визуальную страницу по адресу /swagger
}

app.UseHttpsRedirection();

app.UseDefaultFiles(); // Позволяет открывать index.html по умолчанию
app.UseStaticFiles();  // Разрешает отдавать файлы из папки wwwroot

app.UseCors("AllowAll"); // UseCors стоит перед UseAuthentication. Это критически важно!

app.UseAuthentication(); // КТО ты? (проверка токена)
app.UseAuthorization();  // ЧТО тебе можно? (проверка роли)

app.MapControllers();

app.Run();


