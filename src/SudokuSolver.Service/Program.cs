using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using System.Reflection;
using SudokuSolver.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddSingleton(builder.Configuration)
    .AddScoped<ISudokuSolverService, SudokuSolverService>()
    .AddEndpointsApiExplorer()
    .AddHttpContextAccessor()
    .AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger()
   .UseSwaggerUI();

app.UseHttpsRedirection()
   .UseAuthorization();

app.MapControllers();

app.Run();
