using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Hosting;
using Microsoft.AspNetCore.Http;
using Mod.DaprWithFamousActors.Services;
using MediatR;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;
using Mod.Dapr.Client.SerDes;

namespace Mod.DaprWithFamousActors
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder
                    .AddActivityPropagation()
                    .UseLocalhostClustering()
                    .AddMemoryGrainStorage("StateStorage")
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Implementation.Grains.GreeterGrain).Assembly).WithReferences())
                    .ConfigureServices(s =>
                    {
                        s.AddMediatR(typeof(Implementation.Mediatr.GreeterGrain.OnSayHello.OnSayHelloHandler).Assembly);
                        //s.AddDaprClient(conf =>
                        //{
                        //    conf.UseJsonSerializationOptions(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
                        //});
                        s.AddNamedDaprClient("first", conf =>
                        {
                            //conf.UseJsonSerializationOptions(new System.Text.Json.JsonSerializerOptions(){ IncludeFields = true, PropertyNameCaseInsensitive = true });
                            conf.UseSerializationOptions(new JsonSerDes(new System.Text.Json.JsonSerializerOptions() { IncludeFields = true, PropertyNameCaseInsensitive = true }));
                        });
                        s.AddNamedDaprClient("default", conf =>
                        {
                            //conf.UseJsonSerializationOptions(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
                            conf.UseSerializationOptions(new JsonSerDes(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)));
                        });
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddGrpc();
                        //Add MediatR
                        services.AddMediatR(typeof(Integration.Mediatr.GreeterGrain.SayHello.SayHelloHandler).Assembly);
                    });
                    webBuilder.Configure((ctx, app) =>
                    {
                        if (ctx.HostingEnvironment.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGrpcService<DaprService>();

                            endpoints.MapGet("/", async context =>
                            {
                                await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                            });
                        });
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddOpenTelemetryTracing(
                        (builder) => builder
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(typeof(Program).Assembly.GetName().Name))
                            .AddAspNetCoreInstrumentation()
                            .AddSource(Implementation.GrainFilters.ActivityPropagationGrainCallFilter.ActivitySourceName)
                            .AddZipkinExporter()
                            );
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });
        }

    }

    public static class SiloBuilderExtesions
    {
        /// <summary>
        /// Add <see cref="Activity.Current"/> propagation through grain calls.
        /// Note: according to <see cref="ActivitySource.StartActivity(string, ActivityKind)"/> activity will be created only when any listener for activity exists <see cref="ActivitySource.HasListeners()"/> and <see cref="ActivityListener.Sample"/> returns <see cref="ActivitySamplingResult.PropagationData"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static ISiloBuilder AddActivityPropagation(this ISiloBuilder builder)
        {
            if (Activity.DefaultIdFormat != ActivityIdFormat.W3C)
                throw new InvalidOperationException("Activity propagation available only for Activities in W3C format. Set Activity.DefaultIdFormat into ActivityIdFormat.W3C.");

            return builder
            .AddOutgoingGrainCallFilter<Implementation.GrainFilters.ActivityPropagationOutgoingGrainCallFilter>()
            .AddIncomingGrainCallFilter<Implementation.GrainFilters.ActivityPropagationIncomingGrainCallFilter>();
        }

        /// <summary>
        /// Add <see cref="Activity.Current"/> propagation through grain calls.
        /// Note: according to <see cref="ActivitySource.StartActivity(string, ActivityKind)"/> activity will be created only when any listener for activity exists <see cref="ActivitySource.HasListeners()"/> and <see cref="ActivityListener.Sample"/> returns <see cref="ActivitySamplingResult.PropagationData"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IClientBuilder AddActivityPropagation(this IClientBuilder builder)
        {
            if (Activity.DefaultIdFormat != ActivityIdFormat.W3C)
                throw new InvalidOperationException("Activity propagation available only for Activities in W3C format. Set Activity.DefaultIdFormat into ActivityIdFormat.W3C.");

            return builder
                .AddOutgoingGrainCallFilter<Implementation.GrainFilters.ActivityPropagationOutgoingGrainCallFilter>();
        }
    }
}
