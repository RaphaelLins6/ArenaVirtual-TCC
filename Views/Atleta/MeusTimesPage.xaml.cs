// MeusTimesPage.xaml.cs
using ArenaVirtual.ViewModels.Atleta;
using Microsoft.Extensions.DependencyInjection; // Adicione este using

namespace ArenaVirtual.Views.Atleta {
    public partial class MeusTimesPage : ContentPage {
        public MeusTimesPage() {
            InitializeComponent();
            BindingContext = App.Current.Handler.MauiContext.Services.GetRequiredService<MeuTimePageViewModel>();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is MeuTimePageViewModel vm) {
            }
        }
    }
}