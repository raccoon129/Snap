using Snap.Servicios;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Snap.Paginas;

public partial class CapturaPage : ContentPage
{
    private bool camaraIniciada = false;
    private FileResult fotoSeleccionada;
    private bool modoEdicion = false;
    private bool hayFoto = false;
    private readonly ApiService _apiService;

    public CapturaPage()
    {
        InitializeComponent();
        _apiService = new ApiService();

        // Configurar grid de botones
        BtnTexto.IsVisible = true;
        BtnVoltear.IsVisible = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Iniciar en modo cámara
        if (!modoEdicion)
        {
            MostrarModoCamara();
            await IniciarCamara();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Detener la cámara cuando se sale de la página
        if (!modoEdicion)
        {
            DetenerCamara();
        }
    }

    private async Task IniciarCamara()
    {
        try
        {
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

            // En una implementación real, aquí iniciaríamos la vista previa de la cámara
            // Para este ejemplo, mostramos una imagen estática
            ImgVistaPrevia.Source = "placeholder_camera.png";
            camaraIniciada = true;
            LblNoCamara.IsVisible = false;
        }
        catch (Exception ex)
        {
            LblNoCamara.Text = $"Error al iniciar cámara: {ex.Message}";
            LblNoCamara.IsVisible = true;
            Debug.WriteLine($"Error al iniciar cámara: {ex}");
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

        try
        {
            // Capturar foto usando MediaPicker
            fotoSeleccionada = await MediaPicker.CapturePhotoAsync();

            if (fotoSeleccionada != null)
            {
                hayFoto = true;
                MostrarModoEdicion();
            }
            else
            {
                await DisplayAlert("Aviso", "No se pudo capturar la foto", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al capturar foto: {ex.Message}", "OK");
            Debug.WriteLine($"Error al capturar foto: {ex}");
        }
        finally
        {
            BtnCapturar.IsEnabled = true;
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
                MostrarModoEdicion();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo acceder a la galería: {ex.Message}", "OK");
            Debug.WriteLine($"Error al acceder a la galería: {ex}");
        }
    }

    private void OnVoltearClicked(object sender, EventArgs e)
    {
        // Cambiar entre cámara frontal y trasera
        // En una implementación real, cambiarías la cámara activa
        DisplayAlert("Cambiar cámara", "Funcionalidad en desarrollo", "OK");
    }

    private void OnTextoClicked(object sender, EventArgs e)
    {
        // Cambiar al modo de publicación de solo texto
        hayFoto = false;
        MostrarModoEdicion();
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        // Volver al modo cámara
        MostrarModoCamara();
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
                // Para publicaciones sin foto, necesitamos implementar este método
                // en ApiService si no existe
                resultado = await CrearPublicacionSinFoto(descripcion, ubicacion);
            }

            if (resultado.Item1)
            {
                await DisplayAlert("Éxito", "Publicación creada correctamente", "OK");
                // Volver al modo cámara y limpiar los datos
                LimpiarFormulario();
                MostrarModoCamara();
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

    private void MostrarModoCamara()
    {
        // Cambiar a modo cámara
        modoEdicion = false;

        // Mostrar controles de cámara
        CamaraControles.IsVisible = true;
        CamaraGrid.IsVisible = true;
        EditorGrid.IsVisible = false;

        // Ocultar controles de edición
        BtnCancelar.IsVisible = false;
        BtnPublicar.IsVisible = false;
        LblTitulo.IsVisible = false;

        // Mostrar botones correctos
        BtnTexto.IsVisible = true;
        BtnVoltear.IsVisible = true;
    }

    private void MostrarModoEdicion()
    {
        // Cambiar a modo edición
        modoEdicion = true;

        // Ocultar controles de cámara
        CamaraControles.IsVisible = false;
        CamaraGrid.IsVisible = false;

        // Mostrar editor
        EditorGrid.IsVisible = true;

        // Mostrar controles de edición
        BtnCancelar.IsVisible = true;
        BtnPublicar.IsVisible = true;
        LblTitulo.IsVisible = true;

        // Mostrar foto si hay
        FrameFotoPreview.IsVisible = hayFoto;
        if (hayFoto && fotoSeleccionada != null)
        {
            ImgFotoPreview.Source = fotoSeleccionada.FullPath;
        }
    }

    private void LimpiarFormulario()
    {
        // Limpiar todos los campos
        EditorDescripcion.Text = string.Empty;
        EntryUbicacion.Text = string.Empty;
        fotoSeleccionada = null;
        hayFoto = false;
    }

    private async Task<Tuple<bool, string, int>> CrearPublicacionSinFoto(string descripcion, string ubicacion)
    {
        try
        {
            // Validar si el usuario está autenticado
            var sesion = await _apiService.ObtenerSesionActual();
            if (!sesion.SesionActiva || sesion.Usuario == null)
            {
                return Tuple.Create(false, "Usuario no autenticado", 0);
            }

            // Crear modelo para la publicación sin foto
            var publicacionModel = new
            {
                IdUsuario = sesion.Usuario.id_usuario,
                Descripcion = descripcion ?? string.Empty,
                Ubicacion = ubicacion ?? string.Empty
            };

            // Hacer la petición POST al endpoint
            var response = await new HttpClient().PostAsJsonAsync(
                $"{COMMON.Params.UrlAPI}api/publicacion",
                publicacionModel
            );

            if (response.IsSuccessStatusCode)
            {
                var publicacion = await response.Content.ReadFromJsonAsync<COMMON.Entidades.publicacion>();
                if (publicacion != null)
                {
                    return Tuple.Create(true, "Publicación creada con éxito", publicacion.id_publicacion);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                ? "Error al crear publicación"
                : errorContent, 0);
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"Error: {ex.Message}", 0);
        }
    }
}