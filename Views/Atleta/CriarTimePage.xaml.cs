using ArenaVirtual.ViewModels.Atleta;
using ArenaVirtual.Services;

namespace ArenaVirtual.Views.Atleta {
    public partial class CriarTimePage : ContentPage {
        public CriarTimePage(TimeService timeService) {
            InitializeComponent();
            BindingContext = new CriarTimePageViewModel(timeService);
        }
    }
}