using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class JogadorEstatisticaViewModel : ObservableObject {
        public int Id { get; set; }

        [ObservableProperty]
        private int posicao;

        [ObservableProperty]
        private string? fotoUrl;

        [ObservableProperty]
        private string? nomeJogador;

        [ObservableProperty]
        private string? nomeTime;

        [ObservableProperty]
        private string? logoTimeUrl;

        // Propriedade para exibir o valor da estatística principal selecionada (formatada)
        [ObservableProperty]
        private string? valorEstatisticaPrincipal;

        // Estatísticas individuais
        public int Pontos { get; set; }
        public int Assistencias { get; set; }
        public int Rebotes { get; set; }
        public int Roubos { get; set; }
        public int Bloqueios { get; set; }
        public int Turnovers { get; set; }
        public int Faltas { get; set; }

        // Arremessos de 2 pontos
        public int Arremessos2PontosConvertidos { get; set; }
        public int Arremessos2PontosTentados { get; set; }
        public double Percentual2Pontos => Arremessos2PontosTentados > 0 ? (double)Arremessos2PontosConvertidos / Arremessos2PontosTentados : 0.0;

        // Arremessos de 3 pontos
        public int Arremessos3PontosConvertidos { get; set; }
        public int Arremessos3PontosTentados { get; set; }
        public double Percentual3Pontos => Arremessos3PontosTentados > 0 ? (double)Arremessos3PontosConvertidos / Arremessos3PontosTentados : 0.0;

        // Lances Livres
        public int LancesLivresConvertidos { get; set; }
        public int LancesLivresTentados { get; set; }
        public double PercentualLancesLivres => LancesLivresTentados > 0 ? (double)LancesLivresConvertidos / LancesLivresTentados : 0.0;
    }
}
