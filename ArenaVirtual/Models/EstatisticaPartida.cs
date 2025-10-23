using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArenaVirtual.Models {

    public class EstatisticaPartida : ObservableObject, ISyncable {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public int UsuarioId { get; set; }

        public int JogoId { get; set; }

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

        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        public EstatisticaPartida() {
            IsSynced = false;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}