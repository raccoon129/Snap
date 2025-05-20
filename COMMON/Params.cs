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
        //public static string UrlAPI = @"http://localhost:5131/";
        public static string UrlAPI = @"https://webapi-snap-fxgrc8fne3dvdqbj.canadacentral-01.azurewebsites.net/";
#else
        public static string UrlAPI = @"https://webapi-snap-fxgrc8fne3dvdqbj.canadacentral-01.azurewebsites.net/";
#endif
    }
}
