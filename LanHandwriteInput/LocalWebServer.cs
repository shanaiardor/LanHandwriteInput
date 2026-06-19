using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace LanHandwriteInput
{
    internal sealed class LocalWebServer : IDisposable
    {
        private readonly int _port;
        private readonly Action<string> _onTextReceived;
        private WebApplication? _app;

        public LocalWebServer(int port, Action<string> onTextReceived)
        {
            _port = port;
            _onTextReceived = onTextReceived;
        }

        public void Start()
        {
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.UseUrls($"http://0.0.0.0:{_port}");
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            });

            _app = builder.Build();
            MapRoutes(_app);
            _app.StartAsync().GetAwaiter().GetResult();
        }

        private void MapRoutes(WebApplication app)
        {
            var distPath = Path.Combine(AppContext.BaseDirectory, "dist");
            Directory.CreateDirectory(distPath);

            var fileProvider = new PhysicalFileProvider(distPath);
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                DefaultFileNames = new List<string> { "index.html" }
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider
            });

            app.MapPost("/send", (SendTextRequest request) =>
            {
                var text = request.Text ?? string.Empty;
                if (text.Length > 0)
                {
                    _onTextReceived(text);
                }

                return Results.Json(new { ok = true });
            });

            app.MapFallback(async context =>
            {
                var indexPath = Path.Combine(distPath, "index.html");
                if (File.Exists(indexPath))
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.SendFileAsync(indexPath);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("dist/index.html 不存在。请先构建 Vue 页面，或把前端文件放到 dist 文件夹。");
            });
        }

        public void Dispose()
        {
            if (_app is null)
            {
                return;
            }

            _app.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
            _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _app = null;
        }

        private sealed record SendTextRequest(string? Text);
    }
}
