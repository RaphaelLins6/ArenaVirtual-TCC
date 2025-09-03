using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.OpenApi.Models; // Adiciona a diretiva using para OpenApi

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner
builder.Services.AddControllers();

// Adiciona o DbContext com SQL Server, usando a string de conexão correta
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// Este é o código para registrar o gerador de Swagger com a documentação da API.
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArenaVirtualAPI", Version = "v1" });
});

// ==========================================================
// REGISTRO DOS SERVIÇOS DE SINCRONIZAÇÃO
// ==========================================================
// Registra os serviços específicos
builder.Services.AddScoped<IBackendService<Usuario, UsuarioSyncDto>, UsuarioService>();
builder.Services.AddScoped<IBackendService<Campeonato, CampeonatoSyncDto>, CampeonatoService>();
builder.Services.AddScoped<IBackendService<Time, TimeSyncDto>, TimeService>();
builder.Services.AddScoped<IBackendService<Convite, ConviteSyncDto>, ConviteService>();

// Registra a fábrica de serviços de sincronização
builder.Services.AddScoped<IBackendSyncServiceFactory, BackendSyncServiceFactory>();

// Registra o serviço principal de sincronização
builder.Services.AddScoped<BackendSyncService>();

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment()) {
    // Habilita o middleware para servir o Swagger gerado como um endpoint JSON.
    app.UseSwagger();

    // Habilita o middleware para servir a página da UI do Swagger.
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArenaVirtualAPI v1");
    });
} else {
    // Para ambientes de produção, redirecione para HTTPS para garantir segurança.
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
