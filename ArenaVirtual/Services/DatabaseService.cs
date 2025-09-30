using ArenaVirtual.Models;
using SQLite;
using System.Diagnostics;

namespace ArenaVirtual.Services {

    public class DatabaseService(string dbPath) {
        private readonly SQLiteAsyncConnection _database = new(dbPath);

        private class TimeClientAppIdProjection {
            public Guid TimeClientAppId { get; set; }
        }

        // --- Inicialização ---

        public async Task InitializeAsync() {
            // Criação de todas as tabelas
            await _database.CreateTableAsync<Usuario>();
            await _database.CreateTableAsync<Campeonato>();
            await _database.CreateTableAsync<Time>();
            await _database.CreateTableAsync<Partida>();
            await _database.CreateTableAsync<AvaliacaoArbitro>();
            await _database.CreateTableAsync<CampanhaPatrocinio>();
            await _database.CreateTableAsync<Estatistica>();
            await _database.CreateTableAsync<Jogo>();
            await _database.CreateTableAsync<PropostaPatrocinio>();
            await _database.CreateTableAsync<UsuarioCampeonatoFavorito>();
            await _database.CreateTableAsync<Convite>();
            await _database.CreateTableAsync<Inscricao>();
        }

        public AsyncTableQuery<T> GetTable<T>() where T : new() {
            return _database.Table<T>();
        }

        // --- Métodos de Usuário ---

        public Task<int> InserirUsuarioAsync(Usuario usuario) => _database.InsertAsync(usuario);
        public Task<Usuario?> ObterUsuarioPorEmailAsync(string email) =>
            _database.Table<Usuario>().Where(u => u.Email == email).FirstOrDefaultAsync();
        public Task<List<Usuario>> ListarUsuariosAsync() => _database.Table<Usuario>().ToListAsync();
        public async Task<bool> EmailExisteAsync(string email) {
            var usuario = await _database.Table<Usuario>().Where(u => u.Email == email).FirstOrDefaultAsync();
            return usuario != null;
        }
        public async Task<int> AtualizarUsuarioAsync(Usuario usuario) {
            Debug.WriteLine($"[DatabaseService] Atualizando usuário ID: {usuario.Id}, ImagemPath: {usuario.ImagemPath}");
            var existingUser = await _database.FindAsync<Usuario>(usuario.Id);
            if (existingUser != null) {
                Debug.WriteLine($"[DatabaseService] Usuário existente no DB (ID={existingUser.Id}): ImagemPath={existingUser.ImagemPath}");
            } else {
                Debug.WriteLine($"[DatabaseService] Usuário com ID {usuario.Id} NÃO encontrado no DB para atualização.");
                return 0;
            }
            int rowsAffected = await _database.UpdateAsync(usuario);
            Debug.WriteLine($"[DatabaseService] UpdateAsync retornou: {rowsAffected} linhas afetadas.");
            return rowsAffected;
        }
        public Task<int> DeletarUsuarioAsync(Usuario usuario) => _database.DeleteAsync(usuario);

        public Task<Usuario?> ObterUsuarioPorClientAppIdAsync(Guid clientAppId) =>
            _database.Table<Usuario>().Where(u => u.ClientAppId == clientAppId).FirstOrDefaultAsync();

        public Task<List<Usuario>> GetMembrosByTimeClientAppIdAsync(Guid timeClientAppId) =>
            _database.Table<Usuario>().Where(u => u.TimeClientAppId == timeClientAppId).ToListAsync();

        public async Task<Guid?> GetUsuarioClientAppIdById(int id) {
            var usuario = await _database.Table<Usuario>().Where(u => u.Id == id).FirstOrDefaultAsync();
            return usuario?.ClientAppId;
        }

        public AsyncTableQuery<Usuario> GetUsuarioTable() => _database.Table<Usuario>();

        // --- Métodos de Campeonato ---

        public async Task<int> InserirCampeonatoAsync(Campeonato campeonato) {
            var existente = await _database.Table<Campeonato>().Where(c => c.Nome == campeonato.Nome && c.DataInicio == campeonato.DataInicio).FirstOrDefaultAsync();
            if (existente != null) return 0;
            return await _database.InsertAsync(campeonato);
        }
        public Task<List<Campeonato>> ListarCampeonatosAsync() => _database.Table<Campeonato>().ToListAsync();
        public Task<int> AtualizarCampeonatoAsync(Campeonato item) => _database.UpdateAsync(item);
        public Task<int> DeletarCampeonatoAsync(Campeonato item) => _database.DeleteAsync(item);

