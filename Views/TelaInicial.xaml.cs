namespace ArenaVirtuall.Views;

public partial class TelaInicial : ContentPage
{
    public bool IsRotated { get; set; } // Add this property

    public TelaInicial()
	{
        InitializeComponent();

        BindingContext = new TelaInicialViewModel();
        IsRotated = false; // Initialize the property
    }
}