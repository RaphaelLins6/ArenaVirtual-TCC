using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.OpenApi.Models;

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
// CONFIGURAÇÃO DO KESTREL PARA HABILITAR HTTP E HTTPS
// ==========================================================
builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(5067);
    options.ListenAnyIP(7117, listenOptions => {
        listenOptions.UseHttps();
    });
});

// ==========================================================
// REGISTRO DOS SERVIÇOS DE SINCRONIZAÇÃO
// ==========================================================
builder.Services.AddScoped<IBackendService<Usuario, UsuarioSyncDto>, UsuarioService>();
builder.Services.AddScoped<IBackendService<Campeonato, CampeonatoSyncDto>, CampeonatoService>();
builder.Services.AddScoped<IBackendService<Time, TimeSyncDto>, TimeService>();
builder.Services.AddScoped<IBackendService<Convite, ConviteSyncDto>, ConviteService>();
builder.Services.AddScoped<IBackendService<UsuarioCampeonatoFavorito, UsuarioCampeonatoFavoritoSyncDto>, UsuarioCampeonatoFavoritoService>();
builder.Services.AddScoped<IBackendSyncServiceFactory, BackendSyncServiceFactory>();

builder.Services.AddScoped<BackendSyncService>();

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment()) {
    // Habilita a página de exceção detalhada em ambiente de desenvolvimento
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArenaVirtualAPI v1");
    });
} else {
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();