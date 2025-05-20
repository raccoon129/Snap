using Microsoft.Extensions.Logging;
using Snap.Paginas;
using Snap.Servicios;

namespace Snap
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registro de servicios
            builder.Services.AddSingleton<ApiService>();

            // Registro de páginas
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<InicioPage>();
            // Registrar el resto de páginas cuando se implementen

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
