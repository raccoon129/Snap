using DAL;

namespace WebAPI
{
    public static class Parametros
    {
        //Conexión a la base de datos

#if DEBUG
        // Mientras sigue en debug
        public static string CadenaDeConexion = @"Server=localhost;Database=snap;Uid=root;Pwd=;";
        public static TipoDB TipoDB = TipoDB.MySQL;
#else
        //En producción con Release
 
public static TipoDB TipoDB = TipoDB.MySQL;

#endif
        public static FabricRepository FabricaRepository = new FabricRepository(CadenaDeConexion, TipoDB);
    }
}
