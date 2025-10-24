using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System; 

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    public partial class TimeEstatisticaViewModel : ObservableObject {

        public Time Time { get; }
        public string NomeTime => Time.Nome;
        public string logoUrl => Time.LogoUrl;
        public int JogosDisputados { get; set; } = 0; 

        public TimeEstatisticaViewModel(Time time) {
            Time = time;
        }

        public int TotalPontos { get; set; }
        public int TotalRebotes { get; set; }
        public int TotalAssistencias { get; set; }
        public int TotalRoubos { get; set; }
        public int TotalBloqueios { get; set; }
        public int TotalTurnovers { get; set; }
        public int TotalFaltas { get; set; }

        public int TotalArremessos2PontosConvertidos { get; set; }
        public int TotalArremessos2PontosTentados { get; set; }

        public int TotalArremessos3PontosConvertidos { get; set; }
        public int TotalArremessos3PontosTentados { get; set; }

        public int TotalLancesLivresConvertidos { get; set; }
        public int TotalLancesLivresTentados { get; set; }

        public int TotalArremessosConvertidos => TotalArremessos2PontosConvertidos + TotalArremessos3PontosConvertidos;
        public int TotalArremessosTentados => TotalArremessos2PontosTentados + TotalArremessos3PontosTentados;

        private double SafeDivision(int total) => JogosDisputados > 0 ? (double)total / JogosDisputados : 0.0;

        public double MediaPontos => SafeDivision(TotalPontos);
        public double MediaRebotes => SafeDivision(TotalRebotes);
        public double MediaAssistencias => SafeDivision(TotalAssistencias);
        public double MediaRoubos => SafeDivision(TotalRoubos);
        public double MediaBloqueios => SafeDivision(TotalBloqueios);
        public double MediaTurnovers => SafeDivision(TotalTurnovers);
        public double MediaFaltas => SafeDivision(TotalFaltas);

        public double MediaArremessos2PConvertidos => SafeDivision(TotalArremessos2PontosConvertidos);
        public double MediaArremessos2PTentados => SafeDivision(TotalArremessos2PontosTentados);

        public double MediaArremessos3PConvertidos => SafeDivision(TotalArremessos3PontosConvertidos);
        public double MediaArremessos3PTentados => SafeDivision(TotalArremessos3PontosTentados);

        public double MediaLancesLivresConvertidos => SafeDivision(TotalLancesLivresConvertidos);
        public double MediaLancesLivresTentados => SafeDivision(TotalLancesLivresTentados);

        private double SafePercent(int convertido, int tentado) => tentado > 0
            ? (double)convertido / tentado * 100.0 
            : 0.0;

        public double PercentualArremessosCampo => SafePercent(TotalArremessosConvertidos, TotalArremessosTentados);

        public double Percentual2P => SafePercent(TotalArremessos2PontosConvertidos, TotalArremessos2PontosTentados);

        public double Percentual3P => SafePercent(TotalArremessos3PontosConvertidos, TotalArremessos3PontosTentados);

        public double PercentualLancesLivres => SafePercent(TotalLancesLivresConvertidos, TotalLancesLivresTentados);

    }
}