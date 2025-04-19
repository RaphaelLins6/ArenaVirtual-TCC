namespace ArenaVirtuall;

public partial class AppShell : Shell {
    public AppShell() {
        InitializeComponent();

        // Rotas (opcional, se quiser navegar com GoToAsync)
        Routing.RegisterRoute(nameof(Views.TelaInicial), typeof(Views.TelaInicial));
        Routing.RegisterRoute(nameof(Views.CriarCampeonato), typeof(Views.CriarCampeonato));
        Routing.RegisterRoute(nameof(Views.PaginaAtleta), typeof(Views.PaginaAtleta));
    }
}
