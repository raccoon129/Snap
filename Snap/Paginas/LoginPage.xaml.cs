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
        // Limpiar campos si vienen de cierre de sesi�n
        EntryUsuario.Text = string.Empty;
        EntryPin.Text = string.Empty;



        var sesion = await _apiService.ObtenerSesionActual();
        if (sesion.SesionActiva)
        {
            // Navegar al TabBar principal
            await Shell.Current.GoToAsync("///Inicio");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryUsuario.Text) || string.IsNullOrWhiteSpace(EntryPin.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa tu email/tel�fono y PIN", "OK");
            return;
        }

        try
        {
            Cargando.IsVisible = true;
            Cargando.IsRunning = true;
            BtnLogin.IsEnabled = false;

            var resultado = await _apiService.IniciarSesion(EntryUsuario.Text, EntryPin.Text);

            if (resultado.Item1)
            {
                // Ir a la p�gina principal
                await Shell.Current.GoToAsync("///Inicio");
            }
            else
            {
                await DisplayAlert("Error", resultado.Item2, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurri� un error: {ex.Message}", "OK");
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
        // Navegar a la p�gina de registro (cuando se implemente)
        //await DisplayAlert("Registro", "Funcionalidad de registro pendiente", "OK");

        await Shell.Current.GoToAsync("/Registro");
    }
}