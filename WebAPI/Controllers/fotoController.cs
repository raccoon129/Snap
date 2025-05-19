using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class fotoController : GenericController<foto>
    {
        public fotoController() : base(Parametros.FabricaRepository.FotoRepository())
        {
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
    }
}