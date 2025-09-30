// EM: ArenaVirtual.ViewModels.CampeonatoPage/SolicitacaoTimeItemViewModel.cs
using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    // A classe SolicitacaoTimeItemViewModel deve herdar de ObservableObject
    // se você planeja adicionar comandos [RelayCommand] ou propriedades observáveis no futuro.
    public partial class SolicitacaoTimeItemViewModel : ObservableObject {

        // CORREÇÃO: Propriedade agora é do tipo Convite
        public Convite SolicitacaoOriginal { get; }

        public Time TimeSolicitante { get; }

        public string NomeTime => TimeSolicitante?.Nome ?? "Nome indisponível";
        public string ImagemTime => TimeSolicitante?.LogoUrl ?? "default_team_image.png";

        // CORREÇÃO: O construtor agora aceita o tipo Convite (Resolve CS1503)
        public SolicitacaoTimeItemViewModel(Convite solicitacao, Time time) {
            SolicitacaoOriginal = solicitacao;
            TimeSolicitante = time;
        }
    }
}