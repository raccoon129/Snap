using COMMON.Entidades;
using COMMON.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class GenericController<T> : ControllerBase where T : camposControl
    {

        protected IDB<T> _repositorio;
        public GenericController(IDB<T> repositorio)
        {
            _repositorio = repositorio;//inyeccion de dependencias
        }

        //obtener todos
        [HttpGet]
        public ActionResult<List<T>> Get()
        {
            try
            {
                var datos = _repositorio.ObtenerTodas();
                if (datos != null)
                {
                    return Ok(datos);
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
        //obtener por id
        [HttpGet("{id}")]
        public ActionResult<T> GetById(string id)
        {
            try
            {
                var datos = _repositorio.ObtenerPorID(id);
                if (datos != null)
                {
                    return Ok(datos);
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
        //insertar
        [HttpPost]
        public ActionResult<T> Post([FromBody] T datos)
        {
            try
            {
                var resultado = _repositorio.Insertar(datos);
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
        //actualizar
        [HttpPut]
        public ActionResult<T> Put([FromBody] T entidad)
        {
            try
            {
                var datos = _repositorio.Actualizar(entidad);
                if (datos != null)
                {
                    return Ok(datos);
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
        //eliminar
        [HttpDelete]
        public ActionResult<bool> Delete([FromBody] T entidad)
        {
            try
            {
                var datos = _repositorio.Eliminar(entidad);
                if (datos)
                {
                    return Ok(true);
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