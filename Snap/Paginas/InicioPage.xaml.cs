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

            // Mostrar indicador de carga
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            // Obtener las publicaciones reales de la API
            var publicaciones = await _apiService.ObtenerPublicaciones();

            if (publicaciones != null && publicaciones.Count > 0)
            {
                foreach (var pub in publicaciones)
                {
                    _publicaciones.Add(pub);
                }
            }
            else
            {
                // No mostrar mensaje si no hay publicaciones, simplemente dejarlo vacío
                // El EmptyView del CollectionView ya mostrará un mensaje adecuado
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar publicaciones: {ex.Message}");
            // No mostrar el error al usuario a menos que sea necesario
            // await DisplayAlert("Error", $"No se pudieron cargar las publicaciones: {ex.Message}", "OK");
        }
        finally
        {
            // Ocultar indicador de carga
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            RefrescarContenido.IsRefreshing = false;
        }
    }

    private async void RefrescarContenido_Refreshing(object sender, EventArgs e)
    {
        await CargarPublicaciones();
    }

    private async void OnPublicacionTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is PublicacionViewModel publicacion)
        {
            try
            {
                // Navegar a la página de detalle de la publicación
                await Shell.Current.GoToAsync($"PublicacionPage?id={publicacion.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir la publicación: {ex.Message}", "OK");
            }
        }
    }
}