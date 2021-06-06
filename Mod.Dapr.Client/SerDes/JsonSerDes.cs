using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mod.Dapr.Client.SerDes
{
    public class JsonSerDes : IDaprClientSerDes
    {
        private static ReadOnlySpan<byte> ToJsonReadOnlySpan<T>(T data, JsonSerializerOptions options)
        {
            return new ReadOnlySpan<byte>(JsonSerializer.SerializeToUtf8Bytes(data, options));
        }


        private static T FromJsonReadOnlySpan<T>(ReadOnlySpan<byte> bytes, JsonSerializerOptions options)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(bytes, options);
        }

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonSerDes(JsonSerializerOptions jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public virtual string PublishEventContentType => Constants.ContentTypeApplicationJson;

        public virtual T GetStateDeserialize<T>(ReadOnlySpan<byte> data)
        {
            return FromJsonReadOnlySpan<T>(data, _jsonSerializerOptions);
        }

        public virtual T InvokeBindingDeserialize<T>(ReadOnlySpan<byte> data)
        {
            return FromJsonReadOnlySpan<T>(data, _jsonSerializerOptions);
        }

        public virtual ReadOnlySpan<byte> InvokeBindingSerialize<T>(T data)
        {
            return ToJsonReadOnlySpan(data, _jsonSerializerOptions);
        }

        public virtual async Task<T> InvokeMethodDeserializeAsync<T>(HttpContent httpContent, CancellationToken cancellationToken)
        {
            return await httpContent.ReadFromJsonAsync<T>(_jsonSerializerOptions, cancellationToken);
        }

        public virtual HttpContent InvokeMethodSerialize<T>(T data)
        {
            return JsonContent.Create<T>(data, options: _jsonSerializerOptions);

        }

        public virtual ReadOnlySpan<byte> PublishEventSerialize<T>(T data)
        {
            return ToJsonReadOnlySpan(data, _jsonSerializerOptions);
        }

        public virtual ReadOnlySpan<byte> SaveStateSerialize<T>(T data)
        {
            return ToJsonReadOnlySpan(data, _jsonSerializerOptions);
        }
    }
}
