using Snap.Servicios;
using System.Collections.ObjectModel;

namespace Snap.Paginas;

public partial class PerfilPage : ContentPage
{
    private readonly ApiService _apiService;
    private ObservableCollection<PublicacionViewModel> _publicaciones = new ObservableCollection<PublicacionViewModel>();
    private ObservableCollection<PublicacionViewModel> _favoritos = new ObservableCollection<PublicacionViewModel>();

    public PerfilPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        GridRecuerdos.ItemsSource = _publicaciones;
        GridFavoritos.ItemsSource = _favoritos;
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
            return;
        }

        // Cargar datos del usuario
        ImgPerfil.Source = !string.IsNullOrEmpty(sesion.Usuario.foto_perfil)
            ? sesion.Usuario.foto_perfil
            : "default_profile.png";

        LblNombreUsuario.Text = $"@{sesion.Usuario.nombre_usuario}";
        LblBiografia.Text = !string.IsNullOrEmpty(sesion.Usuario.biografia)
            ? sesion.Usuario.biografia
            : "Lorem ipsum dolor sit amet, consectetur adipiscing elit...";
    }

    private async Task CargarPublicaciones()
    {
        try
        {
            // Cargar publicaciones (recuerdos) del usuario
            _publicaciones.Clear();

            // Datos de ejemplo
            for (int i = 1; i <= 9; i++)
            {
                _publicaciones.Add(new PublicacionViewModel
                {
                    Id = i,
                    UrlFoto = $"https://picsum.photos/500/500?random={i}"
                });
            }

            // Cargar favoritos
            _favoritos.Clear();
            for (int i = 10; i <= 15; i++)
            {
                _favoritos.Add(new PublicacionViewModel
                {
                    Id = i,
                    UrlFoto = $"https://picsum.photos/500/500?random={i}"
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las publicaciones: {ex.Message}", "OK");
        }
    }

    private void MostrarPestana(int pestana)
    {
        // Restablecer todo
        BtnRecuerdos.TextColor = Colors.Gray;
        BtnFavoritos.TextColor = Colors.Gray;
        BtnAjustes.TextColor = Colors.Gray;

        IndicadorRecuerdos.BackgroundColor = Colors.Transparent;
        IndicadorFavoritos.BackgroundColor = Colors.Transparent;
        IndicadorAjustes.BackgroundColor = Colors.Transparent;

        GridRecuerdos.IsVisible = false;
        GridFavoritos.IsVisible = false;
        ViewAjustes.IsVisible = false;

        // Mostrar pestaña seleccionada
        if (pestana == 0)
        {
            BtnRecuerdos.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorRecuerdos.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            GridRecuerdos.IsVisible = true;
        }
        else if (pestana == 1)
        {
            BtnFavoritos.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorFavoritos.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            GridFavoritos.IsVisible = true;
        }
        else if (pestana == 2)
        {
            BtnAjustes.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorAjustes.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            ViewAjustes.IsVisible = true;
        }
    }

    private void OnRecuerdosClicked(object sender, EventArgs e)
    {
        MostrarPestana(0);
    }

    private void OnFavoritosClicked(object sender, EventArgs e)
    {
        MostrarPestana(1);
    }

    private void OnAjustesClicked(object sender, EventArgs e)
    {
        MostrarPestana(2);
    }

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Editar perfil", "Funcionalidad en desarrollo", "OK");
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Cerrar sesión", "¿Estás seguro que deseas cerrar tu sesión?", "Sí", "No");
        if (confirmar)
        {
            await _apiService.CerrarSesion();
            await Shell.Current.GoToAsync("//Login");
        }
    }

    private async void OnPublicacionSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var publicacion = e.CurrentSelection.FirstOrDefault() as PublicacionViewModel;
            if (publicacion != null)
            {
                // Deseleccionar
                ((CollectionView)sender).SelectedItem = null;

                // Navegar al detalle de la publicación
                var route = $"PublicacionPage?id={publicacion.Id}";
                await Shell.Current.GoToAsync(route);
            }
        }
    }
}