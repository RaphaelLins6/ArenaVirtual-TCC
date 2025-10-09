using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.ViewModels.Arbitro {

    public partial class SolicitacaoArbitroItemViewModel : ObservableObject {

        public Convite SolicitacaoOriginal { get; }
        public Usuario ArbitroSolicitante { get; }
        public string NomeArbitro => ArbitroSolicitante?.Nome;
        public string EmailArbitro => ArbitroSolicitante?.Email;
        public string FotoArbitroUrl => ArbitroSolicitante?.ImagemPath;

        public SolicitacaoArbitroItemViewModel(Convite solicitacao, Usuario arbitro) {
            SolicitacaoOriginal = solicitacao;
            ArbitroSolicitante = arbitro;
        }

        public SolicitacaoArbitroItemViewModel(Usuario arbitro) {
            SolicitacaoOriginal = null; // Não precisamos do Convite para árbitros já inscritos
            ArbitroSolicitante = arbitro ?? throw new ArgumentNullException(nameof(arbitro));
        }
    }
}