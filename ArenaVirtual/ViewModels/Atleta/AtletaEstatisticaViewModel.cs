using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.ViewModels.Atleta {
    // Classe para representar as estatísticas agregadas (médias por jogo) de um jogador
    public partial class AtletaEstatisticaViewModel : ObservableObject {
        [ObservableProperty]
        private string nomeAtleta = "Carregando...";

        // Métricas básicas
        [ObservableProperty]
        private double mediaPontos = 0.0; // P/J
        [ObservableProperty]
        private double mediaRebotes = 0.0; // RBT/J
        [ObservableProperty]
        private double mediaAssistencias = 0.0; // AST/J

        // Novas Métricas (Médias por Jogo)
        [ObservableProperty]
        private double mediaRoubos = 0.0; // ROU/J
        [ObservableProperty]
        private double mediaBloqueios = 0.0; // BLO/J
        [ObservableProperty]
        private double mediaFaltas = 0.0; // F/J
        [ObservableProperty]
        private double mediaTurnovers = 0.0; // T/J

        // Percentuais (Formatados para exibição)
        [ObservableProperty]
        private double percentual2P = 0.0; // 2P%
        [ObservableProperty]
        private double percentual3P = 0.0; // 3P%
        [ObservableProperty]
        private double percentualLancesLivres = 0.0; // LL%
    }
}