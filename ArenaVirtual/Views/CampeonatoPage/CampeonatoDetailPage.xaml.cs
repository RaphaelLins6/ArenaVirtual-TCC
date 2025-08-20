using ArenaVirtual.Models;
using ArenaVirtual.ViewModels.CampeonatoPage;

namespace ArenaVirtual.Views.CampeonatoPage {
    public partial class CampeonatoDetailPage : ContentPage {
        public CampeonatoDetailPage(Campeonato campeonato) {
            InitializeComponent();
            BindingContext = new CampeonatoDetailViewModel(campeonato);
        }
    }
}