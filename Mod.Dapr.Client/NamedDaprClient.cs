using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Dapr.Client
{
    public class NamedDaprClient : INamedDaprClient
    {
        private readonly string _clientName;
        private readonly DaprClient _daprClient;

        public NamedDaprClient(string clientName, DaprClient daprClient)
        {
            _clientName = clientName;
            _daprClient = daprClient;
        }

        public string GetName() => _clientName;

        public DaprClient Instance { get { return _daprClient; } }
    }

}
