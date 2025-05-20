using System.Collections.ObjectModel;

namespace Snap.Paginas;

[QueryProperty(nameof(PublicacionId), "id")]
public partial class PublicacionPage : ContentPage
{
    private int _publicacionId;
    private ObservableCollection<ComentarioViewModel> _comentarios = new ObservableCollection<ComentarioViewModel>();

    public int PublicacionId
    {
        get => _publicacionId;
        set
        {
            _publicacionId = value;
            CargarPublicacion();
        }
    }

    public PublicacionPage()
    {
        InitializeComponent();
        ListaComentarios.ItemsSource = _comentarios;
    }

    private async void CargarPublicacion()
    {
        try
        {
            // En una implementación real, cargaríamos los datos de la API
            // Por ahora usamos datos de ejemplo basados en el wireframe
            ImgPerfilUsuario.Source = "https://randomuser.me/api/portraits/men/32.jpg";
            LblNombreUsuario.Text = "@nombre_random";
            LblTiempoPublicacion.Text = "Hace 1 hora";
            ImgPublicacion.Source = "https://picsum.photos/500/500?random=1";
            LblNumeroLikes.Text = "3";
            LblNumeroComentarios.Text = "1";
            LblDescripcion.Text = "Aquinombre el tag...";

            // Comentarios de ejemplo
            _comentarios.Add(new ComentarioViewModel
            {
                NombreUsuario = "@usuario1",
                UrlFotoPerfil = "https://randomuser.me/api/portraits/women/44.jpg",
                Comentario = "¡Muy buena foto!",
                TiempoComentario = "Hace 30 minutos"
            });

            _comentarios.Add(new ComentarioViewModel
            {
                NombreUsuario = "@usuario2",
                UrlFotoPerfil = "https://randomuser.me/api/portraits/men/44.jpg",
                Comentario = "Me encanta esta vista",
                TiempoComentario = "Hace 10 minutos"
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar la publicación: {ex.Message}", "OK");
        }
    }
}

public class ComentarioViewModel
{
    public string NombreUsuario { get; set; } = "";
    public string UrlFotoPerfil { get; set; } = "";
    public string Comentario { get; set; } = "";
    public string TiempoComentario { get; set; } = "";
}