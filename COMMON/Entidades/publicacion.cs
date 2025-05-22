using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class publicacion : camposControl
    {
        public int id_publicacion { get; set; }
        public int id_usuario { get; set; }

        public string? descripcion { get; set; }
        public string? ubicacion { get; set; }
        public DateTime fecha_publicacion { get; set; } = DateTime.Now;
    }
}
