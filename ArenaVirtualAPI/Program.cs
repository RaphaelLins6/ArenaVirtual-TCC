using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner
builder.Services.AddControllers();

// Adiciona o DbContext com SQL Server E RESILIÊNCIA DE CONEXÃO
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            // Habilita a Resiliência de Conexão para Azure SQL (Trata erros transientes como 40613)
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// CORREÇÃO: AddSwaggerGen está correto no ConfigureServices
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArenaVirtualAPI", Version = "v1" });
});

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
builder.Services.AddScoped<IBackendService<Jogo, JogoSyncDto>, JogoService>();

// Registro corrigido para UsuarioCampeonatoFavoritoService
builder.Services.AddScoped<UsuarioCampeonatoFavoritoService>();
builder.Services.AddScoped<IBackendService<UsuarioCampeonatoFavorito, UsuarioCampeonatoFavoritoSyncDto>>(sp =>
    sp.GetRequiredService<UsuarioCampeonatoFavoritoService>());

// Registro da Factory e do Serviço principal
builder.Services.AddScoped<IBackendSyncServiceFactory, BackendSyncServiceFactory>();
builder.Services.AddScoped<BackendSyncService>();

var app = builder.Build();

// CORREÇÃO: Movemos o Swagger para fora da condição IsDevelopment.
// Isso permite que o Visual Studio acesse o endpoint do Swagger JSON durante o deploy no Azure (Produção).
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArenaVirtualAPI v1");
});

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    // Mantemos a garantia de redirecionamento HTTPS no ambiente de Produção do Azure
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();