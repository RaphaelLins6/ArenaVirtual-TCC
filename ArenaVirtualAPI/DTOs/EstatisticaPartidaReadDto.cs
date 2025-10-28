using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar dados
    public class EstatisticaPartidaReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public int TimeId { get; set; }

        public int Pontos { get; set; }
        public int Rebotes { get; set; }
        public int Assistencias { get; set; }
        public int Roubos { get; set; }
        public int Bloqueios { get; set; }
        public int Faltas { get; set; }
        public int Turnovers { get; set; }

        public int Arremessos2PontosConvertidos { get; set; }
        public int Arremessos2PontosTentados { get; set; }
        public int Arremessos3PontosConvertidos { get; set; }
        public int Arremessos3PontosTentados { get; set; }
        public int LancesLivresConvertidos { get; set; }
        public int LancesLivresTentados { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    // DTO para criar/atualizar dados via API
    public class EstatisticaPartidaCreateUpdateDto {
        // Necessário para o upsert via ClientAppId no POST (opcional)
        public Guid? ClientAppId { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        [Required]
        public int JogoId { get; set; }
        [Required]
        public int TimeId { get; set; }

        public int Pontos { get; set; } = 0;
        public int Rebotes { get; set; } = 0;
        public int Assistencias { get; set; } = 0;
        public int Roubos { get; set; } = 0;
        public int Bloqueios { get; set; } = 0;
        public int Faltas { get; set; } = 0;
        public int Turnovers { get; set; } = 0;

        public int Arremessos2PontosConvertidos { get; set; } = 0;
        public int Arremessos2PontosTentados { get; set; } = 0;
        public int Arremessos3PontosConvertidos { get; set; } = 0;
        public int Arremessos3PontosTentados { get; set; } = 0;
        public int LancesLivresConvertidos { get; set; } = 0;
        public int LancesLivresTentados { get; set; } = 0;
    }
}