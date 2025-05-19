using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class comentario :camposControl
    {
        public int id_comentario { get; set; }

        public int id_foto { get; set; }
        public int id_usuario { get; set; }
        public string contenido { get; set; }

        public DateTime fecha_comentario { get; set; }
    }
}
