namespace ArenaVirtuall
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Agora o app usa Shell
            MainPage = new AppShell();
        }
    }
}