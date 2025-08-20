// Services/BackendSyncService.cs (Crie esta pasta e arquivo na sua API)
using ArenaVirtualAPI.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json; // Para desserialização
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        // Injete aqui os serviços/repositórios específicos para cada modelo
        private readonly UsuarioService _usuarioService; // Exemplo: seu serviço de usuários no backend
        private readonly CampeonatoService _campeonatoService; // Exemplo: seu serviço de campeonatos no backend
        // ... adicione serviços para todos os seus tipos sincronizáveis

        public BackendSyncService(ILogger<BackendSyncService> logger,
                                   UsuarioService usuarioService,
                                   CampeonatoService campeonatoService) // Injete os serviços aqui
        {
            _logger = logger;
            _usuarioService = usuarioService;
            _campeonatoService = campeonatoService;
            // ... atribua os outros serviços
        }

        // Método para processar o upload de dados
        public async Task ProcessUploadAsync(JsonElement data, string modelTypeName) {
            try {
                // Usa o tipo para desserializar corretamente
                switch (modelTypeName) {
                    case "Usuario":
                        var usuarios = JsonSerializer.Deserialize<List<Usuario>>(data.GetRawText());
                        if (usuarios != null) {
                            foreach (var user in usuarios) {
                                // Lógica para inserir ou atualizar o usuário no DB do backend
                                var existingUser = await _usuarioService.GetByIdAsync(user.Id); // Assuma que existe um método GetByIdAsync
                                if (existingUser == null) {
                                    await _usuarioService.AddAsync(user);
                                } else {
                                    await _usuarioService.UpdateAsync(user);
                                }
                            }
                            _logger.LogInformation($"Sincronizados {usuarios.Count} usuários.");
                        }
                        break;
                    case "Campeonato":
                        var campeonatos = JsonSerializer.Deserialize<List<Campeonato>>(data.GetRawText());
                        if (campeonatos != null) {
                            foreach (var camp in campeonatos) {
                                var existingCamp = await _campeonatoService.GetByIdAsync(camp.Id);
                                if (existingCamp == null) {
                                    await _campeonatoService.AddAsync(camp);
                                } else {
                                    await _campeonatoService.UpdateAsync(camp);
                                }
                            }
                            _logger.LogInformation($"Sincronizados {campeonatos.Count} campeonatos.");
                        }
                        break;
                    // Adicione cases para todos os seus tipos sincronizáveis
                    // case "Time": ...
                    // case "Partida": ...
                    default:
                        _logger.LogWarning($"Tipo de modelo desconhecido para sincronização de upload: {modelTypeName}");
                        break;
                }
            } catch (JsonException ex) {
                _logger.LogError($"Erro de desserialização para o tipo {modelTypeName}: {ex.Message}");
                throw; // Re-lança para que o controlador possa retornar um erro 400
            } catch (Exception ex) {
                _logger.LogError($"Erro ao processar upload para o tipo {modelTypeName}: {ex.Message}");
                throw;
            }
        }

        // Método para obter as atualizações de dados
        public async Task<List<ISyncable>> GetUpdatesAsync(DateTime lastSyncTime) {
            var updatedItems = new List<ISyncable>();

            // Exemplo: Obter usuários e campeonatos atualizados
            updatedItems.AddRange(await _usuarioService.GetUpdatedSinceAsync(lastSyncTime));
            updatedItems.AddRange(await _campeonatoService.GetUpdatedSinceAsync(lastSyncTime));
            // ... adicione chamadas para obter atualizações de todos os seus serviços

            return updatedItems;
        }
    }

    // Exemplo de serviço de usuário de backend (adapte para seu DB real)
    public class UsuarioService {
        // Exemplo simples, use seu contexto de DB real (Entity Framework, Dapper, etc.)
        private readonly List<Usuario> _usuarios = new List<Usuario>(); // Simula um DB
        public UsuarioService() {
            // Exemplo de dados
            _usuarios.Add(new Usuario { Id = 1, Nome = "Test User", Email = "test@example.com", SenhaHash = "hash1", UpdatedAt = DateTime.UtcNow.AddHours(-2), IsSynced = true });
        }
        public Task<Usuario?> GetByIdAsync(int id) => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));
        public Task AddAsync(Usuario user) {
            user.IsSynced = true; // No backend, ele está sincronizado
            user.UpdatedAt = DateTime.UtcNow;
            _usuarios.Add(user);
            return Task.CompletedTask;
        }
        public Task UpdateAsync(Usuario user) {
            var existing = _usuarios.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null) {
                existing.Nome = user.Nome;
                existing.Email = user.Email;
                existing.SenhaHash = user.SenhaHash;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsSynced = true;
            }
            return Task.CompletedTask;
        }
        public Task<IEnumerable<Usuario>> GetUpdatedSinceAsync(DateTime lastSyncTime) =>
            Task.FromResult<IEnumerable<Usuario>>(_usuarios.Where(u => u.UpdatedAt > lastSyncTime).ToList());
    }

    // Exemplo de serviço de campeonato de backend
    public class CampeonatoService {
        private readonly List<Campeonato> _campeonatos = new List<Campeonato>(); // Simula um DB
        public CampeonatoService() {
            // Exemplo de dados
            _campeonatos.Add(new Campeonato { Id = 101, Nome = "Copa MAUI", UpdatedAt = DateTime.UtcNow.AddHours(-5), IsSynced = true });
        }
        public Task<Campeonato?> GetByIdAsync(int id) => Task.FromResult(_campeonatos.FirstOrDefault(c => c.Id == id));
        public Task AddAsync(Campeonato camp) {
            camp.IsSynced = true;
            camp.UpdatedAt = DateTime.UtcNow;
            _campeonatos.Add(camp);
            return Task.CompletedTask;
        }
        public Task UpdateAsync(Campeonato camp) {
            var existing = _campeonatos.FirstOrDefault(c => c.Id == camp.Id);
            if (existing != null) {
                existing.Nome = camp.Nome; // Exemplo de atualização
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsSynced = true;
            }
            return Task.CompletedTask;
        }
        public Task<IEnumerable<Campeonato>> GetUpdatedSinceAsync(DateTime lastSyncTime) =>
            Task.FromResult<IEnumerable<Campeonato>>(_campeonatos.Where(c => c.UpdatedAt > lastSyncTime).ToList());
    }
}