using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.DaprWithFamousActors.Implementation.Mediatr.GreeterGrain.OnSayHello
{
    public class OnSayHelloNotification :INotification
    {
        public string Name { get; set; }
        public long GrainId { get; set; }
    }
}
