using Snap.Servicios;
using System.Collections.ObjectModel;

namespace Snap.Paginas;

public partial class PerfilPage : ContentPage
{
    private readonly ApiService _apiService;
    private ObservableCollection<PublicacionViewModel> _publicaciones = new ObservableCollection<PublicacionViewModel>();
    private ObservableCollection<PublicacionViewModel> _favoritos = new ObservableCollection<PublicacionViewModel>();
    private int usuarioId;

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
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        await CargarDatosUsuario();
        await CargarPublicaciones();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        // Por defecto mostrar la pestaña de recuerdos
        MostrarPestana(0);
    }

    private async Task CargarDatosUsuario()
    {
        try
        {
            var sesion = await _apiService.ObtenerSesionActual();
            if (!sesion.SesionActiva || sesion.Usuario == null)
            {
                // Si no hay sesión activa, volver al login
                await Shell.Current.GoToAsync("//Login");
                return;
            }

            usuarioId = sesion.Usuario.id_usuario;

            // Cargar datos del usuario
            ImgPerfil.Source = !string.IsNullOrEmpty(sesion.Usuario.foto_perfil)
                ? sesion.Usuario.foto_perfil
                : "who.jpg";

            LblNombreUsuario.Text = $"@{sesion.Usuario.nombre_usuario}";
            LblBiografia.Text = !string.IsNullOrEmpty(sesion.Usuario.biografia)
                ? sesion.Usuario.biografia
                : "No hay biografía disponible";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar datos de usuario: {ex.Message}");
            ImgPerfil.Source = "who.jpg";
            LblNombreUsuario.Text = "@usuario";
            LblBiografia.Text = "Error al cargar perfil";
        }
    }

    private async Task CargarPublicaciones()
    {
        try
        {
            // Cargar publicaciones del usuario
            _publicaciones.Clear();
            var publicacionesUsuario = await _apiService.ObtenerPublicacionesDeUsuario(usuarioId);

            if (publicacionesUsuario != null && publicacionesUsuario.Count > 0)
            {
                foreach (var pub in publicacionesUsuario)
                {
                    if (!string.IsNullOrEmpty(pub.UrlFoto))
                    {
                        _publicaciones.Add(pub);
                    }
                }
            }

            // Mostrar mensaje si no hay publicaciones
            if (_publicaciones.Count == 0)
            {
                EmptyRecuerdosLabel.IsVisible = true;
            }
            else
            {
                EmptyRecuerdosLabel.IsVisible = false;
            }

            // Cargar favoritos
            _favoritos.Clear();
            var fotos = await _apiService.ObtenerFavoritos();

            // Convertir fotos a PublicacionViewModel para mostrarlas
            if (fotos != null && fotos.Count > 0)
            {
                foreach (var foto in fotos)
                {
                    if (foto != null && !string.IsNullOrEmpty(foto.url_foto))
                    {
                        _favoritos.Add(new PublicacionViewModel
                        {
                            Id = foto.id_foto,
                            UrlFoto = foto.url_foto,
                            IdUsuario = foto.id_usuario
                        });
                    }
                }
            }

            // Mostrar mensaje si no hay favoritos
            if (_favoritos.Count == 0)
            {
                EmptyFavoritosLabel.IsVisible = true;
            }
            else
            {
                EmptyFavoritosLabel.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar publicaciones: {ex.Message}");
            EmptyRecuerdosLabel.IsVisible = true;
            EmptyFavoritosLabel.IsVisible = true;
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
        EmptyRecuerdosLabel.IsVisible = false;
        GridFavoritos.IsVisible = false;
        EmptyFavoritosLabel.IsVisible = false;
        ViewAjustes.IsVisible = false;

        // Mostrar pestaña seleccionada
        if (pestana == 0)
        {
            BtnRecuerdos.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorRecuerdos.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            GridRecuerdos.IsVisible = true;
            EmptyRecuerdosLabel.IsVisible = _publicaciones.Count == 0;
        }
        else if (pestana == 1)
        {
            BtnFavoritos.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorFavoritos.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            GridFavoritos.IsVisible = true;
            EmptyFavoritosLabel.IsVisible = _favoritos.Count == 0;
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
        await Shell.Current.GoToAsync("EditarPerfilPage");
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