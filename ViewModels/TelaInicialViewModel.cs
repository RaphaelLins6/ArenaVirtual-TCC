using ArenaVirtuall.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArenaVirtuall.ViewModels {

    public class TelaInicialViewModel {
        public ObservableCollection<Campeonato> Campeonatos { get; set; }
        public ObservableCollection<Campeonato> Favoritos { get; set; }

        public TelaInicialViewModel() {
            // Inicializar listas
            Campeonatos = new ObservableCollection<Campeonato>
            {
                new Campeonato { Nome = "Campeonato 1", Imagem = "bola.png", IsFavorito = false },
                new Campeonato { Nome = "Campeonato 2", Imagem = "bola.png", IsFavorito = false }
            };

            Favoritos = new ObservableCollection<Campeonato>();
        }
    }
}