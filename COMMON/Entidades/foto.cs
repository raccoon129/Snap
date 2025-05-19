using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class foto : camposControl
    {
        public int id_foto { get; set; }

        public int id_usuario { get; set; }

        public string url_foto { get; set; }

        public DateTime fecha_subida { get; set; } = DateTime.Now;
    }
}
