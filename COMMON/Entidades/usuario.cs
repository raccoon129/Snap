using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class usuario : camposControl
    {
        public int id_usuario { get; set; }

        public string nombre_usuario { get; set; }
        public string? biografia { get; set; }
        public string? email { get; set; }

        public string telefono { get; set; }

        public string? pais { get; set; }

        public string? foto_perfil { get; set; }

        public string estado { get; set; }

        public DateTime ultima_conexion { get; set; }

        public string pin_contacto { get; set; }

        public DateTime fecha_creacion { get; set; }
        public enum rol
        {
            usuario,
            administrador
        }
    }
}