        public Task<Campeonato?> ObterCampeonatoPorCapitaoClientAppIdAsync(Guid capitaoClientAppId) =>
            _database.Table<Campeonato>().Where(c => c.CapitaoClientAppId == capitaoClientAppId).FirstOrDefaultAsync();

        // --- Métodos de Time ---

        public Task<int> InserirTimeAsync(Time item) => _database.InsertAsync(item);
        public Task<List<Time>> ListarTimesAsync() => _database.Table<Time>().ToListAsync();
        public Task<int> AtualizarTimeAsync(Time item) => _database.UpdateAsync(item);
        public Task<int> DeletarTimeAsync(Time item) => _database.DeleteAsync(item);
        public Task<int> ExcluirTimeAsync(Time time) => _database.DeleteAsync(time);

        public async Task<Time?> GetTimeByClientAppIdAsync(Guid clientAppId) {
            try {
                return await _database.Table<Time>().FirstOrDefaultAsync(t => t.ClientAppId == clientAppId);
            } catch (Exception ex) {
                Debug.WriteLine($"[DatabaseService] Erro ao obter Time por ClientAppId: {ex.Message}");
                return null;
            }
        }
        public Task<Time?> GetTimeByIdAsync(int id) => _database.Table<Time>().Where(t => t.Id == id).FirstOrDefaultAsync();

        public async Task<List<Time>> GetTimesPorCampeonatoAsync(Guid campeonatoClientAppId) {
            try {
                var times = await _database.Table<Time>()
                                             .Where(t => t.CampeonatoClientAppId == campeonatoClientAppId)
                                             .ToListAsync();
                return times;
            } catch (Exception ex) {
                Debug.WriteLine($"[DatabaseService] Erro ao buscar times por campeonato: {ex.Message}");
                return new List<Time>();
            }
        }

        // ***********************************************
        // ObterTimesAceitosAsync para usar a tabela Convite
        // AJUSTADO PARA USAR (int) PARA OS ENUMS
        // ***********************************************
        public async Task<List<Time>> ObterTimesAceitosAsync(int campeonatoId) {
            try {
                var campeonato = await _database.Table<Campeonato>()
                                        .Where(c => c.Id == campeonatoId)
                                        .FirstOrDefaultAsync();

                if (campeonato == null) {
                    return new List<Time>();
                }

                // CORREÇÃO: Usar os valores inteiros (int) das Enums
                var idsAceitos = await _database.QueryAsync<TimeClientAppIdProjection>(
                    "SELECT TimeClientAppId FROM Convite WHERE CampeonatoClientAppId = ? AND Status = ? AND Tipo = ?",
                    campeonato.ClientAppId,
                    (int)StatusConvite.Aceito,
                    (int)TipoConvite.InscricaoCampeonato
                );

                var solicitacoesAceitas = idsAceitos
                    .Select(p => p.TimeClientAppId)
                    .ToHashSet();

                if (solicitacoesAceitas.Count == 0) {
                    Debug.WriteLine("[DatabaseService] Nenhuma solicitação 'Aceita' encontrada para este campeonato.");
                    return new List<Time>();
                }

                // 3. Buscar os objetos Time usando os IDs encontrados
                var timesAceitos = await _database.Table<Time>()
                    .Where(t => solicitacoesAceitas.Contains(t.ClientAppId))
                    .ToListAsync();

                Debug.WriteLine($"[DatabaseService] Encontrados {timesAceitos.Count} times aceitos (via ObterTimesAceitosAsync).");

                return timesAceitos;

            } catch (Exception ex) {
                Debug.WriteLine($"[DatabaseService] ERRO ao obter times aceitos: {ex.Message}");
                return new List<Time>();
            }
        }

        // --- Métodos de Convite ---

