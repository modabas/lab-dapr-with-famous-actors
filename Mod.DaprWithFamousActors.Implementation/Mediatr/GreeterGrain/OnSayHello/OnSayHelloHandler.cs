using Dapr.Client;
using MediatR;
using Microsoft.Extensions.Logging;
using Mod.Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mod.DaprWithFamousActors.Implementation.Mediatr.GreeterGrain.OnSayHello
{
    public class OnSayHelloHandler : INotificationHandler<OnSayHelloNotification>
    {
        private readonly ILogger<OnSayHelloHandler> _logger;
        private readonly INamedDaprClient _namedDaprClient;
        private const string clientName = "default";
        public OnSayHelloHandler(ILogger<OnSayHelloHandler> logger, IEnumerable<INamedDaprClient> namedDaprClients)
        {
            _logger = logger;
            _namedDaprClient = namedDaprClients.GetNamedDaprClient(clientName);
        }

        public async Task Handle(OnSayHelloNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Said hello to {name} on grain {grainId}", notification.Name, notification.GrainId);
                if (_namedDaprClient == null)
                    throw new ApplicationException($"Cannot find dapr client with name {clientName}");

                var eventMessage = $"Said hello to {notification.Name} on grain {notification.GrainId}";
                await _namedDaprClient.Instance.PublishEventAsync<string>(
                    "pubsub",
                    "OnSayHello",
                    eventMessage,
                    cancellationToken
                );
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnSayHelloHandler.Handle failed.");
            }
        }
    }
}
