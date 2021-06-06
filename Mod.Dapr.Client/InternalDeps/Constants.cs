using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Dapr.Client
{
    internal class Constants
    {
        public const string ContentTypeApplicationProtobuf = "application/x-protobuf";
        public const string ContentTypeApplicationGrpc = "application/grpc";
        public const string ContentTypeApplicationJson = "application/json";
        public const string ContentTypeApplicationCloudEvent = "application/cloudevents+json";
    }

}
