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

            // Establecer timeouts adecuados
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        #region Autenticación y Gestión de Usuarios

        public async Task<Tuple<bool, string>> IniciarSesion(string emailOTelefono, string pin)
        {
            try
            {
                var loginModel = new
                {
                    Identificador = emailOTelefono,
                    Pin = pin
                };

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
            // Si ya hay una sesión en memoria, verificar que sea válida
            if (_sesion.EstaAutenticado && _sesion.Usuario != null &&
                _sesion.FechaExpiracion > DateTime.Now)
            {
                return _sesion;
            }

            try
            {
                string sesionJson = await SecureStorage.GetAsync("sesion_usuario");
                if (!string.IsNullOrEmpty(sesionJson))
                {
                    var sesionGuardada = JsonSerializer.Deserialize<SesionUsuario>(sesionJson);

                    // Verificar completamente la sesión guardada
                    if (sesionGuardada != null &&
                        sesionGuardada.EstaAutenticado &&
                        sesionGuardada.Usuario != null &&
                        sesionGuardada.FechaExpiracion > DateTime.Now)
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

            // Si llegamos aquí, no hay sesión válida
            _sesion.EstaAutenticado = false;
            _sesion.Usuario = null;
            return _sesion;
        }
        public async Task CerrarSesion()
        {
            try
            {
                // Limpiar datos de sesión completamente
                _sesion.EstaAutenticado = false;
                _sesion.Usuario = null;
                _sesion.FechaExpiracion = DateTime.MinValue;

                // Importante: La clase debe ser serializable para guardar estos cambios
                await GuardarSesion(); // Guardar los cambios para que persistan

                // Limpieza adicional del almacenamiento
                await SecureStorage.SetAsync("sesion_usuario", "");

                Console.WriteLine("Sesión cerrada correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar sesión: {ex.Message}");
            }
        }


        private async Task GuardarSesion()
        {
            string sesionJson = JsonSerializer.Serialize(_sesion);
            await SecureStorage.SetAsync("sesion_usuario", sesionJson);
        }

        public async Task<Tuple<bool, string>> RegistrarUsuario(usuario nuevoUsuario)
        {
            try
            {
                // Usar el endpoint específico para registro
                var registroModel = new
                {
                    NombreUsuario = nuevoUsuario.nombre_usuario,
                    Biografia = nuevoUsuario.biografia,
                    Email = nuevoUsuario.email,
                    Telefono = nuevoUsuario.telefono,
                    Pais = nuevoUsuario.pais,
                    PinContacto = nuevoUsuario.pin_contacto
                };

                var response = await _httpClient.PostAsJsonAsync("usuario/registro", registroModel);

                if (response.IsSuccessStatusCode)
                {
                    var usuarioCreado = await response.Content.ReadFromJsonAsync<usuario>();
                    if (usuarioCreado != null)
                    {
                        return Tuple.Create(true, "Usuario registrado correctamente");
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                    ? "Error al registrar usuario"
                    : errorContent);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, $"Error: {ex.Message}");
            }
        }

        public async Task<Tuple<bool, string>> ActualizarPerfil(
            string idUsuario, string nombreUsuario, string biografia, string pais, string fotoPerfil)
        {
            try
            {
                var perfilModel = new
                {
                    NombreUsuario = nombreUsuario,
                    Biografia = biografia,
                    Pais = pais,
                    FotoPerfil = fotoPerfil
                };

                var response = await _httpClient.PutAsJsonAsync($"usuario/actualizarPerfil/{idUsuario}", perfilModel);

                if (response.IsSuccessStatusCode)
                {
                    var usuarioActualizado = await response.Content.ReadFromJsonAsync<usuario>();
                    if (usuarioActualizado != null)
                    {
                        // Actualizar la sesión con los datos nuevos
                        _sesion.Usuario = usuarioActualizado;
                        await GuardarSesion();
                        return Tuple.Create(true, "Perfil actualizado correctamente");
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                    ? "Error al actualizar perfil"
                    : errorContent);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, $"Error: {ex.Message}");
            }
        }

        public async Task<bool> VerificarContactoExiste(string contacto)
        {
            try
            {
                var response = await _httpClient.GetAsync($"usuario/verificarContacto/{contacto}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Buscar usuario por PIN
        public async Task<UsuarioViewModel> BuscarUsuarioPorPin(string pin)
        {
            try
            {
                var response = await _httpClient.GetAsync($"usuario/buscarPorPin/{pin}");

                if (response.IsSuccessStatusCode)
                {
                    var usuario = await response.Content.ReadFromJsonAsync<usuario>();
                    if (usuario != null)
                    {
                        return new UsuarioViewModel
                        {
                            Id = usuario.id_usuario,
                            NombreUsuario = usuario.nombre_usuario,
                            FotoPerfil = usuario.foto_perfil ?? "default_user.png",
                            Biografia = usuario.biografia ?? string.Empty,
                            Pais = usuario.pais ?? string.Empty
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar usuario por PIN: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Publicaciones
        public async Task<Tuple<bool, string, int>> CrearPublicacionSinFoto(string descripcion, string ubicacion)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return Tuple.Create(false, "Usuario no autenticado", 0);
                }

                var publicacionModel = new publicacion
                {
                    id_usuario = sesion.Usuario.id_usuario,
                    descripcion = descripcion ?? string.Empty,
                    ubicacion = ubicacion ?? string.Empty,
                    fecha_publicacion = DateTime.Now
                };

                var response = await _httpClient.PostAsJsonAsync("publicacion", publicacionModel);

                if (response.IsSuccessStatusCode)
                {
                    var publicacionCreada = await response.Content.ReadFromJsonAsync<publicacion>();
                    if (publicacionCreada != null)
                    {
                        return Tuple.Create(true, "Publicación creada con éxito", publicacionCreada.id_publicacion);
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
        public async Task<List<PublicacionViewModel>> ObtenerPublicaciones()
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return new List<PublicacionViewModel>();
                }

                string idUsuario = sesion.Usuario.id_usuario.ToString();

                try
                {
                    var response = await _httpClient.GetAsync($"publicacion/feed/{idUsuario}");

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"JSON recibido: {jsonResponse}");

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var publicacionesDto = JsonSerializer.Deserialize<List<dynamic>>(jsonResponse, options);

                        if (publicacionesDto != null)
                        {
                            var publicaciones = new List<PublicacionViewModel>();
                            foreach (var pub in publicacionesDto)
                            {
                                try
                                {
                                    var idPublicacion = Convert.ToInt32(pub.GetProperty("id_publicacion").GetInt64());
                                    var idUsuarioPublicacion = Convert.ToInt32(pub.GetProperty("id_usuario").GetInt64());
                                    var numLikes = pub.GetProperty("numero_likes").ValueKind != JsonValueKind.Null ?
                                        Convert.ToInt32(pub.GetProperty("numero_likes").GetInt64()) : 0;
                                    var numComentarios = pub.GetProperty("numero_comentarios").ValueKind != JsonValueKind.Null ?
                                        Convert.ToInt32(pub.GetProperty("numero_comentarios").GetInt64()) : 0;

                                    var fechaPublicacion = pub.GetProperty("fecha_publicacion").GetDateTime();
                                    var nombreUsuario = pub.GetProperty("nombre_usuario").ValueKind != JsonValueKind.Null ?
                                        pub.GetProperty("nombre_usuario").GetString() : "Usuario";
                                    var fotoPerfil = pub.GetProperty("foto_perfil").ValueKind != JsonValueKind.Null ?
                                        pub.GetProperty("foto_perfil").GetString() : "who.jpg";
                                    var urlFoto = pub.GetProperty("url_foto").ValueKind != JsonValueKind.Null ?
                                        pub.GetProperty("url_foto").GetString() : string.Empty;
                                    var descripcion = pub.GetProperty("descripcion").ValueKind != JsonValueKind.Null ?
                                        pub.GetProperty("descripcion").GetString() : string.Empty;
                                    var ubicacion = pub.GetProperty("ubicacion").ValueKind != JsonValueKind.Null ?
                                        pub.GetProperty("ubicacion").GetString() : string.Empty;

                                    publicaciones.Add(new PublicacionViewModel
                                    {
                                        Id = idPublicacion,
                                        IdUsuario = idUsuarioPublicacion,
                                        NombreUsuario = nombreUsuario,
                                        UrlFotoPerfil = fotoPerfil,
                                        UrlFoto = urlFoto,
                                        Descripcion = descripcion,
                                        Ubicacion = ubicacion,
                                        TiempoPublicacion = ObtenerTiempoRelativo(fechaPublicacion),
                                        NumeroLikes = numLikes,
                                        NumeroComentarios = numComentarios
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error al procesar publicación: {ex.Message}");
                                }
                            }
                            return publicaciones;
                        }
                    }

                    Console.WriteLine($"Error en respuesta: {await response.Content.ReadAsStringAsync()}");

                    // Si no funcionó, intentamos con las publicaciones del usuario
                    return await ObtenerPublicacionesDeUsuario(sesion.Usuario.id_usuario);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error específico: {ex.Message}");
                    // Plan B: Obtener publicaciones del usuario actual
                    return await ObtenerPublicacionesDeUsuario(sesion.Usuario.id_usuario);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                return new List<PublicacionViewModel>();
            }
        }
        public async Task<List<PublicacionViewModel>> ObtenerPublicacionesDeUsuario(int idUsuario)
        {
            try
            {
                // Obtener la respuesta del servidor
                var response = await _httpClient.GetAsync($"usuario/{idUsuario}/publicaciones");

                if (response.IsSuccessStatusCode)
                {
                    // Para diagnóstico, imprimir la respuesta
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Respuesta del servidor: {jsonResponse}");

                    // Opciones de deserialización que ignoran mayúsculas/minúsculas
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // Intentar deserializar con el modelo correcto
                    var publicaciones = JsonSerializer.Deserialize<List<PublicacionConDetalles>>(jsonResponse, options);
                    
                    if (publicaciones != null)
                    {
                        var resultado = new List<PublicacionViewModel>();
                        foreach (var p in publicaciones)
                        {
                            // Manejar valores nulos apropiadamente
                            var viewModel = new PublicacionViewModel
                            {
                                Id = p.id_publicacion,
                                IdUsuario = p.id_usuario,
                                NombreUsuario = p.nombre_usuario ?? "Usuario",
                                UrlFotoPerfil = p.foto_perfil ?? "who.jpg",
                                UrlFoto = p.url_foto ?? "imagennodisponible.png",
                                Descripcion = p.descripcion ?? string.Empty,
                                Ubicacion = p.ubicacion ?? string.Empty,
                                TiempoPublicacion = ObtenerTiempoRelativo(p.fecha_publicacion),
                                NumeroLikes = p.numero_likes,
                                NumeroComentarios = p.numero_comentarios
                            };
                            resultado.Add(viewModel);
                        }
                        return resultado;
                    }
                }
                else
                {
                    // Registrar el error para diagnóstico
                    Console.WriteLine($"Error HTTP: {response.StatusCode}");
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }

                return new List<PublicacionViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener publicaciones de usuario: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return new List<PublicacionViewModel>();
            }
        }

        public async Task<Tuple<bool, string, int>> CrearPublicacion(string descripcion, string ubicacion, Stream imagenStream, string nombreArchivo)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return Tuple.Create(false, "Usuario no autenticado", 0);
                }

                // Crear un MultipartFormDataContent para enviar la imagen
                var multipartContent = new MultipartFormDataContent();

                // Agregar la imagen como StreamContent
                var imageContent = new StreamContent(imagenStream);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    GetMimeType(Path.GetExtension(nombreArchivo)));
                multipartContent.Add(imageContent, "Imagen", nombreArchivo);

                // Agregar los demás campos
                multipartContent.Add(new StringContent(sesion.Usuario.id_usuario.ToString()), "IdUsuario");
                multipartContent.Add(new StringContent(descripcion ?? ""), "Descripcion");
                multipartContent.Add(new StringContent(ubicacion ?? ""), "Ubicacion");

                var response = await _httpClient.PostAsync("publicacion/conFoto", multipartContent);

                if (response.IsSuccessStatusCode)
                {
                    var publicacion = await response.Content.ReadFromJsonAsync<publicacion>();
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

        #endregion

        #region Fotos

        public async Task<List<foto>> ObtenerFotosPublicacion(int idPublicacion)
        {
            try
            {
                var response = await _httpClient.GetAsync($"foto/publicacion/{idPublicacion}");

                if (response.IsSuccessStatusCode)
                {
                    var fotos = await response.Content.ReadFromJsonAsync<List<foto>>();
                    return fotos ?? new List<foto>();
                }

                return new List<foto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener fotos: {ex.Message}");
                return new List<foto>();
            }
        }

        public async Task<Tuple<bool, string>> SubirFotoPerfil(Stream imagenStream, string nombreArchivo)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return Tuple.Create(false, "Usuario no autenticado");
                }

                // Crear un MultipartFormDataContent para enviar la imagen
                var multipartContent = new MultipartFormDataContent();

                // Agregar la imagen como StreamContent
                var imageContent = new StreamContent(imagenStream);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    GetMimeType(Path.GetExtension(nombreArchivo)));
                multipartContent.Add(imageContent, "Imagen", nombreArchivo);

                // Agregar los demás campos
                multipartContent.Add(new StringContent(sesion.Usuario.id_usuario.ToString()), "IdUsuario");

                var response = await _httpClient.PostAsync("usuario/subirFotoPerfil", multipartContent);

                if (response.IsSuccessStatusCode)
                {
                    var urlFoto = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(urlFoto))
                    {
                        return Tuple.Create(true, urlFoto);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                    ? "Error al subir foto de perfil"
                    : errorContent);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, $"Error: {ex.Message}");
            }
        }

        #endregion

        #region Comentarios

        public async Task<List<ComentarioViewModel>> ObtenerComentarios(int idFoto)
        {
            try
            {
                var response = await _httpClient.GetAsync($"comentario/foto/{idFoto}");

                if (response.IsSuccessStatusCode)
                {
                    var comentarios = await response.Content.ReadFromJsonAsync<List<ComentarioConDetalles>>();
                    if (comentarios != null)
                    {
                        return comentarios.Select(c => new ComentarioViewModel
                        {
                            Id = c.id_comentario,
                            NombreUsuario = c.nombre_usuario,
                            UrlFotoPerfil = c.foto_perfil ?? "default_user.png",
                            Contenido = c.contenido,
                            FechaComentario = ObtenerTiempoRelativo(c.fecha_comentario)
                        }).ToList();
                    }
                }

                return new List<ComentarioViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener comentarios: {ex.Message}");
                return new List<ComentarioViewModel>();
            }
        }

        public async Task<Tuple<bool, string>> AgregarComentario(int idFoto, string contenido)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return Tuple.Create(false, "Usuario no autenticado");
                }

                var response = await _httpClient.PostAsJsonAsync("comentario", new comentario
                {
                    id_foto = idFoto,
                    id_usuario = sesion.Usuario.id_usuario,
                    contenido = contenido,
                    fecha_comentario = DateTime.Now
                });

                if (response.IsSuccessStatusCode)
                {
                    return Tuple.Create(true, "Comentario agregado correctamente");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                    ? "Error al agregar comentario"
                    : errorContent);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, $"Error: {ex.Message}");
            }
        }

        #endregion

        #region Favoritos

        public async Task<bool> ToggleFavorito(int idFoto)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return false;
                }

                var favoritoModel = new
                {
                    IdUsuario = sesion.Usuario.id_usuario,
                    IdFoto = idFoto
                };

                var response = await _httpClient.PostAsJsonAsync("favorito/toggle", favoritoModel);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<bool>();
                    return resultado;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar favorito: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EsFavorito(int idFoto)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return false;
                }

                var response = await _httpClient.GetAsync($"favorito/verificar/{sesion.Usuario.id_usuario}/{idFoto}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<foto>> ObtenerFavoritos()
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return new List<foto>();
                }

                // Obtener favoritos del usuario
                var response = await _httpClient.GetAsync($"favorito/usuario/{sesion.Usuario.id_usuario}");

                if (response.IsSuccessStatusCode)
                {
                    var favoritos = await response.Content.ReadFromJsonAsync<List<favorito>>();
                    if (favoritos != null && favoritos.Count > 0)
                    {
                        // Obtener detalles de cada foto en paralelo para mejorar rendimiento
                        var tareasFotos = favoritos.Select(async fav => {
                            try {
                                var fotoResponse = await _httpClient.GetAsync($"foto/{fav.id_foto}");
                                if (fotoResponse.IsSuccessStatusCode)
                                {
                                    return await fotoResponse.Content.ReadFromJsonAsync<foto>();
                                }
                                return null;
                            }
                            catch {
                                return null; 
                            }
                        });
                        
                        var resultados = await Task.WhenAll(tareasFotos);
                        return resultados.Where(f => f != null).ToList();
                    }
                }
                else
                {
                    Console.WriteLine($"Error al obtener favoritos: {response.StatusCode}");
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }

                return new List<foto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener favoritos: {ex.Message}");
                return new List<foto>();
            }
        }

        #endregion

        #region Amigos

        public async Task<List<UsuarioViewModel>> ObtenerAmigos()
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return new List<UsuarioViewModel>();
                }

                var response = await _httpClient.GetAsync($"amigo/listar/{sesion.Usuario.id_usuario}");

                if (response.IsSuccessStatusCode)
                {
                    var amigos = await response.Content.ReadFromJsonAsync<List<usuario>>();
                    if (amigos != null)
                    {
                        return amigos.Select(a => new UsuarioViewModel
                        {
                            Id = a.id_usuario,
                            NombreUsuario = a.nombre_usuario,
                            FotoPerfil = a.foto_perfil ?? "default_user.png",
                            Biografia = a.biografia
                        }).ToList();
                    }
                }

                return new List<UsuarioViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener amigos: {ex.Message}");
                return new List<UsuarioViewModel>();
            }
        }

        public async Task<List<SolicitudAmistadViewModel>> ObtenerSolicitudesPendientes()
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return new List<SolicitudAmistadViewModel>();
                }

                var response = await _httpClient.GetAsync($"amigo/solicitudes/{sesion.Usuario.id_usuario}");

                if (response.IsSuccessStatusCode)
                {
                    var solicitudes = await response.Content.ReadFromJsonAsync<List<SolicitudAmistadDetalle>>();
                    if (solicitudes != null)
                    {
                        return solicitudes.Select(s => new SolicitudAmistadViewModel
                        {
                            Id = s.id_amigo,
                            IdUsuario = s.id_usuario,
                            NombreUsuario = s.nombre_usuario,
                            FotoPerfil = s.foto_perfil ?? "default_user.png",
                            FechaSolicitud = ObtenerTiempoRelativo(s.fecha_solicitud)
                        }).ToList();
                    }
                }

                return new List<SolicitudAmistadViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener solicitudes: {ex.Message}");
                return new List<SolicitudAmistadViewModel>();
            }
        }

        public async Task<Tuple<bool, string>> EnviarSolicitudAmistad(int idUsuarioDestino)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return Tuple.Create(false, "Usuario no autenticado");
                }

                var response = await _httpClient.PostAsJsonAsync("amigo", new amigo
                {
                    id_usuario = sesion.Usuario.id_usuario,
                    id_amigo_usuario = idUsuarioDestino,
                    estado = "pendiente",
                    fecha_solicitud = DateTime.Now
                });

                if (response.IsSuccessStatusCode)
                {
                    return Tuple.Create(true, "Solicitud enviada correctamente");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Tuple.Create(false, string.IsNullOrEmpty(errorContent)
                    ? "Error al enviar solicitud"
                    : errorContent);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, $"Error: {ex.Message}");
            }
        }

        public async Task<bool> AceptarSolicitudAmistad(int idSolicitud)
        {
            try
            {
                var response = await _httpClient.PutAsync($"amigo/aceptar/{idSolicitud}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Rechazar solicitud de amistad
        public async Task<bool> RechazarSolicitudAmistad(int idSolicitud)
        {
            try
            {
                var response = await _httpClient.PutAsync($"amigo/rechazar/{idSolicitud}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Alertas

        public async Task<List<AlertaViewModel>> ObtenerAlertas()
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return new List<AlertaViewModel>();
                }

                var response = await _httpClient.GetAsync($"alerta/recibidas/{sesion.Usuario.id_usuario}");

                if (response.IsSuccessStatusCode)
                {
                    var alertas = await response.Content.ReadFromJsonAsync<List<AlertaDetalle>>();
                    if (alertas != null)
                    {
                        return alertas.Select(a => new AlertaViewModel
                        {
                            Id = a.id_alerta,
                            IdUsuarioOrigen = a.id_usuario_origen,
                            NombreUsuarioOrigen = a.nombre_origen,
                            FotoPerfilOrigen = a.foto_perfil_origen ?? "default_user.png",
                            Comentario = a.comentario_alerta,
                            FechaAlerta = ObtenerTiempoRelativo(a.fecha_alerta),
                            Leida = a.estado_alerta
                        }).ToList();
                    }
                }

                return new List<AlertaViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener alertas: {ex.Message}");
                return new List<AlertaViewModel>();
            }
        }

        public async Task<bool> MarcarAlertaComoLeida(int idAlerta)
        {
            try
            {
                var response = await _httpClient.PutAsync($"alerta/marcarLeida/{idAlerta}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnviarAlerta(int idUsuarioDestino, string comentario)
        {
            try
            {
                var sesion = await ObtenerSesionActual();
                if (!sesion.SesionActiva || sesion.Usuario == null)
                {
                    return false;
                }

                var nuevaAlerta = new alerta
                {
                    id_usuario_origen = sesion.Usuario.id_usuario,
                    id_usuario_destino = idUsuarioDestino,
                    comentario_alerta = comentario,
                    fecha_alerta = DateTime.Now,
                    estado_alerta = false
                };

                var response = await _httpClient.PostAsJsonAsync("alerta", nuevaAlerta);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Clases auxiliares y métodos privados

        private string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                default:
                    return "application/octet-stream";
            }
        }

        private string ObtenerTiempoRelativo(DateTime fecha)
        {
            var ahora = DateTime.Now;
            var diferencia = ahora - fecha;

            if (diferencia.TotalMinutes < 1)
                return "Ahora mismo";
            if (diferencia.TotalMinutes < 60)
                return $"Hace {(int)diferencia.TotalMinutes} minutos";
            if (diferencia.TotalHours < 24)
                return $"Hace {(int)diferencia.TotalHours} horas";
            if (diferencia.TotalDays < 7)
                return $"Hace {(int)diferencia.TotalDays} días";

            return fecha.ToString("dd/MM/yyyy");
        }

        private PublicacionViewModel ConvertirAViewModel(PublicacionConDetalles p)
        {
            return new PublicacionViewModel
            {
                Id = p.id_publicacion,
                IdUsuario = p.id_usuario,
                NombreUsuario = p.nombre_usuario,
                UrlFotoPerfil = p.foto_perfil ?? "who.jpg",
                UrlFoto = p.url_foto,
                Descripcion = p.descripcion ?? string.Empty,
                Ubicacion = p.ubicacion ?? string.Empty,
                TiempoPublicacion = ObtenerTiempoRelativo(p.fecha_publicacion),
                NumeroLikes = p.numero_likes,
                NumeroComentarios = p.numero_comentarios
            };
        }

        // Clases para deserializar respuestas más complejas
        private class PublicacionConDetalles : publicacion
        {
            public string nombre_usuario { get; set; }
            public string foto_perfil { get; set; }
            public string url_foto { get; set; }
            public int numero_likes { get; set; }
            public int numero_comentarios { get; set; }
        }

        private class ComentarioConDetalles : comentario
        {
            public string nombre_usuario { get; set; }
            public string foto_perfil { get; set; }
        }

        private class SolicitudAmistadDetalle : amigo
        {
            public string nombre_usuario { get; set; }
            public string foto_perfil { get; set; }
        }

        private class AlertaDetalle : alerta
        {
            public string nombre_origen { get; set; }
            public string foto_perfil_origen { get; set; }
        }

        #endregion
    }

    #region ViewModels

    public class PublicacionViewModel
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string UrlFotoPerfil { get; set; } = "";
        public string TiempoPublicacion { get; set; } = "";
        public string UrlFoto { get; set; } = "";
        public int NumeroLikes { get; set; }
        public int NumeroComentarios { get; set; }
        public string Descripcion { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public bool EsFavorito { get; set; } = false;

        // Propiedad para manejar dinámicamente el ícono de favorito
        public string IconoFavorito => EsFavorito ? "starsolid.png" : "starregular.png";
    }

    public class ComentarioViewModel
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string UrlFotoPerfil { get; set; } = "";
        public string Contenido { get; set; } = "";
        public string FechaComentario { get; set; } = "";
    }

    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string FotoPerfil { get; set; } = "";
        public string Biografia { get; set; } = "";
        public string Pais { get; set; } = "";
    }

    public class SolicitudAmistadViewModel
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string FotoPerfil { get; set; } = "";
        public string FechaSolicitud { get; set; } = "";
    }

    public class AlertaViewModel
    {
        public int Id { get; set; }
        public int IdUsuarioOrigen { get; set; }
        public string NombreUsuarioOrigen { get; set; } = "";
        public string FotoPerfilOrigen { get; set; } = "";
        public string Comentario { get; set; } = "";
        public string FechaAlerta { get; set; } = "";
        public bool Leida { get; set; }
    }

    #endregion
}