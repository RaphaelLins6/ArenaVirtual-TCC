using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Controllers;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner
builder.Services.AddControllers();

// Configura Entity Framework Core com SQLite (ajuste se usar outro banco)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registre os serviços de autenticação e autorização
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Registre os serviços personalizados aqui
builder.Services.AddScoped<BackendSyncService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CampeonatoService>();
// Adicione aqui os outros serviços que a sua API utiliza

// Adiciona Swagger para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adicione UseAuthentication antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