        // MÉTODO DE LISTAGEM DE PENDENTES (Corrigido na etapa anterior)
        public async Task<List<Convite>> ObterConvitesPendentesAsync(Guid campeonatoClientAppId) {
            try {
                var solicitacoesPendentes = await _database.Table<Convite>()
                    .Where(c => c.CampeonatoClientAppId == campeonatoClientAppId
                            && c.Status == StatusConvite.Pendente
                            && c.Tipo == TipoConvite.InscricaoCampeonato)
                    .ToListAsync();

                Debug.WriteLine($"[DatabaseService] Encontradas {solicitacoesPendentes.Count} solicitações de campeonato pendentes.");

                return solicitacoesPendentes;

            } catch (Exception ex) {
                Debug.WriteLine($"[DatabaseService] ERRO ao obter convites pendentes de campeonato: {ex.Message}");
                return new List<Convite>();
            }
        }

        // Substitui o antigo AtualizarSolicitacaoCampeonatoAsync
        public Task<int> AtualizarSolicitacaoCampeonatoAsync_Convite(Convite solicitacao) =>
            _database.UpdateAsync(solicitacao);

        public Task<int> InserirConviteAsync(Convite convite) => _database.InsertAsync(convite);
        public Task<int> AtualizarConviteAsync(Convite convite) => _database.UpdateAsync(convite);

        public Task<List<Convite>> ListarConvitesPendentesAsync(Guid timeClientAppId) =>
            _database.Table<Convite>().Where(c => c.TimeClientAppId == timeClientAppId && c.Status == StatusConvite.Pendente).ToListAsync();

        public Task<Convite?> ObterConvitePorUsuarioETimeAsync(Guid solicitanteClientAppId, Guid timeClientAppId) =>
            _database.Table<Convite>().FirstOrDefaultAsync(c => c.SolicitanteClientAppId == solicitanteClientAppId && c.TimeClientAppId == timeClientAppId);

        public Task<Convite?> ObterConvitePendenteDoUsuarioAsync(Guid solicitanteClientAppId) =>
            _database.Table<Convite>()
                .Where(c => c.SolicitanteClientAppId == solicitanteClientAppId && c.Status == StatusConvite.Pendente)
                .FirstOrDefaultAsync();

        public Task<int> DeletarConvitePendenteDoUsuarioAsync(Guid usuarioClientAppId) =>
            _database.Table<Convite>()
                .Where(c => c.SolicitanteClientAppId == usuarioClientAppId && c.Status == StatusConvite.Pendente)
                .DeleteAsync();

        public async Task<Convite?> ObterSolicitacaoPorTimeECampeonatoAsync(string timeId, string campeonatoId) {
            if (!Guid.TryParse(timeId, out var timeGuid) || !Guid.TryParse(campeonatoId, out var campeonatoGuid)) {
                return null;
            }
            var solicitacao = await _database.Table<Convite>()
                .Where(s => s.TimeClientAppId == timeGuid &&
                             s.CampeonatoClientAppId == campeonatoGuid &&
                             s.Tipo == TipoConvite.InscricaoCampeonato) 
                .FirstOrDefaultAsync();
            return solicitacao;
        }

        public AsyncTableQuery<Convite> GetConviteTable() => _database.Table<Convite>();

        // --- Métodos de Inscricao ---

        public Task<List<Inscricao>> ObterInscricoesPendentesPorCampeonatoAsync(Guid campeonatoClientAppId) =>
        _database.Table<Inscricao>()
            .Where(i => i.CampeonatoClientAppId == campeonatoClientAppId && i.Status == StatusConvite.Pendente.ToString())
            .ToListAsync();

        public Task<int> AtualizarInscricaoAsync(Inscricao inscricao) => _database.UpdateAsync(inscricao);

        // --- Outros Métodos (Partida, AvaliacaoArbitro, etc) - Mantidos ---

        public Task<int> InserirPartidaAsync(Partida item) => _database.InsertAsync(item);
        public Task<List<Partida>> ListarPartidasAsync() => _database.Table<Partida>().ToListAsync();
        public Task<int> AtualizarPartidaAsync(Partida item) => _database.UpdateAsync(item);
        public Task<int> DeletarPartidaAsync(Partida item) => _database.DeleteAsync(item);

        public Task<int> InserirAvaliacaoArbitroAsync(AvaliacaoArbitro item) => _database.InsertAsync(item);
        public Task<List<AvaliacaoArbitro>> ListarAvaliacoesArbitroAsync() => _database.Table<AvaliacaoArbitro>().ToListAsync();
        public Task<int> AtualizarAvaliacaoArbitroAsync(AvaliacaoArbitro item) => _database.UpdateAsync(item);
        public Task<int> DeletarAvaliacaoArbitroAsync(AvaliacaoArbitro item) => _database.DeleteAsync(item);

