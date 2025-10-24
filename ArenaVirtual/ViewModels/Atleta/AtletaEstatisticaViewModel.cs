using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.ViewModels.Atleta {
    public partial class AtletaEstatisticaViewModel : ObservableObject {
        [ObservableProperty]
        private string nomeAtleta = "Carregando...";

        [ObservableProperty]
        private double mediaPontos = 0.0; 
        [ObservableProperty]
        private double mediaRebotes = 0.0; 
        [ObservableProperty]
        private double mediaAssistencias = 0.0; 

        [ObservableProperty]
        private double mediaRoubos = 0.0; 
        [ObservableProperty]
        private double mediaBloqueios = 0.0; 
        [ObservableProperty]
        private double mediaFaltas = 0.0; 
        [ObservableProperty]
        private double mediaTurnovers = 0.0; 

        [ObservableProperty]
        private double percentual2P = 0.0; 
        [ObservableProperty]
        private double percentual3P = 0.0; 
        [ObservableProperty]
        private double percentualLancesLivres = 0.0; 
    }
}