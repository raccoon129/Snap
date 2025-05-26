using Snap.Servicios;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Snap.Paginas;

public partial class PerfilPage : ContentPage
{
    private readonly ApiService _apiService;
    private ObservableCollection<PublicacionViewModel> _publicaciones = new ObservableCollection<PublicacionViewModel>();
    private ObservableCollection<PublicacionViewModel> _favoritos = new ObservableCollection<PublicacionViewModel>();
    private int usuarioId;

    // Propiedades para el RefreshView
    public bool IsRefreshingRecuerdos { get; set; }
    public bool IsRefreshingFavoritos { get; set; }
    public ICommand RefreshRecuerdosCommand { get; private set; }
    public ICommand RefreshFavoritosCommand { get; private set; }

    public PerfilPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Inicializar los comandos
        RefreshRecuerdosCommand = new Command(async () => await RefrescarRecuerdos());
        RefreshFavoritosCommand = new Command(async () => await RefrescarFavoritos());

        // Establecer el contexto de los bindings
        BindingContext = this;

        GridRecuerdos.ItemsSource = _publicaciones;
        GridFavoritos.ItemsSource = _favoritos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        await CargarDatosUsuario();
        
        // Si ya teníamos datos cargados, solo refrescar favoritos
        // para evitar recargar todo cada vez
        if (_publicaciones.Count > 0)
        {
            await RefrescarFavoritos();
        }
        else
        {
            await CargarPublicaciones();
        }

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
            // Mostrar indicador de carga
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            // Cargar publicaciones del usuario
            _publicaciones.Clear();
            var publicacionesUsuario = await _apiService.ObtenerPublicacionesDeUsuario(usuarioId);

            if (publicacionesUsuario != null && publicacionesUsuario.Count > 0)
            {
                foreach (var pub in publicacionesUsuario)
                {
                    // Incluir todas las publicaciones, incluso sin foto
                    _publicaciones.Add(pub);
                }
            }

            // Mostrar mensaje si no hay publicaciones
            EmptyRecuerdosLabel.IsVisible = _publicaciones.Count == 0;

            // Cargar favoritos con el método mejorado
            await CargarFavoritos();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar publicaciones: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar tus recuerdos. Por favor, intenta de nuevo.", "OK");
            EmptyRecuerdosLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    // Método separado para cargar favoritos
    private async Task CargarFavoritos()
    {
        try
        {
            _favoritos.Clear();
            var fotos = await _apiService.ObtenerFavoritos();

            // Convertir fotos a PublicacionViewModel para mostrarlas
            if (fotos != null && fotos.Count > 0)
            {
                foreach (var foto in fotos)
                {
                    if (foto != null)
                    {
                        _favoritos.Add(new PublicacionViewModel
                        {
                            Id = foto.id_foto,
                            UrlFoto = foto.url_foto ?? "imagennodisponible.png",
                            IdUsuario = foto.id_usuario
                        });
                    }
                }
            }

            // Mostrar mensaje si no hay favoritos
            EmptyFavoritosLabel.IsVisible = _favoritos.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar favoritos: {ex.Message}");
            EmptyFavoritosLabel.IsVisible = true;
        }
    }

    // Agregar este método para refrescar manualmente los favoritos
    public async Task RefrescarFavoritos()
    {
        try
        {
            IsRefreshingFavoritos = true;
            OnPropertyChanged(nameof(IsRefreshingFavoritos));
            
            await CargarFavoritos();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al refrescar favoritos: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar tus favoritos.", "OK");
        }
        finally
        {
            IsRefreshingFavoritos = false;
            OnPropertyChanged(nameof(IsRefreshingFavoritos));
        }
    }

    private async Task RefrescarRecuerdos()
    {
        try
        {
            IsRefreshingRecuerdos = true;
            OnPropertyChanged(nameof(IsRefreshingRecuerdos));
            
            _publicaciones.Clear();
            var publicacionesUsuario = await _apiService.ObtenerPublicacionesDeUsuario(usuarioId);

            if (publicacionesUsuario != null && publicacionesUsuario.Count > 0)
            {
                foreach (var pub in publicacionesUsuario)
                {
                    _publicaciones.Add(pub);
                }
            }
            
            EmptyRecuerdosLabel.IsVisible = _publicaciones.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al refrescar recuerdos: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar tus recuerdos.", "OK");
        }
        finally
        {
            IsRefreshingRecuerdos = false;
            OnPropertyChanged(nameof(IsRefreshingRecuerdos));
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

        RecuerdosRefreshView.IsVisible = false;
        FavoritosRefreshView.IsVisible = false;
        EmptyRecuerdosLabel.IsVisible = false;
        EmptyFavoritosLabel.IsVisible = false;
        ViewAjustes.IsVisible = false;

        // Mostrar pestaña seleccionada
        if (pestana == 0)
        {
            BtnRecuerdos.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorRecuerdos.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            RecuerdosRefreshView.IsVisible = true;
            EmptyRecuerdosLabel.IsVisible = _publicaciones.Count == 0;
        }
        else if (pestana == 1)
        {
            BtnFavoritos.TextColor = (Color)Application.Current.Resources["Primary"];
            IndicadorFavoritos.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            FavoritosRefreshView.IsVisible = true;
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
    private async void OnQRClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/GestionAmigos");
    }
}