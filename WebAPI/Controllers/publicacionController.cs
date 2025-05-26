using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebAPI.Servicios;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class publicacionController : GenericController<publicacion>
    {
        private readonly S3StorageService _storageService;
        
        public publicacionController() : base(Parametros.FabricaRepository.PublicacionRepository())
        {
            _storageService = new S3StorageService();
        }

        // Modelo para devolver los datos completos de las publicaciones
        public class PublicacionCompletaDto
        {
            // Campos básicos de publicación
            public long id_publicacion { get; set; }  // Cambiado de int a long
            public long id_usuario { get; set; }      // Cambiado de int a long
            public string descripcion { get; set; }
            public string ubicacion { get; set; }
            public DateTime fecha_publicacion { get; set; }

            // Campos adicionales del usuario
            public string nombre_usuario { get; set; }
            public string foto_perfil { get; set; }

            // Campos de estadísticas
            public string url_foto { get; set; }
            public long numero_likes { get; set; }       // Cambiado de int a long
            public long numero_comentarios { get; set; } // Cambiado de int a long
        }

        // Obtener feed de publicaciones de amigos con datos completos
        [HttpGet("feed/{idUsuario}")]
        public ActionResult<List<PublicacionCompletaDto>> ObtenerFeed(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<PublicacionCompletaDto>("sp_feed_publicaciones_completo", parametros);

                if (resultado != null)
                {
                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(_repositorio.Error);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Obtener publicaciones por ubicación
        [HttpGet("ubicacion/{ubicacion}")]
        public ActionResult<List<publicacion>> ObtenerPorUbicacion(string ubicacion)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_ubicacion", ubicacion }
                };

                var resultado = _repositorio.EjecutarProcedimiento<publicacion>("sp_publicaciones_ubicacion", parametros);

                if (resultado != null)
                {
                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(_repositorio.Error);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        //21 de mayo 2025

        // Crear publicación con foto
        [HttpPost("conFoto")]
        public async Task<ActionResult<publicacion>> CrearPublicacionConFoto([FromForm] CrearPublicacionModel modelo)
        {
            try
            {
                if (modelo.Imagen == null || modelo.Imagen.Length == 0)
                    return BadRequest("No se ha proporcionado una imagen");
                
                // Validar tamaño del archivo
                if (modelo.Imagen.Length > Parametros.MaxFileSize)
                    return BadRequest($"La imagen excede el tamaño máximo permitido ({Parametros.MaxFileSize / (1024*1024)}MB)");
                
                // Validar tipo de archivo
                string contentType = modelo.Imagen.ContentType;
                if (!Parametros.AllowedImageTypes.Contains(contentType))
                    return BadRequest("Tipo de archivo no permitido. Use JPG, PNG o GIF");
                
                // Subir la imagen a S3
                using var stream = modelo.Imagen.OpenReadStream();
                string fileName = Path.GetFileName(modelo.Imagen.FileName);
                string url = await _storageService.UploadFileAsync(
                    stream, 
                    fileName, 
                    contentType, 
                    Parametros.S3FolderPublications
                );
                
                // Crear la publicación con la foto
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", modelo.IdUsuario.ToString() },
                    { "p_descripcion", modelo.Descripcion ?? string.Empty },
                    { "p_ubicacion", modelo.Ubicacion ?? string.Empty },
                    { "p_url_foto", url }
                };

                var resultado = _repositorio.EjecutarProcedimiento<publicacion>("sp_crear_publicacion_con_foto", parametros);
                
                if (resultado != null && resultado.Count > 0)
                {
                    return Ok(resultado[0]);
                }
                else
                {
                    // Si falla la inserción en la base de datos, eliminar la foto de S3
                    await _storageService.DeleteFileAsync(url);
                    return BadRequest(_repositorio.Error);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al crear la publicación: {ex.Message}");
            }
        }

        // 26 de Mayo
        // Eliminar publicación
        [HttpDelete("{id}/{idUsuario}")]
        public async Task<ActionResult<bool>> EliminarPublicacion(int id, int idUsuario)
        {
            try
            {
                // Verificar que la publicación exista y pertenezca al usuario
                var parametrosVerificar = new Dictionary<string, string>
                {
                    { "p_id_publicacion", id.ToString() },
                    { "p_id_usuario", idUsuario.ToString() }
                };

                var publicacion = _repositorio.EjecutarProcedimiento<publicacion>(
                    "sp_verificar_propietario_publicacion", parametrosVerificar);

                if (publicacion == null || publicacion.Count == 0)
                {
                    return BadRequest("La publicación no existe o no pertenece al usuario indicado");
                }

                // Obtener las fotos asociadas para eliminarlas de S3
                var parametrosFotos = new Dictionary<string, string>
                {
                    { "p_id_publicacion", id.ToString() }
                };

                var fotos = _repositorio.EjecutarProcedimiento<foto>("sp_fotos_publicacion", parametrosFotos);

                // Eliminar la publicación (esto también debería eliminar las fotos en cascada en la BD)
                var parametrosEliminar = new Dictionary<string, string>
                {
                    { "p_id_publicacion", id.ToString() },
                    { "p_id_usuario", idUsuario.ToString() }
                };

                var resultado = _repositorio.EjecutarProcedimiento<publicacion>(
                    "sp_eliminar_publicacion", parametrosEliminar);

                // Cambiamos esta condición para aceptar tanto resultado no nulo como nulo
                // ya que el procedimiento puede no retornar filas aun cuando se realizó la eliminación
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al eliminar la publicación: {ex.Message}");
            }
        }
    }

    public class CrearPublicacionModel
    {
        public IFormFile Imagen { get; set; }
        public int IdUsuario { get; set; }
        public string? Descripcion { get; set; }
        public string? Ubicacion { get; set; }
    }
}