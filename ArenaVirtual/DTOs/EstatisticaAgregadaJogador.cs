namespace ArenaVirtual.Models {
    public class EstatisticaAgregadaJogador {
        public int UsuarioId { get; set; }
        public string NomeJogador { get; set; }
        public string ImagemPath { get; set; }
        public string NomeTime { get; set; }
        public string LogoTimeUrl { get; set; }
        public double JogosDisputados { get; set; }
        public double TotalPontos { get; set; }
        public double TotalAssistencias { get; set; }
        public double TotalRebotes { get; set; }
        public double TotalRoubos { get; set; }
        public double TotalBloqueios { get; set; }
        public double TotalTurnovers { get; set; }
        public double TotalFaltas { get; set; }
        public double Arremessos2PontosConvertidos { get; set; }
        public double Arremessos2PontosTentados { get; set; }
        public double Arremessos3PontosConvertidos { get; set; }
        public double Arremessos3PontosTentados { get; set; }
        public double LancesLivresConvertidos { get; set; }
        public double LancesLivresTentados { get; set; }
    }
}