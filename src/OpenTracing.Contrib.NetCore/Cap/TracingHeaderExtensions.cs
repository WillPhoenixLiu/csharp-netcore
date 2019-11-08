using System;
using System.Collections.Generic;
using System.Text;
using DotNetCore.CAP.Diagnostics;
using OpenTracing.Contrib.NetCore.CAP;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.NetCore.Cap
{
    public static class TracingHeaderExtensions
    {
        public static TracingHeaders GetInjectHeaders(this ITracer tracer)
        {

            if (tracer != null && tracer.ActiveSpan != null)
            {
                var headers = new TracingHeaders();
                tracer.Inject(tracer.ActiveSpan.Context, BuiltinFormats.TextMap, new BrokerHeadersInjectAdapter(headers));

                return headers;
            }

            return null;
        }
    }
}
