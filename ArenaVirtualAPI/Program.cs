using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner
builder.Services.AddControllers();

// Adiciona o DbContext com SQL Server, usando a string de conexão correta
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================================
// REGISTRO DOS SERVIÇOS DE SINCRONIZAÇÃO
// ==========================================================
// Registra os serviços específicos
builder.Services.AddScoped<IBackendService<Usuario>, UsuarioService>();
builder.Services.AddScoped<IBackendService<Campeonato>, CampeonatoService>();
builder.Services.AddScoped<IBackendService<Time>, TimeService>();
builder.Services.AddScoped<IBackendService<Convite>, ConviteService>();

// Registra a fábrica de serviços de sincronização
builder.Services.AddScoped<IBackendSyncServiceFactory, BackendSyncServiceFactory>();

// Registra o serviço principal de sincronização
builder.Services.AddScoped<BackendSyncService>();

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Inicializa o banco de dados
using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate();
}

app.Run();