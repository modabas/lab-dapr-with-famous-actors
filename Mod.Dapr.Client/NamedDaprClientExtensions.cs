using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Dapr.Client
{
    public static class NamedDaprClientExtensions
    {
        public static INamedDaprClient GetNamedDaprClient(this IEnumerable<INamedDaprClient> namedDaprClients, string clientName)
        {
            return namedDaprClients.GetNamedDaprClient(clientName, StringComparison.Ordinal);
        }

        public static INamedDaprClient GetNamedDaprClient(this IEnumerable<INamedDaprClient> namedDaprClients, string clientName, StringComparison stringComparison)
        {
            return namedDaprClients?.FirstOrDefault(c => c.GetName().Equals(clientName, stringComparison));
        }

    }
}
