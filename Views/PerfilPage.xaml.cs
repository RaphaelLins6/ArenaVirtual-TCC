using ArenaVirtual.Models;
using ArenaVirtual.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {
        public PerfilPage(Usuario usuarioLogado) {
            InitializeComponent();

            // Correção: use o ServiceProvider do MauiContext para resolver o PerfilViewModel, passando o usuário logado
            if (this.Handler?.MauiContext?.Services != null) {
                var serviceProvider = this.Handler.MauiContext.Services;
                BindingContext = ActivatorUtilities.CreateInstance<PerfilViewModel>(serviceProvider, usuarioLogado);
            } else {
                System.Diagnostics.Debug.WriteLine("Erro: Handler ou MauiContext é nulo na PerfilPage.");
            }
        }
    }
}