using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class publicacionController : GenericController<publicacion>
    {
        public publicacionController() : base(Parametros.FabricaRepository.PublicacionRepository())
        {
        }
    }
}
