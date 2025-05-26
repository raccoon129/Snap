using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class amigoController : GenericController<amigo>
    {
        public amigoController() : base(Parametros.FabricaRepository.AmigoRepository())
        {
        }

        // Obtener amigos de un usuario
        [HttpGet("listar/{idUsuario}")]
        public ActionResult<List<usuario>> ObtenerAmigos(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<usuario>("sp_amigos_usuario", parametros);

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

        // Obtener solicitudes pendientes de amistad
        [HttpGet("solicitudes/{idUsuario}")]
        public ActionResult<List<amigo>> ObtenerSolicitudesPendientes(string idUsuario)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_usuario", idUsuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<amigo>("sp_solicitudes_pendientes", parametros);

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

        // Aceptar solicitud de amistad
        [HttpPut("aceptar/{idSolicitud}")]
        public ActionResult<amigo> AceptarSolicitud(string idSolicitud)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_id_amigo", idSolicitud }
                };

                var resultado = _repositorio.EjecutarProcedimiento<amigo>("sp_aceptar_solicitud", parametros);

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
        // Rechazar solicitud de amistad
        [HttpPut("rechazar/{idSolicitud}")]
        public ActionResult<amigo> RechazarSolicitud(string idSolicitud)
        {
            try
            {
                var parametros = new Dictionary<string, string>
        {
            { "p_id_amigo", idSolicitud }
        };

                var resultado = _repositorio.EjecutarProcedimiento<amigo>("sp_rechazar_solicitud", parametros);

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

