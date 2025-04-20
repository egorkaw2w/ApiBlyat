using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ChillAndDrillApI.Model;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// ������������ ChillAndDrillContext
builder.Services.AddDbContext<ChillAndDrillContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� ������� ������������
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ������������ Swagger � ������������ ��������� ����
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChillAndDrill API",
        Version = "v1",
        Description = "API ��� ChillAndDrill"
    });

    // ������ �������: ���������� ������ ��� ���� ��� schemaId
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
