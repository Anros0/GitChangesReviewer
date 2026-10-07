using GitChangesReviewer.Services;
using GitChangesReviewer.ViewModels;
using GitChangesReviewer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using GitChangesReviewer.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace GitChangesReviewer
{
    public partial class App 
    {
        public static IHost AppHost { get; private set; } = null!;

        public static IServiceProvider Services => AppHost.Services;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            AppHost.StartAsync().GetAwaiter().GetResult();

            var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
            mainWindowViewModel.Folder = e.Args.FirstOrDefault();

            var mainWindow = new MainWindow(mainWindowViewModel);
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            AppHost.StopAsync().GetAwaiter().GetResult();
            AppHost.Dispose();
            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            SetupLogger();

            services.AddLogging(configure => configure.AddSerilog());


            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(GetExecutingPath(), "appsettings.json"), true, true)
#if DEBUG
                .AddUserSecrets<App>()
#endif
                .Build();

            var appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddSingleton<DialogService>();
            services.AddSingleton<GitService>();
            services.AddSingleton<AiService>();
            services.AddSingleton<MainWindowViewModel>();
            
        }

        private static void SetupLogger()
        {
            var logDirPath = Path.Combine(GetExecutingPath(), "Log");

            if (!Directory.Exists(logDirPath))
                Directory.CreateDirectory(logDirPath);

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Version",
                    FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion!)
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.File(logDirPath + "\\Info-.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).
                    WriteTo.File(logDirPath + "\\Debug-.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(logDirPath + "\\Warning-.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal).
                    WriteTo.File(logDirPath + "\\Error-.log", rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }

        private static string GetExecutingPath()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        }

    }
}
