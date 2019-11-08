using System;
using System.Diagnostics;
using System.Threading;
using DotNetCore.CAP.Diagnostics;
using DotNetCore.CAP.Infrastructure;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing.Contrib.NetCore.CAP.Internal;
using OpenTracing.Contrib.NetCore.Internal;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.NetCore.CAP
{
    internal sealed class CapDiagnostics : DiagnosticListenerObserver
    {
        // https://github.com/dotnetcore/CAP/blob/develop/src/DotNetCore.CAP/Internal/CapDiagnosticListenerExtensions.cs
        public const string DiagnosticListenerName = CapDiagnosticListenerExtensions.DiagnosticListenerName;

        private readonly CapDiagnosticOptions _options;

        protected override string GetListenerName() => DiagnosticListenerName;

        public CapDiagnostics(ILoggerFactory loggerFactory, ITracer tracer,
            IOptions<CapDiagnosticOptions> options, IOptions<GenericEventOptions> genericEventOptions)
            : base(loggerFactory, tracer, genericEventOptions.Value)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override void OnNext(string eventName, object untypedArg)
        {
            switch (eventName)
            {
                case CapDiagnosticListenerExtensions.CapBeforePublish:
                    {
                        var eventData = (BrokerPublishEventData)untypedArg;
                        var operationName = _options.OperationNameResolver(eventData) + " publish";


                        var spanBuilder = Tracer.BuildSpan(operationName)
                            .WithTag(Tags.SpanKind.Key, Tags.SpanKindProducer)
                            .WithTag(Tags.Component.Key, "Cap")
                            .WithTag(Tags.MessageBusDestination.Key, eventData.BrokerTopicName)
                            .WithTag(Tags.PeerHostname.Key, eventData.BrokerAddress);
                            

                        if (eventData.Headers != null)
                        {
                            var extractedSpanContext = Tracer.Extract(BuiltinFormats.TextMap, new RequestHeadersExtractAdapter(eventData.Headers));
                            spanBuilder.AsChildOf(extractedSpanContext);
                        }


                        var scope = spanBuilder.StartActive(true);

                        _options.OnRequest?.Invoke(scope.Span, eventData);
                        if (_options.InjectEnabled?.Invoke(eventData) ?? true)
                        {
                            Tracer.Inject(scope.Span.Context, BuiltinFormats.TextMap, new BrokerHeadersInjectAdapter(eventData.Headers));
                        }

                    }
                    break;

                case CapDiagnosticListenerExtensions.CapAfterPublish:
                    {
                        DisposeActiveScope(true);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapErrorPublish:
                    {
                        var args = (BrokerPublishErrorEventData)untypedArg;
                        DisposeActiveScope(true, args.Exception);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapBeforeConsume:
                    {
                        var eventData = (BrokerConsumeEventData)untypedArg;

                        var operationName = _options.OperationNameResolver(eventData) + " receive";
                        var builder = Tracer.BuildSpan(operationName)
                            .WithTag(Tags.SpanKind.Key, Tags.SpanKindConsumer)
                            .WithTag(Tags.Component.Key, "Cap")
                            .WithTag(Tags.MessageBusDestination.Key, eventData.BrokerTopicName)
                            .WithTag(Tags.PeerHostname.Key, eventData.BrokerAddress);
                        

                        if (Helper.TryExtractTracingHeaders(eventData.BrokerTopicBody, out var headers,
                            out var removedHeadersJson))
                        {
                            eventData.Headers = headers;
                            eventData.BrokerTopicBody = removedHeadersJson;

                            var extractedSpanContext = Tracer.Extract(BuiltinFormats.TextMap, new RequestHeadersExtractAdapter(eventData.Headers));
                            builder.AsChildOf(extractedSpanContext);
                        }

                        builder.StartActive(true);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapAfterConsume:
                    {
                        var args = (BrokerConsumeEndEventData)untypedArg;
                        Tracer.Inject(Tracer.ActiveSpan.Context, BuiltinFormats.TextMap, new BrokerHeadersInjectAdapter(args.Headers));
                        CapCache.Global.AddOrUpdate("captracing", args.Headers, TimeSpan.FromMinutes(5));
                        DisposeActiveScope(true);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapErrorConsume:
                    {
                        var args = (BrokerConsumeErrorEventData)untypedArg;
                        Tracer.Inject(Tracer.ActiveSpan.Context, BuiltinFormats.TextMap, new BrokerHeadersInjectAdapter(args.Headers));
                        CapCache.Global.AddOrUpdate("captracing", args.Headers, TimeSpan.FromMinutes(5));
                        DisposeActiveScope(true, args.Exception);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapBeforeSubscriberInvoke:
                    {
                        var eventData = (SubscriberInvokeEventData)untypedArg;
                        var builder = Tracer.BuildSpan("Subscriber Invoke")
                            .WithTag(Tags.Component.Key, eventData.MethodName);
                        if (CapCache.Global.Get("captracing") is TracingHeaders headers)
                        {
                            var spanContext = Tracer.Extract(BuiltinFormats.TextMap, new RequestHeadersExtractAdapter(headers));
                            builder.AsChildOf(spanContext);
                        }
                        builder.WithTag(Tags.PeerService.Key, eventData.MethodName)
                            .WithTag(Tags.Component.Key, "Cap")
                            .StartActive(true);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapAfterSubscriberInvoke:
                    {
                        DisposeActiveScope(true);
                    }
                    break;

                case CapDiagnosticListenerExtensions.CapErrorSubscriberInvoke:
                    {
                        var args = (BrokerPublishErrorEventData)untypedArg;

                        DisposeActiveScope(true, args.Exception);
                    }
                    break;
                default:
                    {
                        ProcessUnhandledEvent(eventName, untypedArg);
                    }
                    break;
            }
        }
    }
}
