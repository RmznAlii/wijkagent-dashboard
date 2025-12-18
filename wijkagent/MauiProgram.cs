using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
       
        // notifier singleton
        builder.Services.AddSingleton<CrimeNotifier>();
        
        // Services
        builder.Services.AddScoped<ICrimeService, CrimeService>();


        //builder.Services.AddScoped<ISocialMediaService, MockSocialMediaService>();

        // X API service
        builder.Services.AddSingleton<SocialMediaService>(sp =>
        {
            var httpClient = new HttpClient();
            var token = config["XApi:BearerToken"];

            return new SocialMediaService(httpClient, token);
        });

        // Mock service
        builder.Services.AddSingleton<MockSocialMediaService>();

        // Control (Dit bepaalt welke service de UI gebruikt)
        builder.Services.AddSingleton<ISocialMediaService, SocialMediaControlService>();






        // HttpClient factory for API polling
        builder.Services.AddHttpClient();

        // Register the API poll service as singleton and inject notifier
        builder.Services.AddSingleton<ApiPollService>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var db = sp.GetRequiredService<WijkAgentDbContext>();
            var crimeService = sp.GetRequiredService<ICrimeService>();
            var logger = sp.GetRequiredService<ILogger<ApiPollService>>();
            var notifier = sp.GetRequiredService<CrimeNotifier>();

            // api endpoint
            var apiUrl = "https://service.p2000.page/api-HZFUUVBsdXprUBmcjlHhh1D1cdktrUjl";

            // Poll every 10 seconds
            var pollInterval = TimeSpan.FromSeconds(10);

            return new ApiPollService(httpFactory, db, crimeService, notifier, logger, apiUrl, pollInterval);
        });


        var app = builder.Build();

        // start polling
        app.Services.GetRequiredService<ApiPollService>();

        return app;
    }
}