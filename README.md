# lab-dapr-with-famous-actors
Dapr enabled microsoft orleans service co-hosted with grpc service in same generic host. 
Grpc service layer doesn't use orleans client to reach orleans silo, since it can call orleans silo directly since they are hosted in same generic host.

## Microsoft Orleans:
[Dapr has actor support](https://docs.dapr.io/concepts/faq/#what-is-the-relationship-between-dapr-orleans-and-service-fabric-reliable-actors) based on [Microsoft Orleans](https://dotnet.github.io/orleans/). 
But since this is a .net project we can cast most famous actors for our project by utilizing Microsoft Orleans for our dapr enabled service. Since Orleans is around for much longer, it has many more features not yet implemented by Dapr actors.

## Custom Dapr Client:
Mod.Dapr.Client includes custom dapr client implementation instead of using default one from [dapr-dotnet-sdk](https://github.com/dapr/dotnet-sdk) to be able to send messages with custom serialization (e.g. protobuf), and named clients to be able to add multiple clients with different ser/des options and be able to easily fetch desired one.
A Lot of code had to be copied from dapr-net-sdk because they are internal/private

For custom serialized pub/sub messages to be received correctly, Dapr runtime has to be 1.2 or later with raw message support which resolved this [issue](https://github.com/dapr/dapr/issues/2308).

## Debugging:
[Debugging Dapr enabled service from Visual Studio](https://github.com/dapr/dotnet-sdk/issues/401#issuecomment-747563695)

## Mediatr:
Mediatr is used for tidy coding, single point of entry for sub methods and possible extensions with mediatr pipeline. 
With Dapr pub-sub for inter-service events and Orleans streams for intra-service events, using mediatr notifications/events with more than one handler is [not recommended](https://codeopinion.com/why-use-mediatr-3-reasons-why-and-1-reason-not/).

## Distributed tracing support:
Dapr and Grpc has distributed tracing support out of the box. But as of version 3.4.2, Microsoft Orleans doesn't officially have it. Though [@Ilchert](https://github.com/Ilchert) has a [pull waiting to be merged](https://github.com/dotnet/orleans/pull/6853). Tracing support for Microsoft Orleans is copied from that source with minor changes on couple of internal/private classes/fields to public.

## More References
1. [Sample Dapr test client and Dapr enabled service listening on Grpc](https://github.com/GennadiiSvichinskyi/test-grpc-dapr)
2. [Inspiration for co-hosting Orleans and Grpc Service in same process](https://github.com/ReubenBond/hanbaobao-web)
