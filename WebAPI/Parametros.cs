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

        // Configuración AWS S3 para desarrollo
        public static string S3BucketName = "snap-xires";
        public static string S3Region = RegionEndpoint.USEast2.SystemName;
        public static string S3AccessKey = "AKIAWX2IFOHLTXOHA6HI";
        public static string S3SecretKey = "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";
        public static string S3BaseUrl = "https://snap-xires.s3.us-east-2.amazonaws.com/";

#else
            //En producción con Release
     
            public static string CadenaDeConexion = @"Server=mysql-drift3.alwaysdata.net;Database=drift3_snap;Uid=drift3;Pwd=xhjMz7BuB6PwRpy;";
            public static TipoDB TipoDB = TipoDB.MySQL;


            // Configuración AWS S3 para producción
            public static string S3BucketName = "snap-xires";
            public static string S3Region = RegionEndpoint.USEast2.SystemName;
            public static string S3AccessKey = "AKIAWX2IFOHLTXOHA6HI";
            public static string S3SecretKey = "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";
            public static string S3BaseUrl = "https://snap-xires.s3.us-east-2.amazonaws.com/";

#endif
        public static FabricRepository FabricaRepository = new FabricRepository(CadenaDeConexion, TipoDB);


        // Carpetas para organizar archivos en S3
        public static string S3FolderProfilePics = "profile-pics/";
        public static string S3FolderPublications = "publications/";

        // Tamaño máximo de archivo en bytes (5MB)
        public static long MaxFileSize = 5 * 1024 * 1024;

        // Tipos MIME permitidos para imágenes
        public static readonly string[] AllowedImageTypes = {
                "image/jpeg", "image/jpg", "image/png", "image/gif"
            };

    }
}
