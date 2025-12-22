using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Extensions;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddDataTransactionHandling()
        {
            builder.Services.AddScoped<IDataTransactionHandler, DataTransactionHandler>();

            return builder;
        }
    }
}
