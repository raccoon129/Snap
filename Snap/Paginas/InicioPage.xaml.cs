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
        if (!sesion.SesionActiva || sesion.Usuario == null)
        {
            // Si no hay sesión activa, volver al login
            await Shell.Current.GoToAsync("//Login");
        }
    }

    private async Task CargarPublicaciones()
    {
        try
        {
            _publicaciones.Clear();

            // Simulamos carga con un delay
            await Task.Delay(1000);

            // Datos de prueba según el wireframe
            _publicaciones.Add(new PublicacionViewModel
            {
                NombreUsuario = "@nombre_random",
                UrlFotoPerfil = "https://randomuser.me/api/portraits/men/32.jpg",
                TiempoPublicacion = "Hace 1 hora",
                UrlFoto = "https://picsum.photos/500/500?random=1",
                NumeroLikes = 3,
                NumeroComentarios = 1,
                Descripcion = "Aquinombre el tag..."
            });

            _publicaciones.Add(new PublicacionViewModel
            {
                NombreUsuario = "@nombre_random",
                UrlFotoPerfil = "https://randomuser.me/api/portraits/men/44.jpg",
                TiempoPublicacion = "Hace 1 hora",
                UrlFoto = "https://picsum.photos/500/500?random=2",
                NumeroLikes = 5,
                NumeroComentarios = 2,
                Descripcion = "Aquinombre el tag..."
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las publicaciones: {ex.Message}", "OK");
        }
    }

    private async void RefrescarContenido_Refreshing(object sender, EventArgs e)
    {
        await CargarPublicaciones();
        RefrescarContenido.IsRefreshing = false;
    }
}

public class PublicacionViewModel
{
    public string NombreUsuario { get; set; } = "";
    public string UrlFotoPerfil { get; set; } = "";
    public string TiempoPublicacion { get; set; } = "";
    public string UrlFoto { get; set; } = "";
    public int NumeroLikes { get; set; }
    public int NumeroComentarios { get; set; }
    public string Descripcion { get; set; } = "";
    public string Ubicacion { get; set; } = "";
    public int Id { get; internal set; }
}