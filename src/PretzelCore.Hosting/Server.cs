using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PretzelCore.Hosting
{

    public class Server : IWebHost
    {
        private readonly IWebHost _host;
        public int Port { get; }
        public bool IsRunning { get; private set; }
        public string BasePath { get; }
        public bool Debug { get; }

        public IFeatureCollection ServerFeatures => _host.ServerFeatures;

        public IServiceProvider Services => _host.Services;

        public Server(string basePath, int port, bool debug)
        {
            IsRunning = false;
            BasePath = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(basePath);
            Port = port;
            Debug = debug;

            CompositeFileProvider compositeFileProvider = new CompositeFileProvider(
               new PhysicalFileProvider(BasePath));


            FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".json"] = "application/json";

            _host = new WebHostBuilder()
              .UseContentRoot(BasePath)
              .UseWebRoot(BasePath)
              .ConfigureLogging(l =>
              {
                  if (!Debug) l.ClearProviders();
              })
              .UseKestrel()
              .UseUrls($"http://localhost:{port}")
              .Configure(app =>
              {
                  IHostingEnvironment host = app.ApplicationServices.GetService<IHostingEnvironment>();
                  host.WebRootFileProvider = compositeFileProvider;

                  app.UseDefaultFiles(new DefaultFilesOptions
                  {
                      RequestPath = PathString.Empty,
                      FileProvider = compositeFileProvider,
                      DefaultFileNames = new List<string> { "index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html" }
                     
                  });
                  app.UseStaticFiles(new StaticFileOptions
                  {
                      RequestPath = PathString.Empty,
                      FileProvider = compositeFileProvider,
                      ServeUnknownFileTypes = true,
                      ContentTypeProvider = contentTypeProvider
                  });
              })
              .Build();
        }

        public void Dispose()
        {
            _host.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            _host.Start();
            IsRunning = true;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _host.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _host.StopAsync(cancellationToken);
        }
    }
}
