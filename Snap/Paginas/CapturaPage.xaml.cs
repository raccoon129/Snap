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

        // Esta cosa para desactivar el boton inicialmente
        ActualizarEstadoBotonPublicar();
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

        // Nueva validación: Verificar si hay una foto (requisito obligatorio)
        if (!hayFoto || fotoSeleccionada == null)
        {
            await DisplayAlert("Aviso", "Debes agregar una foto para publicar. En Snap, cada recuerdo debe tener una imagen.", "OK");
            return;
        }

        // Mostrar indicador de carga
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnPublicar.IsEnabled = false;

        try
        {
            // Siempre publicar con foto, ya que ahora es obligatorio
            using var stream = await fotoSeleccionada.OpenReadAsync();
            var resultado = await _apiService.CrearPublicacion(
                descripcion,
                ubicacion,
                stream,
                fotoSeleccionada.FileName
            );

            if (resultado.Item1)
            {
                await DisplayAlert("Éxito", "Publicación creada correctamente", "OK");
                
                // Limpiar formulario
                LimpiarFormulario();
                
                // Navegar directamente a la página de Inicio en lugar de volver atrás
                await Shell.Current.GoToAsync("///Inicio");
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
            ActualizarEstadoBotonPublicar();
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

    private void ActualizarEstadoBotonPublicar()
    {
        // El botón solo estará habilitado si hay una foto
        BtnPublicar.IsEnabled = hayFoto && fotoSeleccionada != null;
    }
}