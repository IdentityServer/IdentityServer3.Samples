using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.Diagnostics;
using System.IO;

namespace SelfHost
{
    class TraceSourceSink : ILogEventSink
    {
        private MessageTemplateTextFormatter _formatter;
        private TraceSource _source;

        public TraceSourceSink(MessageTemplateTextFormatter formatter, string traceSourceName)
        {
            _formatter = formatter;
            _source = new TraceSource(traceSourceName);
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");
            
            var sr = new StringWriter();
            _formatter.Format(logEvent, sr);
            var text = sr.ToString().Trim();

            if (logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal)
                _source.TraceEvent(TraceEventType.Error, 0, text);
            else if (logEvent.Level == LogEventLevel.Warning)
                _source.TraceEvent(TraceEventType.Warning, 0, text);
            else if (logEvent.Level == LogEventLevel.Information)
                _source.TraceEvent(TraceEventType.Information, 0, text);
            else
                _source.TraceEvent(TraceEventType.Verbose, 0, text);
        }
    }
}
