namespace ArenaVirtual.Models {
    public class EstatisticaAgregadaJogador {
        public int UsuarioId { get; set; }
        public string NomeJogador { get; set; }
        public string ImagemPath { get; set; }
        public string NomeTime { get; set; }
        public string LogoTimeUrl { get; set; }

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