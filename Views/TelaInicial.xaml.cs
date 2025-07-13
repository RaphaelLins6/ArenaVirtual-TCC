using ArenaVirtuall.ViewModels;
using ArenaVirtuall.Models;

namespace ArenaVirtuall.Views;

public partial class TelaInicial : ContentPage
{
    public bool IsRotated { get; set; } // Add this property

    public TelaInicial()
	{
        InitializeComponent();

        BindingContext = new TelaInicialViewModel();
        
    }

    private void OnFavoriteButtonClicked(object sender, EventArgs e) {
        if (sender is ImageButton button && button.BindingContext is Campeonato campeonato) {
            var viewModel = BindingContext as TelaInicialViewModel;

            if (viewModel != null) {
                if (campeonato.IsFavorito) {
                    // Remover dos favoritos
                    campeonato.IsFavorito = false;
                    viewModel.Favoritos.Remove(campeonato);
                    viewModel.Campeonatos.Add(campeonato);
                } else {
                    // Adicionar aos favoritos
                    campeonato.IsFavorito = true;
                    viewModel.Campeonatos.Remove(campeonato);
                    viewModel.Favoritos.Add(campeonato);
                }
            }
        }
    }
}