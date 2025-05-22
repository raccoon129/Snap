using DAL;
using Amazon;
using Amazon.S3;

namespace WebAPI
{
    public static class Parametros
    {
        //Conexión a la base de datos

#if DEBUG
        // Mientras sigue en debug
        //public static string CadenaDeConexion = @"Server=localhost;Database=snap;Uid=root;Pwd=;";
        public static string CadenaDeConexion = @"Server=mysql-drift3.alwaysdata.net;Database=drift3_snap;Uid=drift3;Pwd=xhjMz7BuB6PwRpy;";
        public static TipoDB TipoDB = TipoDB.MySQL;
#else
        //En producción con Release
 
        public static string CadenaDeConexion = @"Server=mysql-drift3.alwaysdata.net;Database=drift3_snap;Uid=drift3;Pwd=xhjMz7BuB6PwRpy;";
        public static TipoDB TipoDB = TipoDB.MySQL;

#endif
        public static FabricRepository FabricaRepository = new FabricRepository(CadenaDeConexion, TipoDB);
    }
}
