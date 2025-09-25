using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    // [ObservableObject] não é necessário aqui, pois a classe não contém propriedades observáveis
    public partial class SolicitacaoTimeItemViewModel {

        public Convite ConviteOriginal { get; }

        public Time TimeSolicitante { get; }

        public string NomeTime => TimeSolicitante?.Nome ?? "Nome indisponível";
        public string ImagemTime => TimeSolicitante?.LogoUrl ?? "default_team_image.png";

        public SolicitacaoTimeItemViewModel(Convite convite, Time time) {
            // **Correção:** O construtor agora recebe um objeto do tipo Convite
            ConviteOriginal = convite;
            TimeSolicitante = time;
        }
    }
}