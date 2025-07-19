using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ORMapperSample.Data;
using ORMapperSample.ViewModels;
using System.IO;
using System.Windows;

namespace ORMapperSample
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider = null!;
        private IConfiguration _configuration = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 設定ファイルの読み込み
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            // DIコンテナの設定
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            // データベースの初期化（マイグレーションを適用）
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate(); // EnsureCreated()の代わりにMigrate()を使用
            }

            // メインウィンドウの表示
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // DbContextの設定
            var provider = _configuration["DatabaseProvider"];
            var connectionString = _configuration.GetConnectionString(provider!);

            services.AddDbContext<AppDbContext>(options =>
            {
                switch (provider)
                {
                    case "SQLite":
                        options.UseSqlite(connectionString);
                        break;
                    case "MySQL":
                        var serverVersion = ServerVersion.AutoDetect(connectionString);
                        options.UseMySql(connectionString, serverVersion);
                        break;
                    default:
                        throw new InvalidOperationException($"サポートされていないデータベースプロバイダー: {provider}");
                }
            });

            // ViewModelの登録
            services.AddTransient<MainViewModel>();

            // Windowの登録
            services.AddTransient<MainWindow>(provider =>
            {
                var window = new MainWindow();
                window.DataContext = provider.GetRequiredService<MainViewModel>();
                return window;
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}