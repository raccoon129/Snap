using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class usuarioController : GenericController<usuario>
    {
        public usuarioController() : base(Parametros.FabricaRepository.UsuarioRepository())
        {
        }

        // Iniciar sesión

        /*DELIMITER //
        CREATE PROCEDURE sp_iniciar_sesion(IN p_identificador VARCHAR(100), IN p_pin VARCHAR(50))
        BEGIN
        SELECT * FROM usuario 
        WHERE (email = p_identificador OR telefono = p_identificador) 
        AND pin_contacto = p_pin 
        LIMIT 1;
        END //
        DELIMITER ;
        */
        // El procedimiento almacenado sp_iniciar_sesion se encarga de validar las credenciales del usuario.
        // Si las credenciales son correctas, devuelve el usuario correspondiente.| 
        [HttpPost("iniciarSesion")]
public ActionResult<usuario> IniciarSesion([FromBody] LoginModel modelo)
{
    try
    {
        var parametros = new Dictionary<string, string>
        {
            { "p_identificador", modelo.Identificador },
            { "p_pin", modelo.Pin }
        };

        var resultado = _repositorio.EjecutarProcedimiento<usuario>("sp_iniciar_sesion", parametros);

        if (resultado != null && resultado.Count > 0)
        {
            return Ok(resultado[0]);
        }
        else
        {
            return BadRequest("Credenciales incorrectas");
        }
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

// Buscar usuarios por nombre
[HttpGet("buscar/{nombre}")]
public ActionResult<List<usuario>> BuscarPorNombre(string nombre)
{
    try
    {
        var parametros = new Dictionary<string, string>
        {
            { "p_nombre", nombre }
        };

        var resultado = _repositorio.EjecutarProcedimiento<usuario>("sp_buscar_usuarios", parametros);

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

// Obtener publicaciones de un usuario
[HttpGet("{id}/publicaciones")]
public ActionResult<List<publicacion>> ObtenerPublicacionesUsuario(string id)
{
    try
    {
        var parametros = new Dictionary<string, string>
        {
            { "p_id_usuario", id }
        };

        var resultado = _repositorio.EjecutarProcedimiento<publicacion>("sp_publicaciones_usuario", parametros);

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

public class LoginModel
{
public string Identificador { get; set; } // Email o teléfono
public string Pin { get; set; }
}
}