using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mod.Dapr.Client.SerDes
{
    public class ProtobufSerDes : IDaprClientSerDes
    {
        private static ReadOnlySpan<byte> Serialize<T>(T data)
        {
            if (data is null)
                return null;

            if (!(data is IMessage))
                throw new ApplicationException("data generic type should be a protobuf generated class of type IMessage");

            return new ReadOnlySpan<byte>(((IMessage)data).ToByteArray());
        }

        private static T Deserialize<T>(ByteString data)
        {
            if (!typeof(IMessage).IsAssignableFrom(typeof(T)))
            {
                throw new ApplicationException("output generic type should be a protobuf generated class of type IMessage");
            }

            var descriptor = (MessageDescriptor)typeof(T).GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static).GetValue(null, null); // get the static property Descriptor
            return (T)descriptor.Parser.ParseFrom(data); // parse the byte array to T
        }

        public string PublishEventContentType => Constants.ContentTypeApplicationProtobuf;
        //public string PublishEventContentType => Constants.ContentTypeApplicationJson;
        //public string PublishEventContentType => Constants.ContentTypeApplicationCloudEvent;


        public ReadOnlySpan<byte> PublishEventSerialize<T>(T data)
        {
            return Serialize(data);
        }

        public ReadOnlySpan<byte> InvokeBindingSerialize<T>(T data)
        {
            throw new NotImplementedException();
        }

        public T InvokeBindingDeserialize<T>(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public T GetStateDeserialize<T>(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> SaveStateSerialize<T>(T data)
        {
            throw new NotImplementedException();
        }

        public Task<T> InvokeMethodDeserializeAsync<T>(HttpContent httpContent, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public HttpContent InvokeMethodSerialize<T>(T data)
        {
            throw new NotImplementedException();
        }

    }
}
