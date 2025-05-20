using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON
{
    public static class Params
    {
        public static string UsuarioConectado = "APIUser";
#if DEBUG
        public static string UrlAPI = @"http://localhost:5131/";
#else
        public static string UrlAPI = @"http://1475api.runasp.net/";
#endif
    }
}
