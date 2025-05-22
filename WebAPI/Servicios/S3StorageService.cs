using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.IO;

namespace WebAPI.Servicios
{
    public class S3StorageService
    {
        private readonly IAmazonS3 _s3Client;

        public S3StorageService()
        {
            _s3Client = new AmazonS3Client(
                Parametros.S3AccessKey,
                Parametros.S3SecretKey,
                RegionEndpoint.GetBySystemName(Parametros.S3Region)
            );
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
        {
            try
            {
                // Generar un nombre único para evitar colisiones
                string uniqueFileName = $"{Guid.NewGuid()}_{fileName.Replace(" ", "_")}";
                string key = $"{folder}{uniqueFileName}";

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = key,
                    BucketName = Parametros.S3BucketName,
                    CannedACL = S3CannedACL.PublicRead,
                    ContentType = contentType
                };

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                // Construir la URL del archivo subido
                return $"{Parametros.S3BaseUrl}{key}";
            }
            catch (Exception ex)
            {
                // Loguear el error
                Console.WriteLine($"Error al subir archivo a S3: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extraer la clave del archivo desde la URL
                string key = fileUrl.Replace(Parametros.S3BaseUrl, "");

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = Parametros.S3BucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar archivo de S3: {ex.Message}");
                return false;
            }
        }
    }
}