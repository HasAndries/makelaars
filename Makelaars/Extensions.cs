using System.Reflection;
using Makelaars.Application;
using Makelaars.Infrastructure.Funda;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Makelaars
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSingleton(provider => new FundaApiClientOptions
            {
                ApiKey = EnvironmentVariables.API_KEY,
                ApiUrl = EnvironmentVariables.API_URL,
                DefaultPageSize = EnvironmentVariables.PAGE_SIZE
            });
            services.AddSingleton<IFundaApiClient, FundaApiClient>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddTransient<MakelaarApplication>();

            return services;
        }
    }
}
