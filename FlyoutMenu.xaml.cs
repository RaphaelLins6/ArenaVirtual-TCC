using System;
using ArenaVirtuall.Views;

namespace ArenaVirtuall;

public partial class FlyoutMenu : FlyoutPage
{
	public FlyoutMenu()
	{
		InitializeComponent();
        Detail = new NavigationPage(new TelaInicial()); // Página inicial
        IsPresented = false; // Fecha o menu ao iniciar
    }
}