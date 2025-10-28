using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models // Namespace corrigido para API
{
    // Esta classe é um View Model/DTO, não uma entidade EF Core
    public class EstatisticaAgregadaJogador {
        // Chave primária não necessária para View Model
        public int UsuarioId { get; set; }
        public string NomeJogador { get; set; } = string.Empty;
        public string ImagemPath { get; set; } = string.Empty;
        public string NomeTime { get; set; } = string.Empty;
        public string LogoTimeUrl { get; set; } = string.Empty;

        // Estatísticas
        public int JogosDisputados { get; set; }
        public int TotalPontos { get; set; }
        public int TotalAssistencias { get; set; }
        public int TotalRebotes { get; set; }
        public int TotalRoubos { get; set; }
        public int TotalBloqueios { get; set; }
        public int TotalTurnovers { get; set; }
        public int TotalFaltas { get; set; }

        public int Arremessos2PontosConvertidos { get; set; }
        public int Arremessos2PontosTentados { get; set; }
        public int Arremessos3PontosConvertidos { get; set; }
        public int Arremessos3PontosTentados { get; set; }
        public int LancesLivresConvertidos { get; set; }
        public int LancesLivresTentados { get; set; }
    }
}
