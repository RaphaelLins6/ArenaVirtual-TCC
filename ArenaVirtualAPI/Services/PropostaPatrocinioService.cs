using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    public class PropostaPatrocinioService : IBackendService<PropostaPatrocinio, PropostaPatrocinioSyncDto> {
        private readonly ApiDbContext _context;
        private readonly ILogger<PropostaPatrocinioService> _logger;

        public PropostaPatrocinioService(ApiDbContext context, ILogger<PropostaPatrocinioService> logger) {
            _context = context;
            _logger = logger;
        }

        public Task<PropostaPatrocinio?> GetByIdAsync(int id) => _context.PropostasPatrocinio.FindAsync(id).AsTask();

        public async Task AddAsync(PropostaPatrocinio item) {
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.PropostasPatrocinio.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PropostaPatrocinio item) {
            _context.Entry(item).State = EntityState.Modified;
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Fase 1: Upsert (Cria ou atualiza a entidade base)
        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<PropostaPatrocinioSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();

            foreach (var dto in dtos) {
                var existingItem = await _context.PropostasPatrocinio
                    .FirstOrDefaultAsync(p => p.ClientAppId == dto.ClientAppId);

                Action<PropostaPatrocinio> mapProperties = (entity) => {
                    entity.NomePatrocinador = dto.NomePatrocinador ?? string.Empty;
                    entity.ImagemPatrocinador = dto.ImagemPatrocinador ?? string.Empty;
                    entity.LinkPatrocinador = dto.LinkPatrocinador ?? string.Empty;
                    entity.ValorMonetario = dto.ValorMonetario;
                    entity.DataInicio = dto.DataInicio;
                    entity.DataFim = dto.DataFim;
                    entity.Mensagem = dto.Mensagem ?? string.Empty;
                    entity.Aprovada = dto.Aprovada;
                    entity.IsSynced = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                };

                if (existingItem == null) {
                    var newItem = new PropostaPatrocinio {
                        ClientAppId = dto.ClientAppId,
                        // PatrocinadorId e CampeonatoId serão resolvidos na Fase 2
                    };
                    mapProperties(newItem);
                    _context.PropostasPatrocinio.Add(newItem);
                    idMapping[newItem.ClientAppId] = newItem.Id;
                } else {
                    mapProperties(existingItem);
                    _context.Entry(existingItem).State = EntityState.Modified;
                    idMapping[existingItem.ClientAppId] = existingItem.Id;
                }
            }

            await _context.SaveChangesAsync();
            return idMapping;
        }

        // Fase 2: Atualização de Chaves Estrangeiras (FKs)
        public async Task UpdateForeignKeysAsync(IEnumerable<PropostaPatrocinioSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            // Patrocinador é um Usuário
            var patrocinadorMap = idMappings.GetValueOrDefault("Usuario");
            var campeonatoMap = idMappings.GetValueOrDefault("Campeonato");

            if (patrocinadorMap == null || campeonatoMap == null) {
                _logger.LogWarning("[PropostaPatrocinioService] Mapeamentos de Usuario ou Campeonato ausentes para atualização de FKs.");
                return;
            }

            var clientAppIds = dtos.Select(d => d.ClientAppId).ToHashSet();

            var itemsToUpdate = await _context.PropostasPatrocinio
                .Where(p => clientAppIds.Contains(p.ClientAppId))
                .ToListAsync();

            foreach (var existingItem in itemsToUpdate) {
                var dto = dtos.First(d => d.ClientAppId == existingItem.ClientAppId);
                bool updated = false;

                // 1. Resolver PatrocinadorId (Usuario)
                if (patrocinadorMap.TryGetValue(dto.PatrocinadorClientAppId, out int newPatrocinadorId)) {
                    if (existingItem.PatrocinadorId != newPatrocinadorId) {
                        existingItem.PatrocinadorId = newPatrocinadorId;
                        updated = true;
                    }
                }

                // 2. Resolver CampeonatoId
                if (campeonatoMap.TryGetValue(dto.CampeonatoClientAppId, out int newCampeonatoId)) {
                    if (existingItem.CampeonatoId != newCampeonatoId) {
                        existingItem.CampeonatoId = newCampeonatoId;
                        updated = true;
                    }
                }

                if (updated) {
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
        }

        // Obter atualizações para o cliente
        public async Task<IEnumerable<PropostaPatrocinioSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            // Incluir Patrocinador e Campeonato para obter os ClientAppIds
            return await _context.PropostasPatrocinio
                .Include(p => p.Patrocinador)
                .Include(p => p.Campeonato)
                .Where(p => p.UpdatedAt > lastSyncTime)
                .Select(p => new PropostaPatrocinioSyncDto {
                    Id = p.Id,
                    ClientAppId = p.ClientAppId,
                    UpdatedAt = p.UpdatedAt,
                    IsSynced = p.IsSynced,

                    PatrocinadorClientAppId = p.Patrocinador != null ? p.Patrocinador.ClientAppId : Guid.Empty,
                    CampeonatoClientAppId = p.Campeonato != null ? p.Campeonato.ClientAppId : Guid.Empty,

                    NomePatrocinador = p.NomePatrocinador,
                    ImagemPatrocinador = p.ImagemPatrocinador,
                    LinkPatrocinador = p.LinkPatrocinador,
                    ValorMonetario = p.ValorMonetario,
                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,
                    Mensagem = p.Mensagem,
                    Aprovada = p.Aprovada,
                })
                .ToListAsync();
        }
    }
}