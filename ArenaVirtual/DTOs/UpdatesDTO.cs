using System.Collections.Generic;
using System.Text.Json;

namespace ArenaVirtual.DTOs {
    
    public class UpdatesDTO {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new Dictionary<string, JsonElement>();
        public List<UsuarioSyncDto>? Usuarios { get; set; }
        public List<CampeonatoSyncDto>? Campeonatos { get; set; }
        public List<TimeSyncDto>? Times { get; set; }
        public List<ConviteSyncDto>? Convites { get; set; }
        public List<JogoSyncDto>? Jogos { get; set; }
        public List<UsuarioCampeonatoFavoritoSyncDto>? UsuarioCampeonatoFavoritos { get; set; }
        public List<RodadaDeJogosSyncDto>? RodadasDeJogos { get; set; }
        public List<InscricaoSyncDto>? Inscricoes { get; set; }
        public List<EstatisticaPartidaSyncDto>? EstatisticasPartidas { get; set; }
        public List<AvaliacaoArbitroSyncDto>? AvaliacoesArbitros { get; set; }
        public List<CampanhaPatrocinioSyncDto>? CampanhasPatrocinios { get; set; }
        public List<PropostaPatrocinioSyncDto>? PropostasPatrocinios { get; set; }
    }
}