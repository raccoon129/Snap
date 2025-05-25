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

        // Iniciar en modo c�mara
        if (!modoEdicion)
        {
            MostrarModoCamara();
            await IniciarCamara();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Detener la c�mara cuando se sale de la p�gina
        if (!modoEdicion)
        {
            DetenerCamara();
        }
    }

    private async Task IniciarCamara()
    {
        try
        {
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

            // En una implementaci�n real, aqu� iniciar�amos la vista previa de la c�mara
            // Para este ejemplo, mostramos una imagen est�tica
            ImgVistaPrevia.Source = "placeholder_camera.png";
            camaraIniciada = true;
            LblNoCamara.IsVisible = false;
        }
        catch (Exception ex)
        {
            LblNoCamara.Text = $"Error al iniciar c�mara: {ex.Message}";
            LblNoCamara.IsVisible = true;
            Debug.WriteLine($"Error al iniciar c�mara: {ex}");
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
                    await DisplayAlert("Permiso denegado", "Se requiere acceso a la galer�a", "OK");
                    return;
                }
            }

            // Seleccionar foto de la galer�a
            fotoSeleccionada = await MediaPicker.PickPhotoAsync();

            if (fotoSeleccionada != null)
            {
                hayFoto = true;
                MostrarModoEdicion();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo acceder a la galer�a: {ex.Message}", "OK");
            Debug.WriteLine($"Error al acceder a la galer�a: {ex}");
        }
    }

    private void OnVoltearClicked(object sender, EventArgs e)
    {
        // Cambiar entre c�mara frontal y trasera
        // En una implementaci�n real, cambiar�as la c�mara activa
        DisplayAlert("Cambiar c�mara", "Funcionalidad en desarrollo", "OK");
    }

    private void OnTextoClicked(object sender, EventArgs e)
    {
        // Cambiar al modo de publicaci�n de solo texto
        hayFoto = false;
        MostrarModoEdicion();
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        // Volver al modo c�mara
        MostrarModoCamara();
    }

    private async void OnPublicarClicked(object sender, EventArgs e)
    {
        // Obtener descripci�n y ubicaci�n
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

            // Publicar con o sin foto seg�n corresponda
            if (hayFoto && fotoSeleccionada != null)
            {
                // Publicaci�n con foto
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
                // Para publicaciones sin foto, necesitamos implementar este m�todo
                // en ApiService si no existe
                resultado = await CrearPublicacionSinFoto(descripcion, ubicacion);
            }

            if (resultado.Item1)
            {
                await DisplayAlert("�xito", "Publicaci�n creada correctamente", "OK");
                // Volver al modo c�mara y limpiar los datos
                LimpiarFormulario();
                MostrarModoCamara();
            }
            else
            {
                await DisplayAlert("Error", $"No se pudo crear la publicaci�n: {resultado.Item2}", "OK");
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
        // Cambiar a modo c�mara
        modoEdicion = false;

        // Mostrar controles de c�mara
        CamaraControles.IsVisible = true;
        CamaraGrid.IsVisible = true;
        EditorGrid.IsVisible = false;

        // Ocultar controles de edici�n
        BtnCancelar.IsVisible = false;
        BtnPublicar.IsVisible = false;
        LblTitulo.IsVisible = false;

        // Mostrar botones correctos
        BtnTexto.IsVisible = true;
        BtnVoltear.IsVisible = true;
    }

    private void MostrarModoEdicion()
    {
        // Cambiar a modo edici�n
        modoEdicion = true;

        // Ocultar controles de c�mara
        CamaraControles.IsVisible = false;
        CamaraGrid.IsVisible = false;

        // Mostrar editor
        EditorGrid.IsVisible = true;

        // Mostrar controles de edici�n
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
            // Validar si el usuario est� autenticado
            var sesion = await _apiService.ObtenerSesionActual();
            if (!sesion.SesionActiva || sesion.Usuario == null)
            {
                return Tuple.Create(false, "Usuario no autenticado", 0);
            }

            // Crear modelo para la publicaci�n sin foto
            var publicacionModel = new
            {
                IdUsuario = sesion.Usuario.id_usuario,
                Descripcion = descripcion ?? string.Empty,
                Ubicacion = ubicacion ?? string.Empty
            };

            // Hacer la petici�n POST al endpoint
            var response = await new HttpClient().PostAsJsonAsync(
                $"{COMMON.Params.UrlAPI}api/publicacion",
                publicacionModel
            );

            if (response.IsSuccessStatusCode)
            {
                var publicacion = await response.Content.ReadFromJsonAsync<COMMON.Entidades.publicacion>();
                if (publicacion != null)
                {
                    return Tuple.Create(true, "Publicaci�n creada con �xito", publicacion.id_publicacion);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                ? "Error al crear publicaci�n"
                : errorContent, 0);
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"Error: {ex.Message}", 0);
        }
    }
}