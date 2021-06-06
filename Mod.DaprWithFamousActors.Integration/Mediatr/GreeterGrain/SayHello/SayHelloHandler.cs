using MediatR;
using Microsoft.Extensions.Logging;
using Mod.DaprWithFamousActors.IntegrationLib.Interfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mod.DaprWithFamousActors.Integration.Mediatr.GreeterGrain.SayHello
{
    public class SayHelloHandler : IRequestHandler<SayHelloRequest, SayHelloResponse>
    {
        private readonly ILogger<SayHelloHandler> _logger;
        private readonly IGrainFactory _grainFactory;

        public SayHelloHandler(ILogger<SayHelloHandler> logger, IGrainFactory grainFactory)
        {
            _logger = logger;
            _grainFactory = grainFactory;
        }
        public async Task<SayHelloResponse> Handle(SayHelloRequest request, CancellationToken cancellationToken)
        {
            using (var grainCancellationTokenSource = new GrainCancellationTokenSource())
            {
                var grainCancellationToken = grainCancellationTokenSource.Token; //create a grain token
                //link token to grain cancellation token source for this scope, so grain token will be cancelled if token is cancelled
                using (cancellationToken.Register(async () =>
                {
                    try
                    {
                        await grainCancellationTokenSource.Cancel();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SayHelloHandler Grain cancellation token failed for name: {name} / grainId: {grainId}.",
                            request.Name, request.GrainId);
                        throw;
                    }
                }
                ))
                {
                    return new SayHelloResponse() { Response = await _grainFactory.GetGrain<IGreeterGrain>(request.GrainId).SayHello(request.Name, grainCancellationToken) };
                }
            }
        }
    }
}
