using System;
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
                ApiKey =  Environment.GetEnvironmentVariable(EnvironmentVariableNames.API_KEY) ?? "ac1b0b1572524640a0ecc54de453ea9f"
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
