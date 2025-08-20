using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class CampeonatoDetailViewModel : ObservableObject {
        [ObservableProperty]
        private Campeonato campeonato;

        public CampeonatoDetailViewModel(Campeonato campeonato) {
            Campeonato = campeonato;
        }
    }
}