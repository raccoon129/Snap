using COMMON.Entidades;
using Snap.Modelos;
using System.Net.Http.Json;
using System.Text.Json;

namespace Snap.Servicios
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly SesionUsuario _sesion;

        public ApiService()
        {
            _httpClient = new HttpClient();
            // Aseguramos que la URL base termina con una barra "/"
            var baseUrl = COMMON.Params.UrlAPI;
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            // Añadimos "api/" explícitamente ya que los controladores tienen esa ruta base
            _httpClient.BaseAddress = new Uri(baseUrl + "api/");
            _sesion = new SesionUsuario();
        }


        public async Task<Tuple<bool, string>> IniciarSesion(string emailOTelefono, string pin)
        {
            try
            {
                var loginModel = new
                {
                    Identificador = emailOTelefono,
                    Pin = pin
                };

                // Para depuración: imprimir la URL completa
                Console.WriteLine($"URL de solicitud: {_httpClient.BaseAddress}usuario/iniciarSesion");

                var response = await _httpClient.PostAsJsonAsync("usuario/iniciarSesion", loginModel);

                if (response.IsSuccessStatusCode)
                {
                    var usuarioResponse = await response.Content.ReadFromJsonAsync<usuario>();
                    if (usuarioResponse != null)
                    {
                        _sesion.Usuario = usuarioResponse;
                        _sesion.EstaAutenticado = true;
                        _sesion.FechaExpiracion = DateTime.Now.AddDays(7);

                        await GuardarSesion();
                        return Tuple.Create(true, string.Empty);
                    }
                }
                else
                {
                    // Podemos registrar el mensaje de error para depuración
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error en la respuesta: {errorContent}");
                    return Tuple.Create(false, errorContent);
                }
                return Tuple.Create(false, "Credenciales incorrectas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
                return Tuple.Create(false, ex.Message);
            }
        }



        public async Task<SesionUsuario> ObtenerSesionActual()
        {
            if (_sesion.EstaAutenticado)
                return _sesion;

            // Intentar cargar la sesión guardada
            try
            {
                string sesionJson = await SecureStorage.GetAsync("sesion_usuario");
                if (!string.IsNullOrEmpty(sesionJson))
                {
                    var sesionGuardada = JsonSerializer.Deserialize<SesionUsuario>(sesionJson);
                    if (sesionGuardada != null && sesionGuardada.SesionActiva)
                    {
                        _sesion.Usuario = sesionGuardada.Usuario;
                        _sesion.EstaAutenticado = true;
                        _sesion.FechaExpiracion = sesionGuardada.FechaExpiracion;
                        return _sesion;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al recuperar sesión: {ex.Message}");
            }

            return new SesionUsuario { EstaAutenticado = false };
        }

        public async Task CerrarSesion()
        {
            _sesion.EstaAutenticado = false;
            _sesion.Usuario = null;

            await SecureStorage.SetAsync("sesion_usuario", "");
        }

        private async Task GuardarSesion()
        {
            string sesionJson = JsonSerializer.Serialize(_sesion);
            await SecureStorage.SetAsync("sesion_usuario", sesionJson);
        }


        //////
        ///

        public async Task<Tuple<bool, string>> RegistrarUsuario(usuario nuevoUsuario)
        {
            try
            {
                // Realizar la petición POST para crear el usuario
                var response = await _httpClient.PostAsJsonAsync("usuario", nuevoUsuario);

                if (response.IsSuccessStatusCode)
                {
                    var usuarioCreado = await response.Content.ReadFromJsonAsync<usuario>();
                    if (usuarioCreado != null)
                    {
                        return new Tuple<bool, string>(true, "Usuario registrado correctamente");
                    }
                }

                // Si hay un error, intentar obtener el mensaje
                var errorContent = await response.Content.ReadAsStringAsync();
                return new Tuple<bool, string>(false, string.IsNullOrEmpty(errorContent)
                    ? "Error al registrar usuario"
                    : errorContent);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, $"Error: {ex.Message}");
            }
        }
    }




}