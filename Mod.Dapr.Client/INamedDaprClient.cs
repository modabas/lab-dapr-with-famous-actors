using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Dapr.Client
{
    public interface INamedDaprClient
    {
        string GetName();
        DaprClient Instance {get;}
    }
}
