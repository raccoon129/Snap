using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class favorito : camposControl
    {
        public int id_favorito { get; set; }

        public int id_foto { get; set; }
        public int id_usuario { get; set; }

        public DateTime fecha_favorito { get; set; }
    }
}
