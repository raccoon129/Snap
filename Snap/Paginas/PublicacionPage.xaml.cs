using Snap.Servicios;
using System.Collections.ObjectModel;

namespace Snap.Paginas;

[QueryProperty(nameof(PublicacionId), "id")]
public partial class PublicacionPage : ContentPage
{
    private readonly ApiService _apiService;
    private int _publicacionId;
    private int _fotoId;
    private ObservableCollection<ComentarioViewModel> _comentarios = new ObservableCollection<ComentarioViewModel>();
    private bool _esFavorito = false;

    private bool _esUsuarioPropietario = false;

    public PublicacionPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        ListaComentarios.ItemsSource = _comentarios;
    }

    public int PublicacionId
    {
        get => _publicacionId;
        set
        {
            _publicacionId = value;
            CargarPublicacion();
        }
    }

    private async void CargarPublicacion()
    {
        try
        {
            // Obtener la sesión del usuario actual
            var sesion = await _apiService.ObtenerSesionActual();

            // Mostrar indicador de carga (si existe)
            if (LoadingIndicator != null)
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;
            }

            // Obtener las fotos de la publicación
            var fotos = await _apiService.ObtenerFotosPublicacion(_publicacionId);
            if (fotos == null || fotos.Count == 0)
            {
                ImgPublicacion.Source = "imagennodisponible.png";
                await DisplayAlert("Error", "No se encontraron fotos para esta publicación", "OK");
                return;
            }

            // Tomar la primera foto (generalmente solo habrá una por publicación)
            var foto = fotos[0];
            _fotoId = foto.id_foto;

            // Intentar obtener la publicación del feed
            var publicaciones = await _apiService.ObtenerPublicaciones();
            var publicacion = publicaciones.FirstOrDefault(p => p.Id == _publicacionId);

            if (publicacion == null)
            {
                // Si no se encuentra en el feed, intentar obtenerla directamente
                //var sesion = await _apiService.ObtenerSesionActual();
                if (sesion.SesionActiva && sesion.Usuario != null)
                {
                    var publicacionesUsuario = await _apiService.ObtenerPublicacionesDeUsuario(sesion.Usuario.id_usuario);
                    publicacion = publicacionesUsuario.FirstOrDefault(p => p.Id == _publicacionId);
                }
            }

            if (publicacion != null)
            {

                // Verificar si el usuario actual es el propietario
                _esUsuarioPropietario = sesion.SesionActiva &&
                                       sesion.Usuario != null &&
                                       publicacion.IdUsuario == sesion.Usuario.id_usuario;

                // Mostrar u ocultar el botón de opciones según corresponda
                BtnOpciones.IsVisible = _esUsuarioPropietario;


                // Cargar los datos de la publicación
                ImgPerfilUsuario.Source = !string.IsNullOrEmpty(publicacion.UrlFotoPerfil)
                    ? publicacion.UrlFotoPerfil
                    : "who.jpg";

                LblNombreUsuario.Text = $"@{publicacion.NombreUsuario}";
                LblTiempoPublicacion.Text = publicacion.TiempoPublicacion;

                ImgPublicacion.Source = !string.IsNullOrEmpty(foto.url_foto)
                    ? foto.url_foto
                    : "imagennodisponible.png";

                LblNumeroLikes.Text = publicacion.NumeroLikes.ToString();
                LblNumeroComentarios.Text = publicacion.NumeroComentarios.ToString();
                LblDescripcion.Text = !string.IsNullOrEmpty(publicacion.Descripcion)
                    ? publicacion.Descripcion
                    : "";

                // Verificar si esta foto es favorita para el usuario actual
                _esFavorito = await _apiService.EsFavorito(_fotoId);
                ActualizarIconoFavorito();

                // Cargar los comentarios
                await CargarComentarios();
            }
            else
            {
                ImgPerfilUsuario.Source = "who.jpg";
                ImgPublicacion.Source = !string.IsNullOrEmpty(foto.url_foto)
                    ? foto.url_foto
                    : "imagennodisponible.png";
                LblNombreUsuario.Text = "Usuario";
                LblDescripcion.Text = "Descripción no disponible";

                await DisplayAlert("Advertencia", "No se pudieron cargar todos los detalles de la publicación", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar publicación: {ex.Message}");
            await DisplayAlert("Error", $"No se pudo cargar la publicación: {ex.Message}", "OK");

            // Establecer valores por defecto
            ImgPerfilUsuario.Source = "who.jpg";
            ImgPublicacion.Source = "imagennodisponible.png";
        }
        finally
        {
            // Ocultar indicador de carga
            if (LoadingIndicator != null)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }

    private async Task CargarComentarios()
    {
        try
        {
            _comentarios.Clear();

            var comentarios = await _apiService.ObtenerComentarios(_fotoId);

            if (comentarios != null && comentarios.Count > 0)
            {
                foreach (var comentario in comentarios)
                {
                    _comentarios.Add(new ComentarioViewModel
                    {
                        NombreUsuario = comentario.NombreUsuario,
                        UrlFotoPerfil = !string.IsNullOrEmpty(comentario.UrlFotoPerfil)
                            ? comentario.UrlFotoPerfil
                            : "who.jpg",
                        Contenido = comentario.Contenido,
                        FechaComentario = comentario.FechaComentario
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar comentarios: {ex.Message}");
            // No mostrar el error al usuario 
        }
    }

    private void ActualizarIconoFavorito()
    {
        // Actualizar el icono según el estado actual
        if (BtnMeGusta != null)
        {
            BtnMeGusta.Source = _esFavorito ? "starsolid.png" : "starregular.png";
        }
    }

    private async void OnMeGustaClicked(object sender, EventArgs e)
    {
        try
        {
            // Toggle el estado de favorito
            bool resultado = await _apiService.ToggleFavorito(_fotoId);

            // Si la operación fue exitosa, actualizamos el UI
            _esFavorito = !_esFavorito;
            ActualizarIconoFavorito();

            // Actualizar el contador de likes
            int likesActuales = int.Parse(LblNumeroLikes.Text);
            LblNumeroLikes.Text = (_esFavorito ? likesActuales + 1 : likesActuales - 1).ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo procesar la acción: {ex.Message}", "OK");
        }
    }

    private async void OnEnviarComentarioClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryComentario.Text))
            return;

        try
        {
            var resultado = await _apiService.AgregarComentario(_fotoId, EntryComentario.Text);

            if (resultado.Item1)
            {
                // Añadir el comentario al listado local
                var sesion = await _apiService.ObtenerSesionActual();
                _comentarios.Add(new ComentarioViewModel
                {
                    NombreUsuario = $"@{sesion.Usuario.nombre_usuario}",
                    UrlFotoPerfil = !string.IsNullOrEmpty(sesion.Usuario.foto_perfil)
                        ? sesion.Usuario.foto_perfil
                        : "who.jpg",
                    Contenido = EntryComentario.Text,
                    FechaComentario = "Ahora mismo"
                });

                // Limpiar el campo de entrada
                EntryComentario.Text = string.Empty;

                // Actualizar el contador de comentarios
                int comentariosActuales = int.Parse(LblNumeroComentarios.Text);
                LblNumeroComentarios.Text = (comentariosActuales + 1).ToString();
            }
            else
            {
                await DisplayAlert("Error", resultado.Item2, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo añadir el comentario: {ex.Message}", "OK");
        }
    }
    // Implementar el manejador del botón de opciones
    private async void OnOpcionesClicked(object sender, EventArgs e)
    {
        if (!_esUsuarioPropietario)
            return;

        // Mostrar opciones
        bool eliminar = await DisplayActionSheet("Opciones", "Cancelar", null, "Eliminar publicación") == "Eliminar publicación";

        if (eliminar)
        {
            // Pedir confirmación
            bool confirmar = await DisplayAlert("Eliminar publicación",
                "¿Estás seguro de que deseas eliminar esta publicación? Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (confirmar)
            {
                // Mostrar indicador de carga
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                try
                {
                    bool eliminado = await _apiService.EliminarPublicacion(_publicacionId);

                    if (eliminado)
                    {
                        await DisplayAlert("Éxito", "La publicación ha sido eliminada", "OK");
                        // Navegar a la página de inicio
                        await Shell.Current.GoToAsync("///Inicio");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar la publicación", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al eliminar la publicación: {ex.Message}", "OK");
                }
                finally
                {
                    LoadingIndicator.IsRunning = false;
                    LoadingIndicator.IsVisible = false;
                }
            }
        }
    }
}