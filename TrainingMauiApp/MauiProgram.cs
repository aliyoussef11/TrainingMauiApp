using TrainingMauiApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace TrainingMauiApp
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

            builder.Services.AddSingleton<AuthService>();
            MicrosoftAuthService.Init(
                clientId: "1950a258-227b-4e31-a9cf-717495945fc2",
                tenantId: "common"
            );

            #if DEBUG
            builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
