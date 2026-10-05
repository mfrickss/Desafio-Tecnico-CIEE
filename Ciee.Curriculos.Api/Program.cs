using System;
using System.Text.Json.Serialization;
using Ciee.Curriculos.Api.Common;
using Ciee.Curriculos.Api.Common.Converters;
using Ciee.Curriculos.Api.Data;
using Ciee.Curriculos.Api.DTOs;
using Ciee.Curriculos.Api.Services;
using Ciee.Curriculos.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configuracao do DbContext com SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Injecao de Dependencia dos Servicos e Validadores (DIP estrito via interfaces)
builder.Services.AddScoped<IPdfExtractionService, PdfExtractionService>();
builder.Services.AddScoped<IValidator<CriarCandidatoDto>, CriarCandidatoDtoValidator>();

// Suporte a RFC 7807 (ProblemDetails) e interceptador global de excecoes
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Controllers e endpoints com serializacao canonica ISO 8601 UTC
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new Iso8601UtcDateTimeJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CIEE - API de Cadastro e Triagem de Currículos",
        Version = "v1",
        Description = "API RESTful para cadastro manual e triagem automática de currículos em PDF."
    });
});

// CORS para integracao com o Frontend Angular
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Tratamento de excecoes nao tratadas via ProblemDetails (RFC 7807 / RFC 9457)
app.UseExceptionHandler();
app.UseStatusCodePages();

// Executa migrations automaticamente em tempo de inicializacao
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.IsSqlServer())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations do Entity Framework Core.");
    }
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CIEE Currículos API v1");
        c.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

app.Run();
