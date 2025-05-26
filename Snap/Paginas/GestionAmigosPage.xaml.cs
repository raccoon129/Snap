using System.Collections.ObjectModel;
using Snap.Servicios;

namespace Snap.Paginas;

public partial class GestionAmigos : ContentPage
{
    private readonly ApiService _apiService;
    private ObservableCollection<SolicitudAmistadViewModel> _solicitudes = new ObservableCollection<SolicitudAmistadViewModel>();
    private ObservableCollection<UsuarioViewModel> _amigos = new ObservableCollection<UsuarioViewModel>();
    private string _pinUsuario;

    public GestionAmigos(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Establecer fuentes de datos
        ListaSolicitudes.ItemsSource = _solicitudes;
        ListaAmigos.ItemsSource = _amigos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CargandoIndicador.IsRunning = true;
        CargandoIndicador.IsVisible = true;

        await CargarDatos();

        CargandoIndicador.IsRunning = false;
        CargandoIndicador.IsVisible = false;
    }

    private async Task CargarDatos()
    {
        try
        {
            // Obtener el PIN del usuario actual
            var sesion = await _apiService.ObtenerSesionActual();
            if (sesion.SesionActiva && sesion.Usuario != null)
            {
                _pinUsuario = sesion.Usuario.pin_contacto;
                LblPinUsuario.Text = _pinUsuario;

                // Cargar solicitudes pendientes
                await CargarSolicitudesPendientes();

                // Cargar amigos
                await CargarAmigos();
            }
            else
            {
                await DisplayAlert("Error", "No hay sesión activa", "OK");
                await Shell.Current.GoToAsync("//Login");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
        }
    }

    private async Task CargarSolicitudesPendientes()
    {
        try
        {
            _solicitudes.Clear();
            var solicitudes = await _apiService.ObtenerSolicitudesPendientes();

            if (solicitudes != null && solicitudes.Count > 0)
            {
                foreach (var solicitud in solicitudes)
                {
                    _solicitudes.Add(solicitud);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar solicitudes: {ex.Message}", "OK");
        }
    }

    private async Task CargarAmigos()
    {
        try
        {
            _amigos.Clear();
            var amigos = await _apiService.ObtenerAmigos();

            if (amigos != null && amigos.Count > 0)
            {
                foreach (var amigo in amigos)
                {
                    _amigos.Add(amigo);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar amigos: {ex.Message}", "OK");
        }
    }

    private async void OnMiPinClicked(object sender, EventArgs e)
    {
        BtnMiPin.TextColor = (Color)Application.Current.Resources["Primary"];
        BtnSolicitudes.TextColor = Colors.Gray;
        IndicadorMiPin.BackgroundColor = (Color)Application.Current.Resources["Primary"];
        IndicadorSolicitudes.BackgroundColor = Colors.Transparent;

        VistaMiPin.IsVisible = true;
        VistaSolicitudes.IsVisible = false;
    }

    private async void OnSolicitudesClicked(object sender, EventArgs e)
    {
        BtnMiPin.TextColor = Colors.Gray;
        BtnSolicitudes.TextColor = (Color)Application.Current.Resources["Primary"];
        IndicadorMiPin.BackgroundColor = Colors.Transparent;
        IndicadorSolicitudes.BackgroundColor = (Color)Application.Current.Resources["Primary"];

        VistaMiPin.IsVisible = false;
        VistaSolicitudes.IsVisible = true;

        // Refrescar datos al cambiar a esta pestaña
        CargandoIndicador.IsRunning = true;
        CargandoIndicador.IsVisible = true;

        await CargarSolicitudesPendientes();
        await CargarAmigos();

        CargandoIndicador.IsRunning = false;
        CargandoIndicador.IsVisible = false;
    }

    private async void OnEnviarSolicitudClicked(object sender, EventArgs e)
    {
        try
        {
            // Mostrar indicador y ocultar mensaje previo
            CargandoIndicador.IsRunning = true;
            CargandoIndicador.IsVisible = true;
            LblResultadoSolicitud.IsVisible = false;

            // Validar el PIN
            string pinAmigo = EntryPinAmigo.Text?.Trim();
            if (string.IsNullOrEmpty(pinAmigo))
            {
                LblResultadoSolicitud.Text = "Introduce un PIN válido";
                LblResultadoSolicitud.TextColor = Colors.Red;
                LblResultadoSolicitud.IsVisible = true;
                return;
            }

            // Verificar si el PIN pertenece a un usuario válido
            // Nota: Este método no existe actualmente, se necesita implementar en el backend
            var usuarioEncontrado = await _apiService.BuscarUsuarioPorPin(pinAmigo);

            if (usuarioEncontrado != null)
            {
                // Enviar solicitud de amistad
                var resultado = await _apiService.EnviarSolicitudAmistad(usuarioEncontrado.Id);

                if (resultado.Item1)
                {
                    LblResultadoSolicitud.Text = "Solicitud enviada correctamente";
                    LblResultadoSolicitud.TextColor = Colors.Green;
                    EntryPinAmigo.Text = string.Empty;
                }
                else
                {
                    LblResultadoSolicitud.Text = resultado.Item2;
                    LblResultadoSolicitud.TextColor = Colors.Red;
                }
            }
            else
            {
                LblResultadoSolicitud.Text = "No se encontró ningún usuario con ese PIN";
                LblResultadoSolicitud.TextColor = Colors.Red;
            }

            LblResultadoSolicitud.IsVisible = true;
        }
        catch (Exception ex)
        {
            LblResultadoSolicitud.Text = $"Error: {ex.Message}";
            LblResultadoSolicitud.TextColor = Colors.Red;
            LblResultadoSolicitud.IsVisible = true;
        }
        finally
        {
            CargandoIndicador.IsRunning = false;
            CargandoIndicador.IsVisible = false;
        }
    }

    private async void OnAceptarSolicitudClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter != null)
        {
            int idSolicitud = Convert.ToInt32(button.CommandParameter);
            await AceptarSolicitud(idSolicitud);
        }
    }

    private async void OnRechazarSolicitudClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter != null)
        {
            int idSolicitud = Convert.ToInt32(button.CommandParameter);
            await RechazarSolicitud(idSolicitud);
        }
    }

    private async Task AceptarSolicitud(int idSolicitud)
    {
        try
        {
            bool aceptada = await _apiService.AceptarSolicitudAmistad(idSolicitud);

            if (aceptada)
            {
                await DisplayAlert("Éxito", "Solicitud de amistad aceptada", "OK");

                // Refrescar listas
                await CargarSolicitudesPendientes();
                await CargarAmigos();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo aceptar la solicitud", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al aceptar solicitud: {ex.Message}", "OK");
        }
    }

    private async Task RechazarSolicitud(int idSolicitud)
    {
        try
        {
            // Nota: Este método no existe actualmente, se necesita implementar en el backend
            bool rechazada = await _apiService.RechazarSolicitudAmistad(idSolicitud);

            if (rechazada)
            {
                await DisplayAlert("Éxito", "Solicitud de amistad rechazada", "OK");

                // Refrescar lista de solicitudes
                await CargarSolicitudesPendientes();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo rechazar la solicitud", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al rechazar solicitud: {ex.Message}", "OK");
        }
    }
}