using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mod.Dapr.Client.SerDes
{
    public interface IDaprClientSerDes
    {
        ReadOnlySpan<byte> PublishEventSerialize<T>(T data);
        ReadOnlySpan<byte> InvokeBindingSerialize<T>(T data);
        T InvokeBindingDeserialize<T>(ReadOnlySpan<byte> data);
        T GetStateDeserialize<T>(ReadOnlySpan<byte> data);
        ReadOnlySpan<byte> SaveStateSerialize<T>(T data);
        Task<T> InvokeMethodDeserializeAsync<T>(HttpContent httpContent, CancellationToken cancellationToken);
        HttpContent InvokeMethodSerialize<T>(T data);
        string PublishEventContentType { get; }
    }
}
