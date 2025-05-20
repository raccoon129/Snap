using Snap.Servicios;

namespace Snap.Paginas;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var sesion = await _apiService.ObtenerSesionActual();
        if (sesion.SesionActiva)
        {
            // Navegar al TabBar principal
            await Shell.Current.GoToAsync("//Principal/Inicio");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryUsuario.Text) || string.IsNullOrWhiteSpace(EntryPin.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa tu email/teléfono y PIN", "OK");
            return;
        }

        try
        {
            Cargando.IsVisible = true;
            Cargando.IsRunning = true;
            BtnLogin.IsEnabled = false;

            var resultado = await _apiService.IniciarSesion(EntryUsuario.Text, EntryPin.Text);

            if (resultado)
            {
                // Ir a la página principal
                await Shell.Current.GoToAsync("//Principal/Inicio");
            }
            else
            {
                await DisplayAlert("Error", "Credenciales incorrectas", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
        finally
        {
            Cargando.IsVisible = false;
            Cargando.IsRunning = false;
            BtnLogin.IsEnabled = true;
        }
    }

    private async void OnRegistrateClicked(object sender, EventArgs e)
    {
        // Navegar a la página de registro (cuando se implemente)
        await DisplayAlert("Registro", "Funcionalidad de registro pendiente", "OK");
    }
}