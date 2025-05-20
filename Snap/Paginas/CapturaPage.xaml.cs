namespace Snap.Paginas;

public partial class CapturaPage : ContentPage
{
    private bool camaraIniciada = false;

    public CapturaPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await IniciarCamara();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Detener la c�mara cuando se sale de la p�gina
        DetenerCamara();
    }

    private async Task IniciarCamara()
    {
        try
        {
            // En una implementaci�n real, aqu� se iniciar�a la c�mara
            // Esto requiere permisos y usar CommunityToolkit.Maui.MediaElement o similar

            // Comprobaci�n de permisos
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    LblNoCamara.Text = "Se requiere permiso de c�mara";
                    LblNoCamara.IsVisible = true;
                    return;
                }
            }

            // Simular que la c�mara est� lista
            ImgVistaPrevia.Source = "placeholder_camera.png";
            camaraIniciada = true;
            LblNoCamara.IsVisible = false;
        }
        catch (Exception ex)
        {
            LblNoCamara.Text = $"Error al iniciar c�mara: {ex.Message}";
            LblNoCamara.IsVisible = true;
        }
    }

    private void DetenerCamara()
    {
        // Detener la c�mara
        camaraIniciada = false;
    }

    private async void OnCapturarClicked(object sender, EventArgs e)
    {
        if (!camaraIniciada)
        {
            await DisplayAlert("Error", "La c�mara no est� disponible", "OK");
            return;
        }

        // Simulaci�n de captura
        BtnCapturar.IsEnabled = false;
        await Task.Delay(500); // Simular flash o procesamiento

        // Aqu� se tomar�a la foto realmente

        // Navegar a la vista previa de la publicaci�n
        // await Shell.Current.GoToAsync("NuevaPublicacion");
        await DisplayAlert("Foto capturada", "Funcionalidad en desarrollo", "OK");

        BtnCapturar.IsEnabled = true;
    }

    private async void OnGaleriaClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                // Procesar la foto seleccionada
                // await Shell.Current.GoToAsync("NuevaPublicacion");
                await DisplayAlert("Foto seleccionada", "Funcionalidad en desarrollo", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo acceder a la galer�a: {ex.Message}", "OK");
        }
    }

    private void OnVoltearClicked(object sender, EventArgs e)
    {
        // Cambiar entre c�mara frontal y trasera
        // En una implementaci�n real, cambiar�as la c�mara activa
        DisplayAlert("Cambiar c�mara", "Funcionalidad en desarrollo", "OK");
    }
}