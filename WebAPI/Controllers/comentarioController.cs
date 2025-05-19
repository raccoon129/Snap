using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class comentarioController : GenericController<comentario>
    {
        public comentarioController() : base(Parametros.FabricaRepository.ComentarioRepository())
        {
        }

        // Obtener comentarios de una foto
        [HttpGet("foto/{idFoto}")]
        public ActionResult<List<comentario>> ObtenerComentariosFoto(string idFoto)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_foto", idFoto }
                };

                var resultado = _repositorio.EjecutarProcedimiento<comentario>("sp_comentarios_foto", parametros);

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