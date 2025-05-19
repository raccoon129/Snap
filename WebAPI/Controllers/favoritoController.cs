using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class favoritoController : GenericController<favorito>
    {
        public favoritoController() : base(Parametros.FabricaRepository.FavoritoRepository())
        {
        }

        // Obtener favoritos de un usuario
        [HttpGet("usuario/{idUsuario}")]
        public ActionResult<List<favorito>> ObtenerFavoritosUsuario(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<favorito>("sp_favoritos_usuario", parametros);

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

        // Verificar si una foto es favorita para un usuario
        [HttpGet("verificar/{idUsuario}/{idFoto}")]
        public ActionResult<bool> VerificarFavorito(string idUsuario, string idFoto)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario },
                    { "p_id_foto", idFoto }
                };

                var resultado = _repositorio.EjecutarProcedimiento<dynamic>("sp_verificar_favorito", parametros);

                if (resultado != null && resultado.Count > 0)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}