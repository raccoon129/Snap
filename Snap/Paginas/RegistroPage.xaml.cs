using COMMON.Entidades;
using Snap.Servicios;
using System.Text;

namespace Snap.Paginas;

public partial class RegistroPage : ContentPage
{
    private readonly ApiService _apiService;
    private int _pasoActual = 1;
    private string _pinGenerado;
    private string _contacto;

    public RegistroPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnSiguienteClicked(object sender, EventArgs e)
    {
        // Deshabilitar botones durante procesamiento
        BtnSiguiente.IsEnabled = false;
        BtnAnterior.IsEnabled = false;

        try
        {
            switch (_pasoActual)
            {
                case 1:
                    await ProcesarPaso1();
                    break;
                case 2:
                    MostrarPaso3();
                    break;
                case 3:
                    await ProcesarRegistroFinal();
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
        finally
        {
            // Rehabilitar botones según corresponda
            BtnSiguiente.IsEnabled = true;
            BtnAnterior.IsEnabled = _pasoActual > 1;
        }
    }

    private void OnAnteriorClicked(object sender, EventArgs e)
    {
        _pasoActual--;

        switch (_pasoActual)
        {
            case 1:
                Paso1.IsVisible = true;
                Paso2.IsVisible = false;
                LblPasoActual.Text = "Paso 1: Datos de contacto";
                BtnAnterior.IsEnabled = false;
                break;
            case 2:
                Paso2.IsVisible = true;
                Paso3.IsVisible = false;
                LblPasoActual.Text = "Paso 2: PIN de acceso";
                break;
        }

        BtnSiguiente.Text = "Siguiente";
    }

    private async Task ProcesarPaso1()
    {
        if (string.IsNullOrWhiteSpace(EntryContacto.Text))
        {
            await DisplayAlert("Campos incompletos", "Por favor ingresa tu correo o número de teléfono.", "OK");
            return;
        }

        _contacto = EntryContacto.Text.Trim();
        bool esCorreo = _contacto.Contains('@');

        // Verificar si el correo/teléfono ya existe
        Cargando.IsVisible = true;
        Cargando.IsRunning = true;

        try
        {
            // Aquí deberías llamar a un endpoint para verificar si el contacto existe
            // Por ahora simulamos que la validación es exitosa
            await Task.Delay(1000); // Simular petición al servidor

            // Generar PIN y mostrar paso 2
            _pinGenerado = GenerarPinAleatorio();
            LblPinGenerado.Text = _pinGenerado;

            MostrarPaso2();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo verificar el contacto: {ex.Message}", "OK");
            throw;
        }
        finally
        {
            Cargando.IsVisible = false;
            Cargando.IsRunning = false;
        }
    }

    private void MostrarPaso2()
    {
        _pasoActual = 2;
        Paso1.IsVisible = false;
        Paso2.IsVisible = true;
        Paso3.IsVisible = false;
        LblPasoActual.Text = "Paso 2: PIN de acceso";
        BtnAnterior.IsEnabled = true;
    }

    private void MostrarPaso3()
    {
        _pasoActual = 3;
        Paso2.IsVisible = false;
        Paso3.IsVisible = true;
        LblPasoActual.Text = "Paso 3: Configura tu perfil";
        BtnSiguiente.Text = "Completar registro";
    }

    private async Task ProcesarRegistroFinal()
    {
        if (string.IsNullOrWhiteSpace(EntryNombreUsuario.Text))
        {
            await DisplayAlert("Campos incompletos", "Por favor ingresa un nombre de usuario.", "OK");
            return;
        }

        string nombreUsuario = EntryNombreUsuario.Text.Trim();
        if (nombreUsuario.StartsWith("@"))
        {
            nombreUsuario = nombreUsuario.Substring(1);
        }

        Cargando.IsVisible = true;
        Cargando.IsRunning = true;

        try
        {
            // Crear objeto usuario para registrar
            var nuevoUsuario = new usuario
            {
                nombre_usuario = nombreUsuario,
                biografia = EditorBiografia.Text,
                pais = EntryPais.Text,
                pin_contacto = _pinGenerado,
                estado = "activo",
                ultima_conexion = DateTime.Now,
                fecha_creacion = DateTime.Now
            };

            // Determinar si se registra email o teléfono
            if (_contacto.Contains('@'))
            {
                nuevoUsuario.email = _contacto;
                nuevoUsuario.telefono = "";
            }
            else
            {
                nuevoUsuario.telefono = _contacto;
                nuevoUsuario.email = null;
            }

            // Registrar el usuario
            var resultado = await _apiService.RegistrarUsuario(nuevoUsuario);
            if (resultado.Item1)
            {
                await DisplayAlert("¡Registro exitoso!",
                    $"Tu cuenta ha sido creada con éxito. Tu PIN de acceso es: {_pinGenerado}. Por favor guárdalo para iniciar sesión.",
                    "OK");

                // Iniciar sesión automáticamente
                var loginResult = await _apiService.IniciarSesion(_contacto, _pinGenerado);
                if (loginResult.Item1)
                {
                    await Shell.Current.GoToAsync("///Inicio");
                }
                else
                {
                    // Si el inicio automático falla, ir a la pantalla de login
                    await Shell.Current.GoToAsync("///Login");
                }
            }
            else
            {
                await DisplayAlert("Error", resultado.Item2, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo completar el registro: {ex.Message}", "OK");
        }
        finally
        {
            Cargando.IsVisible = false;
            Cargando.IsRunning = false;
        }
    }

    private string GenerarPinAleatorio(int longitud = 5)
    {
        const string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder();
        Random random = new Random();

        for (int i = 0; i < longitud; i++)
        {
            int index = random.Next(caracteresPermitidos.Length);
            sb.Append(caracteresPermitidos[index]);
        }

        return sb.ToString();
    }
}