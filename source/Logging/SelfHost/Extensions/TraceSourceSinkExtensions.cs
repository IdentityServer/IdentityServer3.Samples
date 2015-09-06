using SelfHost;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;

namespace Serilog
{
    static class TraceSourceSinkExtensions
    {
        const string DefaultOutputTemplate = "{Message}{NewLine}{Exception}";

        public static LoggerConfiguration TraceSource(
            this LoggerSinkConfiguration sinkConfiguration,
            string traceSourceName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrWhiteSpace(traceSourceName)) throw new ArgumentNullException("traceSourceName");
            if (sinkConfiguration == null) throw new ArgumentNullException("sinkConfiguration");
            if (outputTemplate == null) throw new ArgumentNullException("outputTemplate");
            
            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return sinkConfiguration.Sink(new TraceSourceSink(formatter, traceSourceName), restrictedToMinimumLevel);
        }
    }
}
