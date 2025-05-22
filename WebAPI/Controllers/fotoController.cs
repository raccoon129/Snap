using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebAPI.Servicios;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class fotoController : GenericController<foto>
    {
        private readonly S3StorageService _storageService;

        public fotoController() : base(Parametros.FabricaRepository.FotoRepository())
        {
            _storageService = new S3StorageService();
        }

        // Obtener fotos de una publicación
        [HttpGet("publicacion/{idPublicacion}")]
        public ActionResult<List<foto>> ObtenerFotosPublicacion(string idPublicacion)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_publicacion", idPublicacion }
                };

                var resultado = _repositorio.EjecutarProcedimiento<foto>("sp_fotos_publicacion", parametros);

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

        // Obtener fotos de un usuario
        [HttpGet("usuario/{idUsuario}")]
        public ActionResult<List<foto>> ObtenerFotosUsuario(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<foto>("sp_fotos_usuario", parametros);

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
        
        //21 de Mayo

        // Subir foto (guardar en S3 y en base de datos)
        [HttpPost("upload")]
        public async Task<ActionResult<foto>> SubirFoto([FromForm] SubirFotoModel modelo)
        {
            try
            {
                if (modelo.Imagen == null || modelo.Imagen.Length == 0)
                    return BadRequest("No se ha proporcionado una imagen válida");
                
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
                
                // Guardar la foto en la base de datos
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", modelo.IdUsuario.ToString() },
                    { "p_id_publicacion", modelo.IdPublicacion.ToString() },
                    { "p_url_foto", url }
                };

                var resultado = _repositorio.EjecutarProcedimiento<foto>("sp_cargar_foto", parametros);
                
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
                return BadRequest($"Error al subir la foto: {ex.Message}");
            }
        }
    }

    public class SubirFotoModel
    {
        public IFormFile Imagen { get; set; }
        public int IdUsuario { get; set; }
        public int IdPublicacion { get; set; }
    }
}