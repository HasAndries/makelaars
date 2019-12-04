using System;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Makelaars
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Application...");
            var serviceProvider = ConfigureServices(new ServiceCollection());
            Console.WriteLine("Initialization Done. Starting Application...");
            
            using var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                serviceProvider.GetService<MakelaarApplication>()
                    .Run(cancellationTokenSource.Token)
                    .Wait(cancellationTokenSource.Token);
            }
            catch (TaskCanceledException tcex)
            {
                Console.WriteLine("A Task was cancelled: {0}", tcex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: {0}", ex);
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
            Console.WriteLine("Exiting...");
            cancellationTokenSource.Cancel();
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure();
            services.AddApplication();

            return services.BuildServiceProvider();
        }
    }
}
