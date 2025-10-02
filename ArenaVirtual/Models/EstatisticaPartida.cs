using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArenaVirtual.Models {

    public class EstatisticaPartida : ObservableObject, ISyncable {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        // Chave estrangeira para o Atleta/Usuário (Quem fez a estatística)
        public int UsuarioId { get; set; }

        // Chave estrangeira para o Jogo
        public int JogoId { get; set; }

        // Chave estrangeira para o Time (Para facilitar buscas no contexto do time)
        public int TimeId { get; set; }

        // ESTATÍSTICAS BÁSICAS (Mantendo o que você já tinha)
        public int Pontos { get; set; } = 0;
        public int Rebotes { get; set; } = 0;
        public int Assistencias { get; set; } = 0;

        // NOVAS ESTATÍSTICAS DE BASQUETE (Inteiros)
        public int Roubos { get; set; } = 0;
        public int Bloqueios { get; set; } = 0;
        public int Faltas { get; set; } = 0;
        public int Turnovers { get; set; } = 0;

        // Arremessos Tentados/Convertidos
        public int Arremessos2PontosConvertidos { get; set; } = 0;
        public int Arremessos2PontosTentados { get; set; } = 0;

        public int Arremessos3PontosConvertidos { get; set; } = 0;
        public int Arremessos3PontosTentados { get; set; } = 0;

        public int LancesLivresConvertidos { get; set; } = 0;
        public int LancesLivresTentados { get; set; } = 0;

        // Controle de Sincronização
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        public EstatisticaPartida() {
            IsSynced = false;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}