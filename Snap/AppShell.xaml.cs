namespace Snap;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registramos TODAS las rutas para navegación
        Routing.RegisterRoute("Login", typeof(Paginas.LoginPage));
        Routing.RegisterRoute("Inicio", typeof(Paginas.InicioPage));
        Routing.RegisterRoute("Explorar", typeof(Paginas.InicioPage)); // Temporalmente usando InicioPage
        Routing.RegisterRoute("Camara", typeof(Paginas.InicioPage));   // Temporalmente usando InicioPage
        Routing.RegisterRoute("Favoritos", typeof(Paginas.InicioPage)); // Temporalmente usando InicioPage
        Routing.RegisterRoute("Perfil", typeof(Paginas.InicioPage));    // Temporalmente usando InicioPage
    }
}
