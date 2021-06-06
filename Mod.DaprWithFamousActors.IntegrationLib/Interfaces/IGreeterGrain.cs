using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mod.DaprWithFamousActors.IntegrationLib.Interfaces
{
    public interface IGreeterGrain : IGrainWithIntegerKey
    {
        public Task<string> SayHello(string name, GrainCancellationToken grainCancellationToken);
    }
}
