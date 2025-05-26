using COMMON.Entidades;
using System;

namespace Snap.Modelos
{
    public class SesionUsuario
    {
        public usuario Usuario { get; set; }
        public bool EstaAutenticado { get; set; }
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }

        // En SesionUsuario.cs
        public bool SesionActiva => EstaAutenticado && Usuario != null;
    }
}