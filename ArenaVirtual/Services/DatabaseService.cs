using ArenaVirtual.Models;
using ArenaVirtual.DTOs;
using SQLite;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArenaVirtual.Services {
    public class DatabaseService {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath) {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        private class TimeClientAppIdProjection { public Guid TimeClientAppId { get; set; } }
        private class CampeonatoIdProjection { public int CampeonatoId { get; set; } }

        public async Task InitializeAsync() {
            await _database.CreateTableAsync<Usuario>();
            await _database.CreateTableAsync<Campeonato>();
            await _database.CreateTableAsync<Time>();
            await _database.CreateTableAsync<Jogo>();
            await _database.CreateTableAsync<AvaliacaoArbitro>();
            await _database.CreateTableAsync<CampanhaPatrocinio>();
            await _database.CreateTableAsync<EstatisticaPartida>();
            await _database.CreateTableAsync<PropostaPatrocinio>();
            await _database.CreateTableAsync<UsuarioCampeonatoFavorito>();
            await _database.CreateTableAsync<Convite>();
            await _database.CreateTableAsync<Inscricao>();
        }

        public AsyncTableQuery<T> GetTable<T>() where T : new() => _database.Table<T>();

        // --- MÉTODOS DE EXCLUSÃO ---
        public async Task DeletarTimeComCascataAsync(Time time) {
            Debug.WriteLine($"[DeletarTimeComCascataAsync] Excluindo time ClientAppId: {time.ClientAppId}");
            await _database.Table<Convite>().Where(c => c.TimeClientAppId == time.ClientAppId).DeleteAsync();
            var membros = await _database.Table<Usuario>().Where(u => u.TimeClientAppId == time.ClientAppId).ToListAsync();
            foreach (var membro in membros) { membro.TimeClientAppId = null; }
            await _database.UpdateAllAsync(membros);
            await _database.DeleteAsync(time);
        }

        public async Task DeletarCampeonatoComCascataAsync(Campeonato campeonato) {
            Debug.WriteLine($"[DeletarCampeonatoComCascataAsync] Excluindo campeonato ClientAppId: {campeonato.ClientAppId}");
            var partidas = await _database.Table<Jogo>().Where(p => p.CampeonatoId == campeonato.Id).ToListAsync();
            foreach (var partida in partidas) {
                var jogo = await _database.Table<Jogo>().Where(j => j.Id == partida.Id).FirstOrDefaultAsync();
                if (jogo != null) {
                    await _database.Table<EstatisticaPartida>().Where(e => e.JogoId == jogo.Id).DeleteAsync();
                    await _database.Table<AvaliacaoArbitro>().Where(a => a.JogoId == jogo.Id).DeleteAsync();
                    await _database.DeleteAsync(jogo);
                }
            }
            await _database.DeleteAsync(partidas);
            await _database.Table<Convite>().Where(c => c.CampeonatoClientAppId == campeonato.ClientAppId).DeleteAsync();
            await _database.Table<Inscricao>().Where(i => i.CampeonatoClientAppId == campeonato.ClientAppId).DeleteAsync();
            await _database.Table<UsuarioCampeonatoFavorito>().Where(f => f.CampeonatoClientAppId == campeonato.ClientAppId).DeleteAsync();
            await _database.DeleteAsync(campeonato);
        }

        // --- MÉTODOS DE USUÁRIO ---
        public Task<int> InserirUsuarioAsync(Usuario usuario) => _database.InsertAsync(usuario);
        public Task<Usuario?> ObterUsuarioPorEmailAsync(string email) => _database.Table<Usuario>().Where(u => u.Email == email).FirstOrDefaultAsync();
        public Task<List<Usuario>> ListarUsuariosAsync() => _database.Table<Usuario>().ToListAsync();
        public async Task<bool> EmailExisteAsync(string email) => await _database.Table<Usuario>().Where(u => u.Email == email).CountAsync() > 0;
        public Task<int> AtualizarUsuarioAsync(Usuario usuario) => _database.UpdateAsync(usuario);
        public async Task<int> DeletarUsuarioAsync(Usuario usuario) {
            if (usuario == null || usuario.Id == 0) return 0;
            if (usuario.Perfil == TipoPerfil.Organizador) {
                var times = await _database.Table<Time>().Where(t => t.AdminClientAppId == usuario.ClientAppId).ToListAsync();
                foreach (var time in times) await DeletarTimeComCascataAsync(time);
                var campeonatos = await _database.Table<Campeonato>().Where(c => c.OrganizadorId == usuario.Id).ToListAsync();
                foreach (var campeonato in campeonatos) await DeletarCampeonatoComCascataAsync(campeonato);
            }
            if (usuario.TimeClientAppId.HasValue) usuario.TimeClientAppId = null;
            await _database.Table<Convite>().Where(c => c.SolicitanteClientAppId == usuario.ClientAppId || c.UsuarioClientAppId == usuario.ClientAppId).DeleteAsync();
            return await _database.DeleteAsync(usuario);
        }
        public Task<Usuario?> ObterUsuarioPorClientAppIdAsync(Guid clientAppId) => _database.Table<Usuario>().Where(u => u.ClientAppId == clientAppId).FirstOrDefaultAsync();
        public Task<List<Usuario>> GetMembrosByTimeClientAppIdAsync(Guid timeClientAppId) => _database.Table<Usuario>().Where(u => u.TimeClientAppId == timeClientAppId).ToListAsync();
        public async Task<Guid?> GetUsuarioClientAppIdById(int id) => (await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.Id == id))?.ClientAppId;
        public AsyncTableQuery<Usuario> GetUsuarioTable() => _database.Table<Usuario>();
        public async Task<List<Usuario>> ObterUsuariosPorIdsAsync(List<Guid> userIds) {
            if (userIds == null || !userIds.Any()) return new List<Usuario>();
            return await _database.Table<Usuario>().Where(u => userIds.Contains(u.ClientAppId)).ToListAsync();
        }

        // --- MÉTODOS DE CAMPEONATO ---
        public async Task<int> InserirCampeonatoAsync(Campeonato campeonato) => await _database.Table<Campeonato>().Where(c => c.Nome == campeonato.Nome && c.DataInicio == campeonato.DataInicio).CountAsync() > 0 ? 0 : await _database.InsertAsync(campeonato);
        public Task<List<Campeonato>> ListarCampeonatosAsync() => _database.Table<Campeonato>().ToListAsync();
        public Task<int> AtualizarCampeonatoAsync(Campeonato item) => _database.UpdateAsync(item);
        public Task<int> DeletarCampeonatoAsync(Campeonato item) => _database.DeleteAsync(item);
        public Task<Campeonato?> ObterCampeonatoPorCapitaoClientAppIdAsync(Guid capitaoClientAppId) => _database.Table<Campeonato>().Where(c => c.CapitaoClientAppId == capitaoClientAppId).FirstOrDefaultAsync();
        public async Task<HashSet<int>> ObterIdsCampeonatosDoTimeAceitoAsync(Guid timeClientAppId) {
            var convitesAceitos = await _database.Table<Convite>().Where(c => c.TimeClientAppId == timeClientAppId && c.Status == StatusConvite.Aceito && c.Tipo == TipoConvite.InscricaoCampeonato).ToListAsync();
            if (!convitesAceitos.Any()) return new HashSet<int>();
            var campClientAppIds = convitesAceitos.Select(c => c.CampeonatoClientAppId).ToHashSet();
            var campeonatos = await _database.Table<Campeonato>().Where(c => campClientAppIds.Contains(c.ClientAppId)).ToListAsync();
            return campeonatos.Select(c => c.Id).ToHashSet();
        }

        // --- MÉTODOS DE TIME ---
        public Task<int> InserirTimeAsync(Time item) => _database.InsertAsync(item);
        public Task<List<Time>> ListarTimesAsync() => _database.Table<Time>().ToListAsync();
        public Task<int> AtualizarTimeAsync(Time item) => _database.UpdateAsync(item);
        public Task<int> DeletarTimeAsync(Time item) => _database.DeleteAsync(item);
        public Task<int> ExcluirTimeAsync(Time time) => _database.DeleteAsync(time);
        public Task<Time?> GetTimeByClientAppIdAsync(Guid clientAppId) => _database.Table<Time>().FirstOrDefaultAsync(t => t.ClientAppId == clientAppId);
        public Task<Time?> GetTimeByIdAsync(int id) => _database.Table<Time>().FirstOrDefaultAsync(t => t.Id == id);
        public Task<List<Time>> GetTimesPorCampeonatoAsync(Guid campeonatoClientAppId) => _database.Table<Time>().Where(t => t.CampeonatoClientAppId == campeonatoClientAppId).ToListAsync();
        public async Task<List<Time>> ObterTimesAceitosAsync(int campeonatoId) {
            var campeonato = await _database.Table<Campeonato>().FirstOrDefaultAsync(c => c.Id == campeonatoId);
            if (campeonato == null) return new List<Time>();
            var idsAceitos = await _database.QueryAsync<TimeClientAppIdProjection>("SELECT TimeClientAppId FROM Convite WHERE CampeonatoClientAppId = ? AND Status = ? AND Tipo = ?", campeonato.ClientAppId, (int)StatusConvite.Aceito, (int)TipoConvite.InscricaoCampeonato);
            var solicitacoesAceitas = idsAceitos.Select(p => p.TimeClientAppId).ToHashSet();
            if (!solicitacoesAceitas.Any()) return new List<Time>();
            return await _database.Table<Time>().Where(t => solicitacoesAceitas.Contains(t.ClientAppId)).ToListAsync();
        }

        // --- MÉTODOS DE CONVITE ---
        public Task<List<Convite>> ObterConvitesPendentesAsync(Guid campeonatoClientAppId) => _database.Table<Convite>().Where(c => c.CampeonatoClientAppId == campeonatoClientAppId && c.Status == StatusConvite.Pendente).ToListAsync();
        public Task<List<Convite>> ObterConvitesPendentesAsync(Guid campeonatoClientAppId, TipoConvite tipo) => _database.Table<Convite>().Where(c => c.CampeonatoClientAppId == campeonatoClientAppId && c.Status == StatusConvite.Pendente && c.Tipo == tipo).ToListAsync();
        public Task<List<Convite>> ObterConvitesAceitosPorCampeonatoAsync(Guid campeonatoClientAppId) => _database.Table<Convite>().Where(c => c.CampeonatoClientAppId == campeonatoClientAppId && c.Status == StatusConvite.Aceito && c.Tipo == TipoConvite.InscricaoArbitro).ToListAsync();
        public async Task<Convite?> ObterSolicitacaoPorArbitroECampeonatoAsync(string arbitroId, string campeonatoId, TipoConvite tipo) { if (!Guid.TryParse(arbitroId, out var arbitroGuid) || !Guid.TryParse(campeonatoId, out var campeonatoGuid)) return null; return await _database.Table<Convite>().FirstOrDefaultAsync(s => s.UsuarioClientAppId == arbitroGuid && s.CampeonatoClientAppId == campeonatoGuid && s.Tipo == tipo); }
        public Task<int> AtualizarSolicitacaoCampeonatoAsync_Convite(Convite solicitacao) => _database.UpdateAsync(solicitacao);
        public Task<int> InserirConviteAsync(Convite convite) => _database.InsertAsync(convite);
        public Task<int> AtualizarConviteAsync(Convite convite) => _database.UpdateAsync(convite);
        public Task<List<Convite>> ListarConvitesPendentesAsync(Guid timeClientAppId) => _database.Table<Convite>().Where(c => c.TimeClientAppId == timeClientAppId && c.Status == StatusConvite.Pendente).ToListAsync();
        public Task<Convite?> ObterConvitePorUsuarioETimeAsync(Guid solicitanteClientAppId, Guid timeClientAppId) => _database.Table<Convite>().FirstOrDefaultAsync(c => c.SolicitanteClientAppId == solicitanteClientAppId && c.TimeClientAppId == timeClientAppId);
        public Task<Convite?> ObterConvitePendenteDoUsuarioAsync(Guid solicitanteClientAppId) => _database.Table<Convite>().FirstOrDefaultAsync(c => c.SolicitanteClientAppId == solicitanteClientAppId && c.Status == StatusConvite.Pendente);
        public Task<int> DeletarConvitePendenteDoUsuarioAsync(Guid usuarioClientAppId) => _database.Table<Convite>().Where(c => c.SolicitanteClientAppId == usuarioClientAppId && c.Status == StatusConvite.Pendente).DeleteAsync();
        public async Task<Convite?> ObterSolicitacaoPorTimeECampeonatoAsync(string timeId, string campeonatoId) { if (!Guid.TryParse(timeId, out var timeGuid) || !Guid.TryParse(campeonatoId, out var campeonatoGuid)) return null; return await _database.Table<Convite>().FirstOrDefaultAsync(s => s.TimeClientAppId == timeGuid && s.CampeonatoClientAppId == campeonatoGuid && s.Tipo == TipoConvite.InscricaoCampeonato); }
        public AsyncTableQuery<Convite> GetConviteTable() => _database.Table<Convite>();
        public Task<int> DeletarConviteArbitroAceitoAsync(Guid campeonatoClientAppId, Guid arbitroClientAppId) {
            return _database.Table<Convite>()
                .Where(c => c.CampeonatoClientAppId == campeonatoClientAppId
                         && c.UsuarioClientAppId == arbitroClientAppId
                         && c.Status == StatusConvite.Aceito
                         && c.Tipo == TipoConvite.InscricaoArbitro)
                .DeleteAsync();
        }

        // --- MÉTODOS DE INSCRICAO ---
        public Task<List<Inscricao>> ObterInscricoesPendentesPorCampeonatoAsync(Guid campeonatoClientAppId) => _database.Table<Inscricao>().Where(i => i.CampeonatoClientAppId == campeonatoClientAppId && i.Status == StatusConvite.Pendente.ToString()).ToListAsync();
        public Task<int> AtualizarInscricaoAsync(Inscricao inscricao) => _database.UpdateAsync(inscricao);

        // --- MÉTODOS DE ÁRBITROS ---
        public Task<int> InserirAvaliacaoArbitroAsync(AvaliacaoArbitro item) => _database.InsertAsync(item);
        public Task<List<AvaliacaoArbitro>> ListarAvaliacoesArbitroAsync() => _database.Table<AvaliacaoArbitro>().ToListAsync();
        public Task<int> AtualizarAvaliacaoArbitroAsync(AvaliacaoArbitro item) => _database.UpdateAsync(item);
        public Task<int> DeletarAvaliacaoArbitroAsync(AvaliacaoArbitro item) => _database.DeleteAsync(item);

        // --- MÉTODOS DE PATROCÍNIOS ---
        public Task<int> InserirPropostaPatrocinioAsync(PropostaPatrocinio item) => _database.InsertAsync(item);
        public Task<List<PropostaPatrocinio>> ListarPropostasPatrocinioAsync() => _database.Table<PropostaPatrocinio>().ToListAsync();
        public Task<int> AtualizarPropostaPatrocinioAsync(PropostaPatrocinio item) => _database.UpdateAsync(item);
        public Task<int> DeletarPropostaPatrocinioAsync(PropostaPatrocinio item) => _database.DeleteAsync(item);
        public Task<int> InserirCampanhaPatrocinioAsync(CampanhaPatrocinio item) => _database.InsertAsync(item);
        public Task<List<CampanhaPatrocinio>> ListarCampanhasPatrocinioAsync() => _database.Table<CampanhaPatrocinio>().ToListAsync();
        public Task<int> AtualizarCampanhaPatrocinioAsync(CampanhaPatrocinio item) => _database.UpdateAsync(item);
        public Task<int> DeletarCampanhaPatrocinioAsync(CampanhaPatrocinio item) => _database.DeleteAsync(item);

        // --- MÉTODOS DE Estatísticas ---
        public Task<int> InserirEstatisticaAsync(EstatisticaPartida item) => _database.InsertAsync(item);
        public Task<List<EstatisticaPartida>> ListarEstatisticasAsync() => _database.Table<EstatisticaPartida>().ToListAsync();
        public Task<int> AtualizarEstatisticaAsync(EstatisticaPartida item) => _database.UpdateAsync(item);
        public Task<int> DeletarEstatisticaAsync(EstatisticaPartida item) => _database.DeleteAsync(item);
        public Task<List<EstatisticaPartida>> ObterEstatisticasPorJogoAsync(int jogoId) => _database.Table<EstatisticaPartida>().Where(e => e.JogoId == jogoId).ToListAsync();
        public Task<List<EstatisticaPartida>> ObterEstatisticasPorAtletaAsync(int usuarioId) => _database.Table<EstatisticaPartida>().Where(e => e.UsuarioId == usuarioId).ToListAsync();

        // --- MÉTODOS DE JOGOS ---
        public async Task<int> AtualizarJogoAsync(Jogo item) {
            // --- PONTO DE DEBUG E: DENTRO DO DATABASE SERVICE ---
            Debug.WriteLine($"[E] DB SERVICE (Atualizar Jogo): Jogo.Id: {item.Id} | ArbitroId: {item.ArbitroId} | IsSynced: {item.IsSynced}");

            if (item.Id <= 0) {
                Debug.WriteLine("[E] DB SERVICE (Falha): Jogo.Id é inválido (<= 0). UpdateAsync retornará 0 (nenhuma linha atualizada).");
                return 0;
            }

            // Executa a atualização no SQLite
            int resultado = await _database.UpdateAsync(item);

            // --- PONTO DE DEBUG F: FIM DO DATABASE SERVICE ---
            Debug.WriteLine($"[F] DB SERVICE (Fim Atualizar): UpdateAsync retornou: {resultado}");
            return resultado;
        }
        public Task<int> InserirJogoAsync(Jogo item) => _database.InsertAsync(item);
        public Task<List<Jogo>> ListarJogosAsync() => _database.Table<Jogo>().ToListAsync();
        public Task<int> DeletarJogoAsync(Jogo item) => _database.DeleteAsync(item);
        public async Task<List<Jogo>> ObterJogosPorCampeonatoAsync(Guid campeonatoClientAppId) {
            // 1. Carrega todos os jogos para o campeonato (incluindo duplicatas).
            var todosOsJogos = await _database.Table<Jogo>()
                .Where(j => j.CampeonatoClientAppId == campeonatoClientAppId)
                .ToListAsync();

            // Logging para rastreio:
            System.Diagnostics.Debug.WriteLine($"[DB SERVICE - Jogo] Encontrados {todosOsJogos.Count} jogos totais no DB para o campeonato.");

            // 2. FILTRAGEM DE DUPLICATAS:
            // Agrupa os jogos por sua identidade única (Rodada e combinação de times).
            // Para garantir que TimeA vs TimeB seja igual a TimeB vs TimeA, usamos Math.Min/Math.Max.
            var jogosUnicos = todosOsJogos
                .GroupBy(j => new {
                    j.Rodada,
                    TimeId1 = Math.Min(j.TimeAId, j.TimeBId),
                    TimeId2 = Math.Max(j.TimeAId, j.TimeBId)
                })
                .Select(g => g.OrderByDescending(j => j.ArbitroId.HasValue).ThenBy(j => j.Id).First())
                // Dentro de cada grupo de duplicatas:
                // - Prioriza (OrderByDescending) o registro que tem ArbitroId preenchido (se houver).
                // - Se o ArbitroId for nulo em todos (ou em caso de empate), pega o registro com o menor Id (o mais antigo).
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[DB SERVICE - Jogo] Retornando {jogosUnicos.Count} jogos únicos após a remoção de duplicatas.");

            return jogosUnicos;
        }
        public async Task<int> SalvarJogoAsync(Jogo item) {
            // --- PONTO DE DEBUG E: DENTRO DO DATABASE SERVICE ---
            Debug.WriteLine($"[E] DB SERVICE (Salvar Jogo): Jogo.Id: {item.Id} | ClientAppId: {item.ClientAppId} | ArbitroId: {item.ArbitroId} | IsSynced: {item.IsSynced}");

            // Use InsertOrReplaceAsync para garantir que a atualização funcione
            // mesmo que o Id (PK int) esteja em 0, contanto que o ClientAppId (GUID) seja único.
            int resultado = await _database.InsertOrReplaceAsync(item);

            // --- PONTO DE DEBUG F: FIM DO DATABASE SERVICE ---
            Debug.WriteLine($"[F] DB SERVICE (Fim Salvar): InsertOrReplaceAsync retornou: {resultado}");

            // Se o resultado for 1, a operação foi um sucesso (inseriu ou substituiu 1 linha).
            return resultado;
        }
        public Task<int> InsertAllAsync<T>(IEnumerable<T> items) {
            // _database é a sua instância de SQLiteAsyncConnection
            return _database.InsertAllAsync(items);
        }
        public Task<Time> ObterTimePorIdAsync(int id) {
            return _database.Table<Time>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }
        public Task<List<Jogo>> ObterJogosPorArbitroAsync(Guid arbitroClientAppId) {
            // O Jogo.ArbitroId deve ser o ClientAppId (GUID) do Árbitro, como você já utiliza
            return _database.Table<Jogo>()
                            .Where(j => j.ArbitroId == arbitroClientAppId)
                            .OrderBy(j => j.DataHora) // Ordena por data/hora para ter as próximas partidas no topo
                            .ToListAsync();
        }

        // --- MÉTODOS DE FAVORITOS ---
        public Task<int> InserirFavoritoAsync(UsuarioCampeonatoFavorito favorito) => _database.InsertAsync(favorito);
        public Task<int> DeletarFavoritoAsync(UsuarioCampeonatoFavorito favorito) => _database.DeleteAsync(favorito);
        public Task<List<UsuarioCampeonatoFavorito>> ListarFavoritosPorUsuarioAsync(Guid usuarioClientAppId) => _database.Table<UsuarioCampeonatoFavorito>().Where(f => f.UsuarioClientAppId == usuarioClientAppId).ToListAsync();
        
        // --- MÉTODOS DE SINCRONIZAÇÃO ---
        public Task<List<T>> GetUnsyncedItemsAsync<T>() where T : ISyncable, new() => _database.Table<T>().Where(i => !i.IsSynced).ToListAsync();
        public Task<List<T>> GetItemsByClientAppIdsAsync<T>(HashSet<Guid> clientAppIds) where T : ISyncable, new() => _database.Table<T>().Where(i => clientAppIds.Contains(i.ClientAppId)).ToListAsync();
        public async Task UpdateIdAndMarkAsSyncedAsync<T>(T item, int serverId) where T : ISyncable, new() {
            var existingItem = await _database.Table<T>().Where(i => i.ClientAppId == item.ClientAppId).FirstOrDefaultAsync();
            if (existingItem != null) {
                var idServidorProperty = typeof(T).GetProperty("IdServidor");
                if (idServidorProperty != null && idServidorProperty.CanWrite) {
                    idServidorProperty.SetValue(existingItem, serverId);
                }
                existingItem.IsSynced = true;
                await _database.UpdateAsync(existingItem);
            }
        }

        #region Métodos de Sincronização de Download

        public async Task SaveDownloadedUsuariosAsync(IEnumerable<UsuarioDownloadDto> dtos) {
            // Definindo valores padrão seguros
            TipoPerfil perfilPadrao = TipoPerfil.Atleta;
            GeneroEnum generoPadrao = GeneroEnum.Outro;

            // Usando 0.0m para garantir double correto, embora 0.0 também funcione
            double pesoPadrao = 0.0;
            double alturaPadrao = 0.0;

            foreach (var dto in dtos) {
                var existing = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.ClientAppId == dto.ClientAppId);
                bool isNew = existing == null;
                if (isNew) {
                    // Garante que ClientAppId é sempre um GUID
                    existing = new Usuario { ClientAppId = dto.ClientAppId ?? Guid.Empty };
                }

                existing.IdServidor = dto.Id ?? 0; // Garantindo que IdServidor é 0 se null

                // Se Nome, Email, ImagemPath, etc. não são anuláveis no modelo Usuario,
                // use o operador de coalescência nula (??) aqui também:
                existing.Nome = dto.Nome ?? string.Empty;
                existing.Email = dto.Email ?? string.Empty;

                // O restante das propriedades de valor já estão corretas:
                existing.Perfil = dto.Perfil ?? perfilPadrao;
                existing.ImagemPath = dto.ImagemPath ?? string.Empty;
                existing.Localizacao = dto.Localizacao ?? string.Empty;
                existing.Telefone = dto.Telefone ?? string.Empty;
                existing.LinkRedeSocial = dto.LinkRedeSocial ?? string.Empty;
                existing.DataNascimento = dto.DataNascimento; // DateTime?
                existing.Genero = dto.Genero ?? generoPadrao;
                existing.NomeEmpresa = dto.NomeEmpresa ?? string.Empty;
                existing.CNPJ = dto.CNPJ ?? string.Empty;

                existing.Peso = dto.Peso ?? pesoPadrao; // double? para double?
                existing.Altura = dto.Altura ?? alturaPadrao; // double? para double?
                existing.FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio ?? string.Empty;

                // Lógica de FK (TimeId)
                if (dto.TimeId.HasValue && dto.TimeId > 0) {
                    var time = await _database.Table<Time>().FirstOrDefaultAsync(t => t.IdServidor == dto.TimeId.Value);
                    if (time != null) existing.TimeClientAppId = time.ClientAppId;
                } else {
                    existing.TimeClientAppId = null;
                }

                existing.IsSynced = true;
                existing.UpdatedAt = dto.UpdatedAt ?? DateTime.UtcNow;

                if (isNew) await _database.InsertAsync(existing);
                else await _database.UpdateAsync(existing);
            }
        }

        public async Task SaveDownloadedCampeonatosAsync(IEnumerable<CampeonatoDownloadDto> dtos) {
            foreach (var dto in dtos) {
                var existing = await _database.Table<Campeonato>().FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);
                bool isNew = existing == null;
                if (isNew) {
                    existing = new Campeonato { ClientAppId = dto.ClientAppId };
                }

                existing.IdServidor = dto.Id;
                existing.Nome = dto.Nome ?? string.Empty;
                existing.Local = dto.Local;
                existing.DataInicio = dto.DataInicio;
                existing.DataFim = dto.DataFim;
                existing.LogoUrl = dto.LogoUrl;
                existing.NomeOrganizador = dto.NomeOrganizador;
                existing.EmailOrganizador = dto.EmailOrganizador;
                existing.TelefoneOrganizador = dto.TelefoneOrganizador;
                existing.NumeroMaximoEquipes = dto.NumeroMaximoEquipes;
                existing.ValorTaxaInscricao = dto.ValorTaxaInscricao;
                existing.FormatoCampeonato = dto.FormatoCampeonato;
                existing.LocaisDosJogos = dto.LocaisDosJogos;
                existing.HaveraPremiacao = dto.HaveraPremiacao;
                existing.Descricao = dto.Descricao;
                existing.Modalidade = dto.Modalidade;
                existing.Regras = dto.Regras;
                existing.DataTermino = dto.DataTermino;
                existing.NumeroEquipes = dto.NumeroEquipes ?? 0;

                var organizador = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.IdServidor == dto.OrganizadorId);
                if (organizador != null) existing.OrganizadorClientAppId = organizador.ClientAppId;

                existing.IsSynced = true;
                existing.UpdatedAt = dto.UpdatedAt;

                if (isNew) await _database.InsertAsync(existing);
                else await _database.UpdateAsync(existing);
            }
        }

        public async Task SaveDownloadedTimesAsync(IEnumerable<TimeDownloadDto> dtos) {
            foreach (var dto in dtos) {
                var existing = await _database.Table<Time>().FirstOrDefaultAsync(t => t.ClientAppId == dto.ClientAppId);
                bool isNew = existing == null;
                if (isNew) {
                    existing = new Time { ClientAppId = dto.ClientAppId };
                }

                existing.IdServidor = dto.Id;
                existing.Nome = dto.Nome ?? string.Empty;
                existing.LogoUrl = dto.LogoUrl;
                existing.Descricao = dto.Descricao;
                existing.DataCriacao = dto.DataCriacao;
                existing.Regiao = dto.Regiao;
                existing.PontuacaoTotal = dto.PontuacaoTotal;
                existing.Vitorias = dto.Vitorias;
                existing.Derrotas = dto.Derrotas;
                existing.Empates = dto.Empates;

                if (dto.CampeonatoId.HasValue && dto.CampeonatoId > 0) {
                    var camp = await _database.Table<Campeonato>().FirstOrDefaultAsync(c => c.IdServidor == dto.CampeonatoId.Value);
                    if (camp != null) existing.CampeonatoClientAppId = camp.ClientAppId;
                }
                if (dto.CapitaoId.HasValue && dto.CapitaoId > 0) {
                    var capitao = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.IdServidor == dto.CapitaoId.Value);
                    if (capitao != null) existing.CapitaoClientAppId = capitao.ClientAppId;
                }

                existing.IsSynced = true;
                existing.UpdatedAt = dto.UpdatedAt;

                if (isNew) await _database.InsertAsync(existing);
                else await _database.UpdateAsync(existing);
            }
        }

        public async Task SaveDownloadedConvitesAsync(IEnumerable<ConviteDownloadDto> dtos) {
            foreach (var dto in dtos) {
                var existing = await _database.Table<Convite>().FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);
                bool isNew = existing == null;
                if (isNew) {
                    existing = new Convite { ClientAppId = dto.ClientAppId };
                }

                existing.IdServidor = dto.Id;
                existing.ConvidadoEmail = dto.ConvidadoEmail;
                existing.DataEnvio = dto.DataEnvio;
                existing.Status = dto.Status;

                var solicitante = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.IdServidor == dto.IdSolicitanteId);
                if (solicitante != null) existing.SolicitanteClientAppId = solicitante.ClientAppId;

                var time = await _database.Table<Time>().FirstOrDefaultAsync(t => t.IdServidor == dto.TimeId);
                if (time != null) existing.TimeClientAppId = time.ClientAppId;

                existing.IsSynced = true;
                existing.UpdatedAt = dto.UpdatedAt;

                if (isNew) await _database.InsertAsync(existing);
                else await _database.UpdateAsync(existing);
            }
        }

        public async Task SaveDownloadedUsuarioCampeonatoFavoritosAsync(IEnumerable<UsuarioCampeonatoFavoritoDownloadDto> dtos) {
            foreach (var dto in dtos) {
                var user = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.IdServidor == dto.UsuarioId);
                var camp = await _database.Table<Campeonato>().FirstOrDefaultAsync(c => c.IdServidor == dto.CampeonatoId);

                if (user != null && camp != null) {
                    var existing = await _database.Table<UsuarioCampeonatoFavorito>().FirstOrDefaultAsync(f => f.UsuarioClientAppId == user.ClientAppId && f.CampeonatoClientAppId == camp.ClientAppId);
                    bool isNew = existing == null;
                    if (isNew) {
                        existing = new UsuarioCampeonatoFavorito {
                            ClientAppId = dto.ClientAppId,
                            UsuarioClientAppId = user.ClientAppId,
                            CampeonatoClientAppId = camp.ClientAppId
                        };
                    }

                    existing.IdServidor = dto.Id;
                    existing.IsSynced = true;
                    existing.UpdatedAt = dto.UpdatedAt;

                    if (isNew) await _database.InsertAsync(existing);
                    else await _database.UpdateAsync(existing);
                }
            }
        }

        public async Task SaveDownloadedJogosAsync(IEnumerable<JogoDownloadDto> dtos) {
            foreach (var dto in dtos) {
                var existing = await _database.Table<Jogo>().FirstOrDefaultAsync(j => j.ClientAppId == dto.ClientAppId);
                bool isNew = existing == null;
                if (isNew) {
                    existing = new Jogo { ClientAppId = dto.ClientAppId };
                }

                if (!isNew && !existing.IsSynced) {
                    Debug.WriteLine($"[Sync Download Jogo] Ignorando download para Jogo ClientAppId: {dto.ClientAppId}. Alteração local pendente (IsSynced=false).");
                    continue;
                }

                existing.IdServidor = dto.Id;
                existing.DataHora = dto.DataHora;
                existing.Local = dto.Local ?? string.Empty;

                existing.PlacarA = dto.PlacarA.ToString();
                existing.PlacarB = dto.PlacarB.ToString();

                existing.Rodada = dto.Rodada;
                existing.Status = dto.Status;


                var camp = await _database.Table<Campeonato>().FirstOrDefaultAsync(c => c.IdServidor == dto.CampeonatoId);
                if (camp != null) {
                    existing.CampeonatoClientAppId = camp.ClientAppId;
                    existing.CampeonatoId = camp.Id; // ID local (int)
                } else {
                    existing.CampeonatoClientAppId = Guid.Empty;
                }

                if (dto.ArbitroId.HasValue && dto.ArbitroId.Value > 0) {
                    var arbitro = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.IdServidor == dto.ArbitroId.Value);
                    existing.ArbitroId = arbitro?.ClientAppId;
                } else {
                    existing.ArbitroId = null;
                }

                var timeA = await _database.Table<Time>().FirstOrDefaultAsync(t => t.IdServidor == dto.TimeAId);
                existing.TimeAId = timeA?.Id ?? 0;

                var timeB = await _database.Table<Time>().FirstOrDefaultAsync(t => t.IdServidor == dto.TimeBId);
                existing.TimeBId = timeB?.Id ?? 0;

                existing.IsSynced = true;
                existing.UpdatedAt = dto.UpdatedAt;

                if (isNew) await _database.InsertAsync(existing);
                else await _database.UpdateAsync(existing);
            }
        }

        #endregion
    }
}
