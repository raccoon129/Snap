using Snap.Servicios;
using System.Diagnostics;

namespace Snap.Paginas;

public partial class CapturaPage : ContentPage
{
    private FileResult fotoSeleccionada;
    private bool hayFoto = false;
    private readonly ApiService _apiService;

    public CapturaPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnCapturarClicked(object sender, EventArgs e)
    {
        try
        {
            // Verificar permisos de cámara
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permiso denegado", "Se requiere acceso a la cámara", "OK");
                    return;
                }
            }

            // Usar MediaPicker para capturar foto directamente
            fotoSeleccionada = await MediaPicker.CapturePhotoAsync();

            if (fotoSeleccionada != null)
            {
                hayFoto = true;
                MostrarFotoSeleccionada();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al capturar foto: {ex.Message}", "OK");
            Debug.WriteLine($"Error al capturar foto: {ex}");
        }
    }

    private async void OnGaleriaClicked(object sender, EventArgs e)
    {
        try
        {
            // Verificar permiso de almacenamiento
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permiso denegado", "Se requiere acceso a la galería", "OK");
                    return;
                }
            }

            // Seleccionar foto de la galería
            fotoSeleccionada = await MediaPicker.PickPhotoAsync();

            if (fotoSeleccionada != null)
            {
                hayFoto = true;
                MostrarFotoSeleccionada();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo acceder a la galería: {ex.Message}", "OK");
            Debug.WriteLine($"Error al acceder a la galería: {ex}");
        }
    }

    private void OnEliminarFotoClicked(object sender, EventArgs e)
    {
        // Eliminar la foto seleccionada
        fotoSeleccionada = null;
        hayFoto = false;
        FrameFotoPreview.IsVisible = false;
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        // Volver a la página anterior
        LimpiarFormulario();
        Shell.Current.GoToAsync("..");
    }

    private async void OnPublicarClicked(object sender, EventArgs e)
    {
        // Obtener descripción y ubicación
        string descripcion = EditorDescripcion.Text?.Trim() ?? string.Empty;
        string ubicacion = EntryUbicacion.Text?.Trim() ?? string.Empty;

        // Validar si hay contenido para publicar
        if (string.IsNullOrEmpty(descripcion) && !hayFoto)
        {
            await DisplayAlert("Aviso", "Debes escribir algo o agregar una foto para publicar", "OK");
            return;
        }

        // Mostrar indicador de carga
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnPublicar.IsEnabled = false;

        try
        {
            Tuple<bool, string, int> resultado;

            // Publicar con o sin foto según corresponda
            if (hayFoto && fotoSeleccionada != null)
            {
                // Publicación con foto
                using var stream = await fotoSeleccionada.OpenReadAsync();
                resultado = await _apiService.CrearPublicacion(
                    descripcion,
                    ubicacion,
                    stream,
                    fotoSeleccionada.FileName
                );
            }
            else
            {
                // Publicación sin foto
                resultado = await _apiService.CrearPublicacionSinFoto(descripcion, ubicacion);
            }

            if (resultado.Item1)
            {
                await DisplayAlert("Éxito", "Publicación creada correctamente", "OK");
                // Limpiar y volver a la página anterior
                LimpiarFormulario();
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", $"No se pudo crear la publicación: {resultado.Item2}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al publicar: {ex.Message}", "OK");
            Debug.WriteLine($"Error al publicar: {ex}");
        }
        finally
        {
            // Ocultar indicador de carga
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnPublicar.IsEnabled = true;
        }
    }

    private void MostrarFotoSeleccionada()
    {
        if (fotoSeleccionada != null)
        {
            ImgFotoPreview.Source = fotoSeleccionada.FullPath;
            FrameFotoPreview.IsVisible = true;
        }
    }

    private void LimpiarFormulario()
    {
        // Limpiar todos los campos
        EditorDescripcion.Text = string.Empty;
        EntryUbicacion.Text = string.Empty;
        fotoSeleccionada = null;
        hayFoto = false;
        FrameFotoPreview.IsVisible = false;
    }
}