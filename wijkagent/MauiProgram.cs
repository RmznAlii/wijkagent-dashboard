using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WijkAgent.Core.Data;
using WijkAgent.Core.Services;

namespace WijkAgent;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        // Blazor WebView
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Database (MySQL)
        var connectionString = "server=127.0.0.1;database=wijkagent;user=root;password=;";
        builder.Services.AddDbContext<WijkAgentDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Services
        builder.Services.AddScoped<ICrimeService, CrimeService>();

        return builder.Build();
    }
}