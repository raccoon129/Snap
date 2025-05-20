using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class publicacionController : GenericController<publicacion>
    {
        public publicacionController() : base(Parametros.FabricaRepository.PublicacionRepository())
        {
        }

        // Obtener feed de publicaciones de amigos
        [HttpGet("feed/{idUsuario}")]
        public ActionResult<List<publicacion>> ObtenerFeed(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<publicacion>("sp_feed_publicaciones", parametros);

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
    }
}