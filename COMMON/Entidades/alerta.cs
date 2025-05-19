using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class alerta : camposControl
    {
        public int id_alerta { get; set; }

        public int id_usuario_origen { get; set; }
        public int id_usuario_destino { get; set; }

        public DateTime fecha_alerta { get; set; }

        public bool estado_alerta { get; set; }

        public string comentario_alerta { get; set; }
    }
}
