using Mod.Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DaprClientBuilderExtensions
    {
        /// <summary>
        /// Adds Dapr client services to the provided <see cref="IServiceCollection" />. This does not include integration
        /// with ASP.NET Core MVC. Use the <c>AddDapr()</c> extension method on <c>IMvcBuilder</c> to register MVC integration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" />.</param>
        /// <param name="configure"></param>
        public static void AddNamedDaprClient(this IServiceCollection services, string clientName, Action<CustomDaprClientBuilder> configure = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (clientName is null)
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            services.AddSingleton<INamedDaprClient>(_ =>
            {
                var builder = new CustomDaprClientBuilder();
                if (configure != null)
                {
                    configure.Invoke(builder);
                }

                return new NamedDaprClient(clientName, builder.Build());
            });
        }
    }
}
