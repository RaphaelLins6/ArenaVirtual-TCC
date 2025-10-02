using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    // Se você tiver uma classe 'Arbitro' no seu Models, use-a, caso contrário, use 'Usuario'
    public partial class SolicitacaoArbitroItemViewModel : ObservableObject {

        public Convite SolicitacaoOriginal { get; }
        public Usuario ArbitroSolicitante { get; } // Assumindo que você usa a model 'Usuario' para o Arbitro

        public string NomeArbitro => ArbitroSolicitante?.Nome;
        public string EmailArbitro => ArbitroSolicitante?.Email;
        // Se você tiver uma URL de foto/imagem para o Árbitro, adicione aqui.
        // public string FotoArbitroUrl => ArbitroSolicitante?.FotoUrl; 

        public SolicitacaoArbitroItemViewModel(Convite solicitacao, Usuario arbitro) {
            SolicitacaoOriginal = solicitacao;
            ArbitroSolicitante = arbitro;
        }
    }
}