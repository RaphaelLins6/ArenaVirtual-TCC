using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System; // Adicione este using se não estiver presente (para DateTime, etc.)

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    // Classe para exibir as estatísticas AGREGADAS de um time na nova aba
    public partial class TimeEstatisticaViewModel : ObservableObject {

        public Time Time { get; }
        public string NomeTime => Time.Nome;
        public string logoUrl => Time.LogoUrl;
        public int JogosDisputados { get; set; } = 0; // Necessário para o cálculo da média

        public TimeEstatisticaViewModel(Time time) {
            Time = time;
        }

        // =========================================================
        // PROPRIEDADES DE SOMA (TOTAIS) - Baseado no modelo EstatisticaPartida
        // =========================================================
        public int TotalPontos { get; set; }
        public int TotalRebotes { get; set; }
        public int TotalAssistencias { get; set; }
        public int TotalRoubos { get; set; }
        public int TotalBloqueios { get; set; }
        public int TotalTurnovers { get; set; }
        public int TotalFaltas { get; set; }

        // Totais de Arremessos 2 Pontos
        public int TotalArremessos2PontosConvertidos { get; set; }
        public int TotalArremessos2PontosTentados { get; set; }

        // Totais de Arremessos 3 Pontos
        public int TotalArremessos3PontosConvertidos { get; set; }
        public int TotalArremessos3PontosTentados { get; set; }

        // Totais de Lances Livres
        public int TotalLancesLivresConvertidos { get; set; }
        public int TotalLancesLivresTentados { get; set; }

        // Adicione aqui a soma de todas as tentativas e conversões para um cálculo geral (FG)
        public int TotalArremessosConvertidos => TotalArremessos2PontosConvertidos + TotalArremessos3PontosConvertidos;
        public int TotalArremessosTentados => TotalArremessos2PontosTentados + TotalArremessos3PontosTentados;

        // =========================================================
        // PROPRIEDADES CALCULADAS (MÉDIAS POR JOGO e PERCENTUAIS)
        // Usamos double/float para as médias.
        // =========================================================

        // Médias por Jogo (P/J, R/J, A/J, etc.)
        private double SafeDivision(int total) => JogosDisputados > 0 ? (double)total / JogosDisputados : 0.0;

        public double MediaPontos => SafeDivision(TotalPontos);
        public double MediaRebotes => SafeDivision(TotalRebotes);
        public double MediaAssistencias => SafeDivision(TotalAssistencias);
        public double MediaRoubos => SafeDivision(TotalRoubos);
        public double MediaBloqueios => SafeDivision(TotalBloqueios);
        public double MediaTurnovers => SafeDivision(TotalTurnovers);
        public double MediaFaltas => SafeDivision(TotalFaltas);

        // Médias dos Arremessos (Tentados e Convertidos por Jogo)
        public double MediaArremessos2PConvertidos => SafeDivision(TotalArremessos2PontosConvertidos);
        public double MediaArremessos2PTentados => SafeDivision(TotalArremessos2PontosTentados);

        public double MediaArremessos3PConvertidos => SafeDivision(TotalArremessos3PontosConvertidos);
        public double MediaArremessos3PTentados => SafeDivision(TotalArremessos3PontosTentados);

        public double MediaLancesLivresConvertidos => SafeDivision(TotalLancesLivresConvertidos);
        public double MediaLancesLivresTentados => SafeDivision(TotalLancesLivresTentados);

        // Percentuais de Conversão (o foco do display)
        private double SafePercent(int convertido, int tentado) => tentado > 0
            ? (double)convertido / tentado * 100.0 // Multiplique por 100.0 se quiser o valor em porcentagem (ex: 45.5)
            : 0.0;

        // Percentual de Arremessos de Campo (FG%)
        public double PercentualArremessosCampo => SafePercent(TotalArremessosConvertidos, TotalArremessosTentados);

        // Percentual de Arremessos de 2 Pontos (2P%)
        public double Percentual2P => SafePercent(TotalArremessos2PontosConvertidos, TotalArremessos2PontosTentados);

        // Percentual de Arremessos de 3 Pontos (3P%)
        public double Percentual3P => SafePercent(TotalArremessos3PontosConvertidos, TotalArremessos3PontosTentados);

        // Percentual de Lances Livres (FT%)
        public double PercentualLancesLivres => SafePercent(TotalLancesLivresConvertidos, TotalLancesLivresTentados);

    }
}