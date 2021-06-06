using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallback;

namespace Mod.DaprWithFamousActors.Services
{
    public class DaprService : AppCallbackBase
    {
        private readonly ILogger<DaprService> _logger;
        private readonly ISender _mediatorSender;


        public DaprService(IServiceProvider serviceProvider, ILogger<DaprService> logger)
        {
            _mediatorSender = serviceProvider.GetRequiredService<ISender>();
            _logger = logger;
        }

        public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            try
            {
                switch (request.Method)
                {
                    case "SayHello":
                        var (helloRequest, contentType) = UnpackInvokeRequest<HelloRequest>(request);
                        var helloReply = new HelloReply()
                        {
                            Message = (await _mediatorSender.Send<Integration.Mediatr.GreeterGrain.SayHello.SayHelloResponse>(new Integration.Mediatr.GreeterGrain.SayHello.SayHelloRequest() { GrainId = 0, Name = helloRequest.Name }, context.CancellationToken)).Response
                        };
                        return PackInvokeResponse<HelloReply>(helloReply, contentType);
                    default:
                        throw new ApplicationException($"Unknown method invocation {request.Method}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnInvoke failed.");
                throw;
            }
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {
            var jsonString = "{\"subscriptions\": [{\"pubsubName\": \"pubsub\", \"topic\": \"SayHello\"}, {\"pubsubName\": \"pubsub\", \"topic\": \"OnSayHello\"}, {\"pubsubName\": \"pubsub\", \"topic\": \"SayHelloRaw\", \"metadata\": {\"rawPayload\": \"true\"}}]}";
            var ret = DeserializeFromJson<ListTopicSubscriptionsResponse>(jsonString);
            return Task.FromResult(ret);

            
        }

        public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            _logger.LogInformation("OnTopicEvent");
            try
            {
                switch (request.PubsubName)
                {
                    case "pubsub":
                        switch (request.Topic)
                        {
                            case "SayHelloRaw":
                                {
                                    var (input, contentType) = UnpackTopicEventProtobufRequest<HelloRequest>(request);
                                    _logger.LogInformation("{name} / {contentType}", input.Name, contentType);
                                }
                                break;

                            case "SayHello":
                                {
                                    var (input, contentType) = UnpackTopicEventRequest<HelloRequest>(request);
                                    _logger.LogInformation("{name} / {contentType}", input.Name, contentType);
                                    _ = await _mediatorSender.Send<Integration.Mediatr.GreeterGrain.SayHello.SayHelloResponse>(new Integration.Mediatr.GreeterGrain.SayHello.SayHelloRequest() { GrainId = 1, Name = input.Name }, 
                                        context.CancellationToken);
                                }
                                break;

                            case "OnSayHello":
                                {
                                    var (input, contentType) = UnpackTopicEventRequest<string>(request);
                                    _logger.LogInformation("OnSayHello event received. {stringData} / {contentType}", input, contentType);
                                }
                                break;

                            default:
                                throw new ApplicationException($"Unsupported Topic {request.Topic} in PubsubName: {request.PubsubName}");
                        }
                        break;

                    default:
                        throw new ApplicationException($"Unsupported PubsubName: {request.PubsubName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnTopicEvent failed");
                return new TopicEventResponse() { Status = TopicEventResponse.Types.TopicEventResponseStatus.Drop };
            }
            return new TopicEventResponse() { Status = TopicEventResponse.Types.TopicEventResponseStatus.Success };
        }

        private (TRequest, string) UnpackInvokeRequest<TRequest>(InvokeRequest request) where TRequest : IMessage, new()
        {
            var contentType = request.ContentType;
            if (contentType.Contains("application/grpc", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Data.TryUnpack<TRequest>(out var ret))
                {
                    return (ret, contentType);
                }
                else
                {
                    throw new ApplicationException($"Cannot unpack request as {typeof(TRequest).Name} for contentType: {contentType}");
                }
            }
            else if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return (System.Text.Json.JsonSerializer.Deserialize<TRequest>(request.Data.Value?.ToStringUtf8(), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), contentType);
            }
            throw new ApplicationException($"Unsupported contentType: {contentType}");
        }

        private InvokeResponse PackInvokeResponse<TResponse>(TResponse response, string contentType) where TResponse : IMessage
        {
            Any any;
            if (contentType.Contains("application/grpc", StringComparison.OrdinalIgnoreCase))
            {
                any = Any.Pack(response);
            }
            else if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                any = ConvertToAny(response);
            }
            else
            {
                throw new ApplicationException($"Unsupported contentType: {contentType}");
            }
            return new InvokeResponse()
            {
                ContentType = contentType,
                Data = any
            };
        }

        private (TRequest, string) UnpackTopicEventRequest<TRequest>(TopicEventRequest request)
        {
            var contentType = request.DataContentType;
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return (System.Text.Json.JsonSerializer.Deserialize<TRequest>(request.Data?.ToStringUtf8(), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), contentType);
            }
            throw new ApplicationException($"Unsupported contentType: {contentType}");
        }

        private Any ConvertToAny(IMessage input)
        {
            var formattedReply = new JsonFormatter(new JsonFormatter.Settings(false)).Format(input);
            var any = new Any
            {
                TypeUrl = input.Descriptor.FullName,
                Value = ByteString.CopyFrom(formattedReply, Encoding.UTF8)
            };
            return any;
        }

        private T DeserializeFromJson<T>(string jsonString) where T : IMessage<T>, new()
        {
            return new MessageParser<T>(() => { return new T(); }).ParseJson(jsonString);
        }

        private T ProtobufDeserialize<T>(byte[] data, bool isNull, SerializationContext context)
            where T : IMessage<T>, new()
        {
            if (isNull)
            {
                return default;
            }

            return new MessageParser<T>(() => { return new T(); }).ParseFrom(data);
        }

        private byte[] ProtobufSerialize<T>(T data, SerializationContext context)
            where T : IMessage<T>
        {
            if (data is null)
                return null;

            return data.ToByteArray();
        }

        private (TRequest, string) UnpackTopicEventProtobufRequest<TRequest>(TopicEventRequest request)
            where TRequest : IMessage<TRequest>, new()
        {
            var contentType = request.DataContentType;
            if (contentType.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                return (ProtobufDeserialize<TRequest>(request.Data?.ToByteArray(),
                    request.Data is null, null), contentType);
            }
            throw new ApplicationException($"Unsupported contentType: {contentType}");

        }

    }
}
