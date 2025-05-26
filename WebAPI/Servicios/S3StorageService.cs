using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace WebAPI.Servicios
{
    public class S3StorageService
    {
        private readonly IAmazonS3 _clienteS3;
        private const int _calidadCompresionJpeg = 10; // Valor de 0-100
        private const int _anchoMaximo = 1920; // Resolución máxima horizontal

        public S3StorageService()
        {
            _clienteS3 = new AmazonS3Client(
                Parametros.S3AccessKey,
                Parametros.S3SecretKey,
                RegionEndpoint.GetBySystemName(Parametros.S3Region)
            );
        }

        public async Task<string> UploadFileAsync(Stream flujoArchivo, string nombreArchivo, string tipoContenido, string carpeta)
        {
            try
            {
                // Comprimir la imagen si es un tipo de imagen compatible
                if (EsTipoContenidoImagen(tipoContenido))
                {
                    var (flujoComprimido, nuevoTipoContenido) = await ComprimirImagenAsync(flujoArchivo, tipoContenido);
                    return await SubirArchivoComprimidoAsync(flujoComprimido, nombreArchivo, nuevoTipoContenido, carpeta);
                }
                else
                {
                    // Si no es una imagen, subir el archivo sin comprimir
                    return await SubirArchivoSinCompresionAsync(flujoArchivo, nombreArchivo, tipoContenido, carpeta);
                }
            }
            catch (Exception ex)
            {
                // Loguear el error
                Console.WriteLine($"Error al subir archivo a S3: {ex.Message}");
                throw;
            }
        }

        private async Task<string> SubirArchivoSinCompresionAsync(Stream flujoArchivo, string nombreArchivo, string tipoContenido, string carpeta)
        {
            // Generar un nombre único para evitar colisiones
            string nombreArchivoUnico = $"{Guid.NewGuid()}_{nombreArchivo.Replace(" ", "_")}";
            string clave = $"{carpeta}{nombreArchivoUnico}";

            var solicitudSubida = new TransferUtilityUploadRequest
            {
                InputStream = flujoArchivo,
                Key = clave,
                BucketName = Parametros.S3BucketName,
                CannedACL = S3CannedACL.PublicRead,
                ContentType = tipoContenido
            };

            var utilidadTransferencia = new TransferUtility(_clienteS3);
            await utilidadTransferencia.UploadAsync(solicitudSubida);

            // Construir la URL del archivo subido
            return $"{Parametros.S3BaseUrl}{clave}";
        }

        private async Task<string> SubirArchivoComprimidoAsync(Stream flujoComprimido, string nombreArchivo, string tipoContenido, string carpeta)
        {
            // Reiniciar la posición del stream para leerlo desde el principio
            flujoComprimido.Position = 0;

            // Generar un nombre único para evitar colisiones
            string nombreArchivoUnico = $"{Guid.NewGuid()}_{nombreArchivo.Replace(" ", "_")}";
            string clave = $"{carpeta}{nombreArchivoUnico}";

            var solicitudSubida = new TransferUtilityUploadRequest
            {
                InputStream = flujoComprimido,
                Key = clave,
                BucketName = Parametros.S3BucketName,
                CannedACL = S3CannedACL.PublicRead,
                ContentType = tipoContenido
            };

            var utilidadTransferencia = new TransferUtility(_clienteS3);
            await utilidadTransferencia.UploadAsync(solicitudSubida);

            // Construir la URL del archivo subido
            return $"{Parametros.S3BaseUrl}{clave}";
        }

        private async Task<(Stream flujoComprimido, string tipoContenido)> ComprimirImagenAsync(Stream flujoImagen, string tipoContenido)
        {
            try
            {
                // Guardamos la posición original para resetearla después
                long posicionOriginal = flujoImagen.Position;
                flujoImagen.Position = 0;

                // Cargar la imagen con ImageSharp
                using var imagen = await Image.LoadAsync(flujoImagen);

                // Restaurar posición original del stream
                flujoImagen.Position = posicionOriginal;

                // Redimensionar la imagen si es más grande que el ancho máximo
                if (imagen.Width > _anchoMaximo)
                {
                    // Calcular nueva altura manteniendo la proporción
                    int nuevaAltura = (int)((double)imagen.Height * _anchoMaximo / imagen.Width);
                    imagen.Mutate(x => x.Resize(_anchoMaximo, nuevaAltura));
                }

                // Preparar el stream para guardar la imagen comprimida
                var flujoSalida = new MemoryStream();

                // Determinar el formato de salida y comprimir
                IImageEncoder codificador;
                string tipoContenidoSalida;

                if (tipoContenido.Contains("jpeg") || tipoContenido.Contains("jpg"))
                {
                    codificador = new JpegEncoder
                    {
                        Quality = _calidadCompresionJpeg
                    };
                    tipoContenidoSalida = "image/jpeg";
                }
                else if (tipoContenido.Contains("png"))
                {
                    // Para PNG, usamos un codificador con compresión
                    codificador = new PngEncoder
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression
                    };
                    tipoContenidoSalida = "image/png";
                }
                else
                {
                    // Para otros formatos, por defecto convertimos a JPEG
                    codificador = new JpegEncoder
                    {
                        Quality = _calidadCompresionJpeg
                    };
                    tipoContenidoSalida = "image/jpeg";
                }

                // Guardar la imagen comprimida en el stream
                await imagen.SaveAsync(flujoSalida, codificador);

                // Registrar el tamaño de compresión logrado
                Console.WriteLine($"Imagen comprimida: {flujoSalida.Length / 1024} KB (Original: {flujoImagen.Length / 1024} KB)");

                // Preparar el stream para leerlo
                flujoSalida.Position = 0;

                return (flujoSalida, tipoContenidoSalida);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al comprimir imagen: {ex.Message}");
                // En caso de error, devolver el stream original sin compresión
                flujoImagen.Position = 0;
                return (flujoImagen, tipoContenido);
            }
        }

        private bool EsTipoContenidoImagen(string tipoContenido)
        {
            return tipoContenido.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> DeleteFileAsync(string urlArchivo)
        {
            try
            {
                // Extraer la clave del archivo desde la URL
                string clave = urlArchivo.Replace(Parametros.S3BaseUrl, "");

                var solicitudEliminacion = new DeleteObjectRequest
                {
                    BucketName = Parametros.S3BucketName,
                    Key = clave
                };

                await _clienteS3.DeleteObjectAsync(solicitudEliminacion);
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