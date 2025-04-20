using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ChillAndDrillApI.Model;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Регистрируем ChillAndDrillContext
builder.Services.AddDbContext<ChillAndDrillContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем сервисы контроллеров
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Конфигурация Swagger с исправлением конфликта схем
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChillAndDrill API",
        Version = "v1",
        Description = "API для ChillAndDrill"
    });

    // Лучшее решение: Используем полное имя типа как schemaId
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

var app = builder.Build();

// Middleware pipeline
app.UseCors("AllowLocalhost3000");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChillAndDrill API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.UseDeveloperExceptionPage();
app.Run();
