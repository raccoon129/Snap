using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class alertaController : GenericController<alerta>
    {
        public alertaController() : base(Parametros.FabricaRepository.AlertaRepository())
        {
        }

        // Obtener alertas recibidas por un usuario
        [HttpGet("recibidas/{idUsuario}")]
        public ActionResult<List<alerta>> ObtenerAlertasRecibidas(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario_destino", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<alerta>("sp_alertas_recibidas", parametros);

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

        // Marcar alerta como leída
        [HttpPut("marcarLeida/{idAlerta}")]
        public ActionResult<alerta> MarcarComoLeida(string idAlerta)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_alerta", idAlerta }
                };

                var resultado = _repositorio.EjecutarProcedimiento<alerta>("sp_marcar_alerta_leida", parametros);

                if (resultado != null && resultado.Count > 0)
                {
                    return Ok(resultado[0]);
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