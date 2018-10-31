using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AzureStorage.Blob;
using Common;
using Lykke.Common;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Service.SmsSender
{
    internal sealed class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");

        public static async Task Main()
        {
            Console.WriteLine($"{AppEnvironment.Name} version {AppEnvironment.Version}");
#if DEBUG
            Console.WriteLine("Is DEBUG");
#else
            Console.WriteLine("Is RELEASE");
#endif           
            Console.WriteLine($"ENV_INFO: {EnvInfo}");
            
            var sertConnString = Environment.GetEnvironmentVariable("CertConnectionString");
            
            try
            {
                if (string.IsNullOrWhiteSpace(sertConnString) || sertConnString.Length < 10)
                {

                    var hostBuilder = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseUrls("http://*:5000/")
                        .UseStartup<Startup>();
#if !DEBUG
                    hostBuilder = hostBuilder.UseApplicationInsights();
#endif
                    var host = hostBuilder.Build();

                    host.Run();

                }
                else
                {
                    var sertContainer = Environment.GetEnvironmentVariable("CertContainer");
                    var sertFilename = Environment.GetEnvironmentVariable("CertFileName");
                    var sertPassword = Environment.GetEnvironmentVariable("CertPassword");

                    var certBlob = AzureBlobStorage.Create(ConstantReloadingManager.From(sertConnString));
                    var cert = certBlob.GetAsync(sertContainer, sertFilename).Result.ToBytes();

                    X509Certificate2 xcert = new X509Certificate2(cert, sertPassword);

                    var hostBuilder = new WebHostBuilder()
                        .UseKestrel(x =>
                        {
                            x.AddServerHeader = false;
                            x.Listen(IPAddress.Any, 443, listenOptions =>
                            {
                                listenOptions.UseHttps(xcert);
                                listenOptions.UseConnectionLogging();
                            });
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseUrls("https://*:443/")
                        .UseStartup<Startup>();
#if !DEBUG
                    hostBuilder = hostBuilder.UseApplicationInsights();
#endif
                    var host = hostBuilder.Build();

                    host.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

                await Task.WhenAny(
                               Task.Delay(delay),
                               Task.Run(() =>
                               {
                                   Console.ReadKey(true);
                               }));
            }

            Console.WriteLine("Terminated");
        }
    }
}
