namespace Snap;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registramos TODAS las rutas para navegación

        Routing.RegisterRoute("Login", typeof(Paginas.LoginPage));
        Routing.RegisterRoute("PublicacionPage", typeof(Paginas.PublicacionPage));
        Routing.RegisterRoute("Registro", typeof(Paginas.RegistroPage));

    }
}
