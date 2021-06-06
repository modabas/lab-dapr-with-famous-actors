using Dapr.Client;
using Google.Protobuf;
using Mod.Dapr.Client;
using Mod.Dapr.Client.SerDes;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mod.DaprWithFamousActors.DaprClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string AppId = args[0] ?? "testGrpcDaprService";
            string MethodName = args[1] ?? "SayHello";
            string name = args[2] ?? "Gennadii";
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            try
            {
                //using var client = new DaprClientBuilder()
                //.UseJsonSerializationOptions(options)
                //.Build();
                //var anyReply = await client.InvokeMethodAsync<HelloRequest, HelloReply>(
                //    AppId,
                //    MethodName,
                //    new HelloRequest
                //    {
                //        Name = name
                //    }
                //);

                //using var client = new DaprClientBuilder()
                //.UseJsonSerializationOptions(options)
                //.Build();
                //var anyReply = await client.InvokeMethodGrpcAsync<HelloRequest, HelloReply>(
                //    AppId,
                //    MethodName,
                //    new HelloRequest
                //    {
                //        Name = name
                //    }
                //);

                //using var client = new CustomDaprClientBuilder()
                //    .UseSerializationOptions(new ProtobufSerDes())
                //    .Build();
                //var anyReply = await client.InvokeMethodGrpcAsync<HelloRequest, HelloReply>(
                //    AppId,
                //    MethodName,
                //    new HelloRequest
                //    {
                //        Name = name
                //    }
                //);

                //Console.WriteLine($"Reply : {anyReply.Message}");

                //using var client = new DaprClientBuilder()
                //    .UseJsonSerializationOptions(options)
                //    .Build();
                //await client.PublishEventAsync<HelloRequest>(
                //    "pubsub",
                //    "SayHello",
                //    new HelloRequest
                //    {
                //        Name = name
                //    }
                //);

                using var client = new CustomDaprClientBuilder()
                    .UseSerializationOptions(new ProtobufSerDes())
                    .Build();
                var metadata = new System.Collections.Generic.Dictionary<string, string>();
                metadata.Add("rawPayload", "true");
                await client.PublishEventAsync<HelloRequest>(
                    "pubsub",
                    "SayHelloRaw",
                    new HelloRequest
                    {
                        Name = name
                    },
                    metadata
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
