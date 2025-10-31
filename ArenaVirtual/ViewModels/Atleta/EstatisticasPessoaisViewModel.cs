using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.ViewModels.Atleta {
    public partial class EstatisticasPessoaisViewModel : ObservableObject {
        private readonly SessaoService _sessaoService;
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private AtletaEstatisticaViewModel estatisticasAtleta = new();

        [ObservableProperty]
        private ImageSource fotoAtletaSource;

        [ObservableProperty]
        private bool estaCarregando = true;

        public EstatisticasPessoaisViewModel() { }

        public EstatisticasPessoaisViewModel(SessaoService sessaoService, DatabaseService databaseService) {
            _sessaoService = sessaoService;
            _databaseService = databaseService;
            _ = CarregarEstatisticasCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task CarregarEstatisticas() {
            EstaCarregando = true;

            var usuarioAtual = _sessaoService.GetUsuarioAtual();

            if (usuarioAtual == null || usuarioAtual.Id <= 0) {
                EstatisticasAtleta.NomeAtleta = "Erro: Usuário não logado.";
                EstaCarregando = false;
                return;
            }

            if (!string.IsNullOrEmpty(usuarioAtual.ImagemPath)) {
                FotoAtletaSource = ImageSource.FromFile(usuarioAtual.ImagemPath);
            } else {
                FotoAtletaSource = ImageSource.FromFile("placeholder.png");
            }

            List<EstatisticaPartida> todasEstatisticas = await _databaseService.ObterEstatisticasPorAtletaAsync(usuarioAtual.Id);

            int totalJogos = todasEstatisticas
                .Select(e => e.JogoId)
                .Distinct()
                .Count();

            var estatisticasValidas = todasEstatisticas.Where(e => e.JogoId > 0).ToList();

            if (totalJogos == 0) {
                EstatisticasAtleta.NomeAtleta = usuarioAtual.Nome;
                EstaCarregando = false;
                return;
            }

            double totalPontos = estatisticasValidas.Sum(e => e.Pontos);
            double totalRebotes = estatisticasValidas.Sum(e => e.Rebotes);
            double totalAssistencias = estatisticasValidas.Sum(e => e.Assistencias);
            double totalRoubos = estatisticasValidas.Sum(e => e.Roubos);
            double totalBloqueios = estatisticasValidas.Sum(e => e.Bloqueios);
            double totalFaltas = estatisticasValidas.Sum(e => e.Faltas);
            double totalTurnovers = estatisticasValidas.Sum(e => e.Turnovers);

            double total2PC = estatisticasValidas.Sum(e => e.Arremessos2PontosConvertidos);
            double total2PT = estatisticasValidas.Sum(e => e.Arremessos2PontosTentados);
            double total3PC = estatisticasValidas.Sum(e => e.Arremessos3PontosConvertidos);
            double total3PT = estatisticasValidas.Sum(e => e.Arremessos3PontosTentados);
            double totalLLC = estatisticasValidas.Sum(e => e.LancesLivresConvertidos);
            double totalLLT = estatisticasValidas.Sum(e => e.LancesLivresTentados);


            EstatisticasAtleta.NomeAtleta = usuarioAtual.Nome;

            EstatisticasAtleta.MediaPontos = totalPontos / totalJogos;
            EstatisticasAtleta.MediaRebotes = totalRebotes / totalJogos;
            EstatisticasAtleta.MediaAssistencias = totalAssistencias / totalJogos;
            EstatisticasAtleta.MediaRoubos = totalRoubos / totalJogos;
            EstatisticasAtleta.MediaBloqueios = totalBloqueios / totalJogos;
            EstatisticasAtleta.MediaFaltas = totalFaltas / totalJogos;
            EstatisticasAtleta.MediaTurnovers = totalTurnovers / totalJogos;

            EstatisticasAtleta.Percentual2P = total2PT > 0 ? (total2PC / total2PT) * 100 : 0.0;
            EstatisticasAtleta.Percentual3P = total3PT > 0 ? (total3PC / total3PT) * 100 : 0.0;
            EstatisticasAtleta.PercentualLancesLivres = totalLLT > 0 ? (totalLLC / totalLLT) * 100 : 0.0;

            EstaCarregando = false;
        }
    }
}