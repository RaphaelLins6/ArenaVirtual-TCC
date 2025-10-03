using ArenaVirtual.Models;
using SQLite;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArenaVirtual.Services {

    public class DatabaseService(string dbPath) {
        private readonly SQLiteAsyncConnection _database = new(dbPath);

        private class TimeClientAppIdProjection {
            public Guid TimeClientAppId { get; set; }
        }

        // NOVO: Classe auxiliar para obter apenas o ID do campeonato
        private class CampeonatoIdProjection {
            public int CampeonatoId { get; set; }
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
            await _database.CreateTableAsync<EstatisticaPartida>();
            await _database.CreateTableAsync<Jogo>();
            await _database.CreateTableAsync<PropostaPatrocinio>();
            await _database.CreateTableAsync<UsuarioCampeonatoFavorito>();
            await _database.CreateTableAsync<Convite>();
            await _database.CreateTableAsync<Inscricao>();
        }

        public AsyncTableQuery<T> GetTable<T>() where T : new() {
            return _database.Table<T>();
        }

        // --- MÉTODOS DE EXCLUSÃO EM CASCATA (NOVOS) ---

        public async Task DeletarTimeComCascataAsync(Time time) {
            Debug.WriteLine($"[DeletarTimeComCascataAsync] Excluindo time ClientAppId: {time.ClientAppId}");

            // 1. Remover Convites (solicitações para este time)
            await _database.Table<Convite>()
                .Where(c => c.TimeClientAppId == time.ClientAppId)
                .DeleteAsync();

            // 2. Remover Jogadores (setar TimeClientAppId para null nos Usuários)
            // Assumimos que o campo TimeClientAppId em Usuario é nullable (Guid?)
            // Buscamos os usuários que estão neste time
            var membros = await _database.Table<Usuario>()
                .Where(u => u.TimeClientAppId == time.ClientAppId)
                .ToListAsync();

            foreach (var membro in membros) {
                membro.TimeClientAppId = null;
                // Não precisa de await aqui, usaremos o UpdateAllAsync ou transação
            }

            // Atualiza os usuários (remove eles do time)
            await _database.UpdateAllAsync(membros);

            // 3. Deletar o Time
            await _database.DeleteAsync(time);
        }

        public async Task DeletarCampeonatoComCascataAsync(Campeonato campeonato) {
            Debug.WriteLine($"[DeletarCampeonatoComCascataAsync] Excluindo campeonato ClientAppId: {campeonato.ClientAppId}");

            // 1. Deletar Entidades de Partida/Jogo (e Estatísticas/Avaliações relacionadas)
            // Partida usa CampeonatoId (int)
            var partidas = await _database.Table<Partida>()
                .Where(p => p.CampeonatoId == campeonato.Id)
                .ToListAsync();

            foreach (var partida in partidas) {
                // CORREÇÃO DEFINITIVA: 
                // EstatisticaPartida, AvaliacaoArbitro e Jogo se ligam à Partida através do JogoId, 
                // e não PartidaId. Vamos assumir que o JogoId é o link principal.

                // Localiza o Jogo correspondente a esta Partida (Partida contém o CampeonatoId e Data/Local, Jogo é o evento com Arbitro/Placar)
                var jogo = await _database.Table<Jogo>()
                    .Where(j => j.Id == partida.Id) // Assumindo que Partida.Id é o Jogo.Id (ou Partida.Id é a FK em Jogo).
                    .FirstOrDefaultAsync();

                if (jogo != null) {
                    // Estatística e Avaliação se ligam pelo JogoId (int)
                    await _database.Table<EstatisticaPartida>()
                        .Where(e => e.JogoId == jogo.Id)
                        .DeleteAsync();

                    await _database.Table<AvaliacaoArbitro>()
                        .Where(a => a.JogoId == jogo.Id)
                        .DeleteAsync();

                    // Deleta o registro de Jogo
                    await _database.DeleteAsync(jogo);
                }
            }

            // Deleta os registros de Partida
            await _database.DeleteAsync(partidas);

            // 2. Deletar Convites e Inscrições relacionados
            await _database.Table<Convite>()
                .Where(c => c.CampeonatoClientAppId == campeonato.ClientAppId)
                .DeleteAsync();

            await _database.Table<Inscricao>()
                .Where(i => i.CampeonatoClientAppId == campeonato.ClientAppId)
                .DeleteAsync();

            // 3. Deletar Favoritos relacionados
            await _database.Table<UsuarioCampeonatoFavorito>()
                .Where(f => f.CampeonatoClientAppId == campeonato.ClientAppId)
                .DeleteAsync();

            // 4. Deletar o próprio Campeonato
            await _database.DeleteAsync(campeonato);
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

        /// MÉTODO DELETAR USUÁRIO REFATORADO COM EXCLUSÃO EM CASCATA
        public async Task<int> DeletarUsuarioAsync(Usuario usuario) {
            if (usuario == null || usuario.Id <= 0) {
                Debug.WriteLine("[DeletarUsuarioAsync] Tentativa de deletar usuário nulo ou sem ID.");
                return 0;
            }

            int totalExcluido = 0;

            // 1. Lógica de Exclusão em Cascata para Organizador
            if (usuario.Perfil == TipoPerfil.Organizador) {
                Debug.WriteLine($"[DeletarUsuarioAsync] Iniciando exclusão em cascata para Organizador ID: {usuario.Id}");

                // A. Deletar Times criados por este organizador
                // CORREÇÃO: Time usa AdminClientAppId para rastrear o criador.
                var times = await _database.Table<Time>()
                    .Where(t => t.AdminClientAppId == usuario.ClientAppId)
                    .ToListAsync();

                foreach (var time in times) {
                    await DeletarTimeComCascataAsync(time);
                }
                Debug.WriteLine($"[DeletarUsuarioAsync] {times.Count} times excluídos em cascata.");

                // B. Deletar Campeonatos criados por este organizador
                // CORREÇÃO: Campeonato usa OrganizadorId.
                var campeonatos = await _database.Table<Campeonato>()
                    .Where(c => c.OrganizadorId == usuario.Id)
                    .ToListAsync();

                foreach (var campeonato in campeonatos) {
                    await DeletarCampeonatoComCascataAsync(campeonato);
                }
                Debug.WriteLine($"[DeletarUsuarioAsync] {campeonatos.Count} campeonatos excluídos em cascata.");
            }

            // 2. Lógica de Limpeza de Referências para todos os perfis

            // A. Remover de times (Se for atleta, árbitro ou patrocinador que estava em um time)
            if (usuario.TimeClientAppId.HasValue) {
                // A remoção de TimeClientAppId é feita antes de deletar o usuário.
                usuario.TimeClientAppId = null;
            }

            // B. Deletar Convites/Solicitações em que o usuário estava envolvido (como solicitante ou alvo de convite)
            await _database.Table<Convite>()
                .Where(c => c.SolicitanteClientAppId == usuario.ClientAppId || c.UsuarioClientAppId == usuario.ClientAppId)
                .DeleteAsync();

            // 3. Deletar o registro principal do Usuário
            totalExcluido = await _database.DeleteAsync(usuario);
            Debug.WriteLine($"[DeletarUsuarioAsync] Registro do Usuário ID {usuario.Id} excluído: {totalExcluido} linha(s).");

            return totalExcluido;
        }

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
        // O antigo DeletarCampeonatoAsync pode ser mantido ou substituído pelo cascata,
        // mas para uso geral, o simples delete pode ser necessário.
        public Task<int> DeletarCampeonatoAsync(Campeonato item) => _database.DeleteAsync(item);

        public Task<Campeonato?> ObterCampeonatoPorCapitaoClientAppIdAsync(Guid capitaoClientAppId) =>
            _database.Table<Campeonato>().Where(c => c.CapitaoClientAppId == capitaoClientAppId).FirstOrDefaultAsync();
        public async Task<HashSet<int>> ObterIdsCampeonatosDoTimeAceitoAsync(Guid timeClientAppId) {
            try {
                // 1. Busca todos os convites/inscrições onde o time está aceito
                var convitesAceitos = await _database.Table<Convite>()
                    .Where(c => c.TimeClientAppId == timeClientAppId
                               && c.Status == StatusConvite.Aceito
                               && c.Tipo == TipoConvite.InscricaoCampeonato)
                    .ToListAsync();

                if (!convitesAceitos.Any()) {
                    return new HashSet<int>();
                }

                // 2. Extrai os ClientAppIds dos Campeonatos
                var campeonatoClientAppIds = convitesAceitos
                    .Select(c => c.CampeonatoClientAppId)
                    .ToHashSet();

                // 3. Busca os objetos Campeonato usando os ClientAppIds
                var campeonatos = await _database.Table<Campeonato>()
                    .Where(c => campeonatoClientAppIds.Contains(c.ClientAppId))
                    .ToListAsync();

                // 4. Projeta para extrair apenas o ID
                var campeonatoIds = campeonatos
                    .Select(c => c.Id)
                    .ToList();

                return campeonatoIds.ToHashSet();

            } catch (Exception ex) {
                Debug.WriteLine($"[DatabaseService] ERRO ao obter IDs de campeonatos do time aceito: {ex.Message}");
                return new HashSet<int>();
            }
        }

        // --- Métodos de Time ---

        public Task<int> InserirTimeAsync(Time item) => _database.InsertAsync(item);
        public Task<List<Time>> ListarTimesAsync() => _database.Table<Time>().ToListAsync();
        public Task<int> AtualizarTimeAsync(Time item) => _database.UpdateAsync(item);
        // Os dois métodos abaixo fazem o mesmo, pode-se manter um, mas seguindo o original:
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

        // ObterTimesAceitosAsync para usar a tabela Convite
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

        // *******************************************************************
        // MÉTODOS CORRIGIDOS E UNIFICADOS: A sobrecarga de 2 argumentos.
        // *******************************************************************

        // 1. Sobrecarga com 1 argumento (o método que já existia)
        public async Task<List<Convite>> ObterConvitesPendentesAsync(Guid campeonatoClientAppId) {
            // Este método puxa TODOS os tipos de convite (Time e Arbitro) pendentes para o campeonato
            return await _database.Table<Convite>()
                                     .Where(c => c.CampeonatoClientAppId == campeonatoClientAppId &&
                                                 c.Status == StatusConvite.Pendente)
                                     .ToListAsync();
        }

        // 2. Sobrecarga com 2 argumentos (ADICIONADO para resolver o erro CS1501)
        public async Task<List<Convite>> ObterConvitesPendentesAsync(Guid campeonatoClientAppId, TipoConvite tipo) {
            Debug.WriteLine($"[DatabaseService] ObterConvitesPendentesAsync - Tipo: {tipo}");
            return await _database.Table<Convite>()
                                     .Where(c => c.CampeonatoClientAppId == campeonatoClientAppId &&
                                                 c.Status == StatusConvite.Pendente &&
                                                 c.Tipo == tipo)
                                     .ToListAsync();
        }

        public async Task<List<Convite>> ObterConvitesAceitosPorCampeonatoAsync(Guid campeonatoClientAppId) {
            try {
                // Busca convites ACEITOS para ÁRBITROS em um CAMPEONATO específico.
                return await _database.Table<Convite>()
                    .Where(c => c.CampeonatoClientAppId == campeonatoClientAppId
                                 && c.Status == StatusConvite.Aceito
                                 && c.Tipo == TipoConvite.InscricaoArbitro)
                    .ToListAsync();
            } catch (Exception ex) {
                Debug.WriteLine($"[DatabaseService] ERRO ao obter convites aceitos por campeonato (Árbitros): {ex.Message}");
                return new List<Convite>();
            }
        }

        // *******************************************************************
        // CORREÇÃO: Usando UsuarioClientAppId ao invés de ArbitroClientAppId
        // *******************************************************************
        public async Task<Convite?> ObterSolicitacaoPorArbitroECampeonatoAsync(string arbitroId, string campeonatoId, TipoConvite tipo) {

            if (!Guid.TryParse(arbitroId, out var arbitroGuid) || !Guid.TryParse(campeonatoId, out var campeonatoGuid)) {
                return null;
            }

            var solicitacao = await _database.Table<Convite>()
                .Where(s => s.UsuarioClientAppId == arbitroGuid && // <-- CORRIGIDO AQUI
                             s.CampeonatoClientAppId == campeonatoGuid &&
                             s.Tipo == tipo)
                .FirstOrDefaultAsync();

            return solicitacao;
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

        public Task<int> InserirEstatisticaAsync(EstatisticaPartida item) => _database.InsertAsync(item);
        public Task<List<EstatisticaPartida>> ListarEstatisticasAsync() => _database.Table<EstatisticaPartida>().ToListAsync();
        public Task<int> AtualizarEstatisticaAsync(EstatisticaPartida item) => _database.UpdateAsync(item);
        public Task<int> DeletarEstatisticaAsync(EstatisticaPartida item) => _database.DeleteAsync(item);

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

        public Task<List<EstatisticaPartida>> ObterEstatisticasPorJogoAsync(int jogoId) =>
            _database.Table<EstatisticaPartida>()
                .Where(e => e.JogoId == jogoId)
                .ToListAsync();

        public Task<List<EstatisticaPartida>> ObterEstatisticasPorAtletaAsync(int usuarioId) =>
            _database.Table<EstatisticaPartida>()
                .Where(e => e.UsuarioId == usuarioId)
                .ToListAsync();
    }
}