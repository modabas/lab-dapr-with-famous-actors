using MediatR;
using Microsoft.Extensions.Logging;
using Mod.DaprWithFamousActors.Implementation.Mediatr.GreeterGrain.OnSayHello;
using Mod.DaprWithFamousActors.IntegrationLib.Interfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mod.DaprWithFamousActors.Implementation.Grains
{
    public class GreeterGrain : Grain, IGreeterGrain
    {
        private readonly IPublisher _mediatorPublisher;
        private readonly ILogger<GreeterGrain> _logger;

        public GreeterGrain(IPublisher mediatorPublisher, ILogger<GreeterGrain> logger)
        {
            _mediatorPublisher = mediatorPublisher;
            _logger = logger;
        }

        public Task<string> SayHello(string name, GrainCancellationToken grainCancellationToken)
        {
            var grainId = this.GetPrimaryKeyLong();
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token; //create a timeout token
                //link grain token to cancellation token source for this scope, so token will be cancelled if grain token is cancelled
                using (grainCancellationToken.CancellationToken.Register(() => cancellationTokenSource.Cancel()))
                {
                    _mediatorPublisher.Publish<OnSayHelloNotification>(new OnSayHelloNotification() { Name = name, GrainId = grainId }, cancellationToken).Ignore();
                }
            }
            return Task.FromResult($"Hello {name}");
        }
    }
}
