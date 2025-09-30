using ArenaVirtual.ViewModels.CampeonatoPage;

namespace ArenaVirtual.Views.CampeonatoPage;

public partial class TimesCadastradosPage : ContentPage {
    public TimesCadastradosPage(TimesCadastradosViewModel vm) {
        
        InitializeComponent();

        BindingContext = vm;
    }
}