        public Task<int> InserirCampanhaPatrocinioAsync(CampanhaPatrocinio item) => _database.InsertAsync(item);
        public Task<List<CampanhaPatrocinio>> ListarCampanhasPatrocinioAsync() => _database.Table<CampanhaPatrocinio>().ToListAsync();
        public Task<int> AtualizarCampanhaPatrocinioAsync(CampanhaPatrocinio item) => _database.UpdateAsync(item);
        public Task<int> DeletarCampanhaPatrocinioAsync(CampanhaPatrocinio item) => _database.DeleteAsync(item);

        public Task<int> InserirEstatisticaAsync(Estatistica item) => _database.InsertAsync(item);
        public Task<List<Estatistica>> ListarEstatisticasAsync() => _database.Table<Estatistica>().ToListAsync();
        public Task<int> AtualizarEstatisticaAsync(Estatistica item) => _database.UpdateAsync(item);
        public Task<int> DeletarEstatisticaAsync(Estatistica item) => _database.DeleteAsync(item);

        public Task<int> InserirJogoAsync(Jogo item) => _database.InsertAsync(item);
        public Task<List<Jogo>> ListarJogosAsync() => _database.Table<Jogo>().ToListAsync();
        public Task<int> AtualizarJogoAsync(Jogo item) => _database.UpdateAsync(item);
        public Task<int> DeletarJogoAsync(Jogo item) => _database.DeleteAsync(item);

        public Task<int> InserirPropostaPatrocinioAsync(PropostaPatrocinio item) => _database.InsertAsync(item);
        public Task<List<PropostaPatrocinio>> ListarPropostasPatrocinioAsync() => _database.Table<PropostaPatrocinio>().ToListAsync();
        public Task<int> AtualizarPropostaPatrocinioAsync(PropostaPatrocinio item) => _database.UpdateAsync(item);
        public Task<int> DeletarPropostaPatrocinioAsync(PropostaPatrocinio item) => _database.DeleteAsync(item);

        public Task<int> InserirFavoritoAsync(UsuarioCampeonatoFavorito favorito) => _database.InsertAsync(favorito);
        public Task<int> DeletarFavoritoAsync(UsuarioCampeonatoFavorito favorito) => _database.DeleteAsync(favorito);
        public Task<List<UsuarioCampeonatoFavorito>> ListarFavoritosPorUsuarioAsync(Guid usuarioClientAppId) =>
            _database.Table<UsuarioCampeonatoFavorito>().Where(f => f.UsuarioClientAppId == usuarioClientAppId).ToListAsync();

        // --- Métodos de Sincronização (ISyncable) - Mantidos ---

        public Task<List<T>> GetUnsyncedItemsAsync<T>() where T : ISyncable, new() =>
            _database.Table<T>().Where(i => !i.IsSynced).ToListAsync();

        public async Task<List<T>> GetItemsByClientAppIdsAsync<T>(HashSet<Guid> clientAppIds) where T : ISyncable, new() {
            return await _database.Table<T>().Where(i => clientAppIds.Contains(i.ClientAppId)).ToListAsync();
        }

        public async Task MarkAsSyncedAsync<T>(List<T> items) where T : ISyncable {
            await _database.RunInTransactionAsync(conn => {
                foreach (var item in items) {
                    item.IsSynced = true;
                    conn.Update(item);
                }
            });
        }

        public async Task SaveDownloadedItemsAsync<T>(List<T> items) where T : ISyncable, new() {
            foreach (var item in items) {
                var existingItem = await _database.Table<T>().Where(i => i.Id == item.Id).FirstOrDefaultAsync();
                if (existingItem != null) {
                    item.IsSynced = true;
                    await _database.UpdateAsync(item);
                } else {
                    item.IsSynced = true;
                    await _database.InsertAsync(item);
                }
            }
        }

        public async Task UpdateIdAndMarkAsSyncedAsync<T>(T item, int serverId) where T : ISyncable, new() {
            var existingItem = await _database.Table<T>().Where(i => i.ClientAppId == item.ClientAppId).FirstOrDefaultAsync();
            if (existingItem != null) {
                existingItem.Id = serverId;
                existingItem.IsSynced = true;
                await _database.UpdateAsync(existingItem);
            }
        }
    }
}