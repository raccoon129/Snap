namespace Snap
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Light; // Establecer el tema de la aplicación
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // Opcional: configurar la ventana
            window.Title = "Snap";

            return window;
        }
    }
}