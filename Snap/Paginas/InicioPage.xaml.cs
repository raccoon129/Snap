using Snap.Servicios;
using System.Collections.ObjectModel;

namespace Snap.Paginas;

public partial class InicioPage : ContentPage
{
    private readonly ApiService _apiService;
    private ObservableCollection<PublicacionViewModel> _publicaciones = new ObservableCollection<PublicacionViewModel>();

    public InicioPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        ListaPublicaciones.ItemsSource = _publicaciones;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDatosUsuario();
        await CargarPublicaciones();
    }

    private async Task CargarDatosUsuario()
    {
        var sesion = await _apiService.ObtenerSesionActual();
        if (sesion.SesionActiva && sesion.Usuario != null)
        {
            LblUsuario.Text = $"¡Hola, {sesion.Usuario.nombre_usuario}!";
        }
        else
        {
            // Si no hay sesión activa, volver al login
            await Shell.Current.GoToAsync("///Login");
        }
    }

    private async Task CargarPublicaciones()
    {
        // En una implementación real, aquí cargaríamos las publicaciones desde la API
        // Por ahora usamos datos de ejemplo
        _publicaciones.Clear();

        // Simulamos una carga
        await Task.Delay(1000);

        // Datos de prueba
        _publicaciones.Add(new PublicacionViewModel
        {
            NombreUsuario = "carlos_dev",
            UrlFotoPerfil = "https://randomuser.me/api/portraits/men/32.jpg",
            Ubicacion = "Madrid, España",
            UrlFoto = "https://picsum.photos/500/500?random=1",
            NumeroLikes = 120,
            Descripcion = "¡Disfrutando del día!",
            FechaPublicacion = "Hace 2 horas"
        });

        _publicaciones.Add(new PublicacionViewModel
        {
            NombreUsuario = "maria_photo",
            UrlFotoPerfil = "https://randomuser.me/api/portraits/women/44.jpg",
            Ubicacion = "Barcelona, España",
            UrlFoto = "https://picsum.photos/500/500?random=2",
            NumeroLikes = 85,
            Descripcion = "Atardecer perfecto 🌇",
            FechaPublicacion = "Hace 5 horas"
        });
    }

    private async void RefrescarContenido_Refreshing(object sender, EventArgs e)
    {
        await CargarPublicaciones();
        RefrescarContenido.IsRefreshing = false;
    }
}

// Modelo de vista para publicaciones
public class PublicacionViewModel
{
    public string NombreUsuario { get; set; }
    public string UrlFotoPerfil { get; set; }
    public string Ubicacion { get; set; }
    public string UrlFoto { get; set; }
    public int NumeroLikes { get; set; }
    public string Descripcion { get; set; }
    public string FechaPublicacion { get; set; }
}