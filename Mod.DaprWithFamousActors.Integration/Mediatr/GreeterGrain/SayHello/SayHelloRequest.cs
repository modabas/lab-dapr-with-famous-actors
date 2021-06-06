using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.DaprWithFamousActors.Integration.Mediatr.GreeterGrain.SayHello
{
    public class SayHelloRequest : IRequest<SayHelloResponse>
    {
        public int GrainId;
        public string Name;
    }
}
