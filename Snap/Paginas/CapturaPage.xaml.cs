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
        // Detener la cámara cuando se sale de la página
        DetenerCamara();
    }

    private async Task IniciarCamara()
    {
        try
        {
            // En una implementación real, aquí se iniciaría la cámara
            // Esto requiere permisos y usar CommunityToolkit.Maui.MediaElement o similar

            // Comprobación de permisos
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    LblNoCamara.Text = "Se requiere permiso de cámara";
                    LblNoCamara.IsVisible = true;
                    return;
                }
            }

            // Simular que la cámara está lista
            ImgVistaPrevia.Source = "placeholder_camera.png";
            camaraIniciada = true;
            LblNoCamara.IsVisible = false;
        }
        catch (Exception ex)
        {
            LblNoCamara.Text = $"Error al iniciar cámara: {ex.Message}";
            LblNoCamara.IsVisible = true;
        }
    }

    private void DetenerCamara()
    {
        // Detener la cámara
        camaraIniciada = false;
    }

    private async void OnCapturarClicked(object sender, EventArgs e)
    {
        if (!camaraIniciada)
        {
            await DisplayAlert("Error", "La cámara no está disponible", "OK");
            return;
        }

        // Simulación de captura
        BtnCapturar.IsEnabled = false;
        await Task.Delay(500); // Simular flash o procesamiento

        // Aquí se tomaría la foto realmente

        // Navegar a la vista previa de la publicación
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
            await DisplayAlert("Error", $"No se pudo acceder a la galería: {ex.Message}", "OK");
        }
    }

    private void OnVoltearClicked(object sender, EventArgs e)
    {
        // Cambiar entre cámara frontal y trasera
        // En una implementación real, cambiarías la cámara activa
        DisplayAlert("Cambiar cámara", "Funcionalidad en desarrollo", "OK");
    }
}