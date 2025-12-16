using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WijkAgent.Core.Data;
using WijkAgent.Core.Services;
using Microsoft.Extensions.Configuration;


namespace WijkAgent;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

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


        //builder.Services.AddScoped<ISocialMediaService, MockSocialMediaService>();

        builder.Services.AddSingleton<ISocialMediaService>(sp =>
        {
            var httpClient = new HttpClient();
            var token = config["XApi:BearerToken"];

            return new SocialMediaService(httpClient, token);
        });


        return builder.Build();
    }
}