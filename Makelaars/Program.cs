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
            PrintEnvironmentVariables();
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

        private static void PrintEnvironmentVariables()
        {
            Console.WriteLine("ENVIRONMENT VARIABLES");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"{"API_URL:", -10} {EnvironmentVariables.API_URL}");
            Console.WriteLine($"{"API_KEY:", -10} {EnvironmentVariables.API_KEY}");
            Console.WriteLine($"{"PAGE_SIZE:", -10} {EnvironmentVariables.PAGE_SIZE}");
            Console.WriteLine(new string('-', 50));
        }
    }
}
