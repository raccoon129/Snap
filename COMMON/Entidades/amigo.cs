using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class amigo : camposControl
    {
        public int id_amigo { get; set; }

        public int id_usuario { get; set; }
        public int id_amigo_usuario { get; set; }

        public string estado { get; set; }

        public DateTime fecha_solicitud { get; set; }

        public DateTime? fecha_aceptacion { get; set; }
    }
}
