using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Zero.Extensions.Logging.Console
{
    internal class ZeroJsonConsoleFormatter : ConsoleFormatter, IDisposable
    {
        private IDisposable _optionsReloadToken;

        public ZeroJsonConsoleFormatter(IOptionsMonitor<JsonConsoleFormatterOptions> options)
            : base(ConsoleFormatterNames.Json)
        {
            ReloadLoggerOptions(options.CurrentValue);
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }
            LogLevel logLevel = logEntry.LogLevel;
            string category = logEntry.Category;
            int eventId = logEntry.EventId.Id;
            Exception exception = logEntry.Exception;
            const int DefaultBufferSize = 1024;
            using (var output = new PooledByteBufferWriter(DefaultBufferSize))
            {
                using (var writer = new Utf8JsonWriter(output, FormatterOptions.JsonWriterOptions))
                {
                    writer.WriteStartObject();
                    var timestampFormat = FormatterOptions.TimestampFormat;
                    if (timestampFormat != null)
                    {
                        DateTimeOffset dateTimeOffset = FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
                        writer.WriteString("Timestamp", dateTimeOffset.ToString(timestampFormat));
                    }
                    writer.WriteNumber(nameof(logEntry.EventId), eventId);
                    writer.WriteString(nameof(logEntry.LogLevel), GetLogLevelString(logLevel));
                    writer.WriteString(nameof(logEntry.Category), category);
                    writer.WriteString("Message", message);

                    if (exception != null)
                    {
                        string exceptionMessage = exception.ToString();
                        if (!FormatterOptions.JsonWriterOptions.Indented)
                        {
                            exceptionMessage = exceptionMessage.Replace(Environment.NewLine, " ");
                        }
                        writer.WriteString(nameof(Exception), exceptionMessage);
                    }

                    if (logEntry.State != null)
                    {
                        // writer.WriteString("Message", logEntry.State.ToString());
                        if (logEntry.State is IReadOnlyCollection<KeyValuePair<string, object>> stateProperties)
                        {
                            if (stateProperties.Count > 1)
                            {
                                writer.WriteStartObject(nameof(logEntry.State));
                                foreach (KeyValuePair<string, object> item in stateProperties)
                                {
                                    WriteItem(writer, item);
                                }
                                writer.WriteEndObject();
                            }
                        }
                    }
                    WriteScopeInformation(writer, scopeProvider);
                    writer.WriteEndObject();
                    writer.Flush();
                }
                textWriter.Write(Encoding.UTF8.GetString(output.WrittenMemory.Span));
            }
            textWriter.Write(Environment.NewLine);
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }

        private void WriteScopeInformation(Utf8JsonWriter writer, IExternalScopeProvider scopeProvider)
        {
            if (FormatterOptions.IncludeScopes && scopeProvider != null)
            {
                var stringScopes = new List<string>();
                var customScopes = new Dictionary<string, object>();

                writer.WriteStartObject("Scope");
                scopeProvider.ForEachScope((scope, state) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object>> scopeItems)
                    {

                        var scopeName = scope.GetType().Name;
                        if (scopeName.EndsWith("LogScope"))
                        {
                            scopeName = scopeName[0..^8];
                            writer.WriteStartObject(scopeName);
                            foreach (KeyValuePair<string, object> item in scopeItems)
                            {
                                WriteItem(state, item);
                            }
                            writer.WriteEndObject();
                        }
                        else
                        {
                            foreach (KeyValuePair<string, object> item in scopeItems)
                            {
                                customScopes.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else if (scope is System.Collections.IDictionary dicScope)
                    {
                        foreach (var k in dicScope.Keys)
                        {
                            customScopes.Add(ToInvariantString(k), dicScope[k]);
                        }
                    }
                    else
                    {
                        stringScopes.Add(ToInvariantString(scope));
                    }
                }, writer);

                if (customScopes.Any())
                {
                    writer.WriteStartObject("Custom");
                    foreach (KeyValuePair<string, object> item in customScopes)
                    {
                        WriteItem(writer, item);
                    }
                    writer.WriteEndObject();
                }
                if (stringScopes.Any())
                {
                    writer.WriteStartArray("Plain");
                    stringScopes.ForEach(s => writer.WriteStringValue(s));
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }
        }

        private void WriteItem(Utf8JsonWriter writer, KeyValuePair<string, object> item)
        {
            var key = item.Key;
            switch (item.Value)
            {
                case bool boolValue:
                    writer.WriteBoolean(key, boolValue);
                    break;
                case byte byteValue:
                    writer.WriteNumber(key, byteValue);
                    break;
                case sbyte sbyteValue:
                    writer.WriteNumber(key, sbyteValue);
                    break;
                case char charValue:
                    writer.WriteString(key, MemoryMarshal.CreateSpan(ref charValue, 1));
                    break;
                case decimal decimalValue:
                    writer.WriteNumber(key, decimalValue);
                    break;
                case double doubleValue:
                    writer.WriteNumber(key, doubleValue);
                    break;
                case float floatValue:
                    writer.WriteNumber(key, floatValue);
                    break;
                case int intValue:
                    writer.WriteNumber(key, intValue);
                    break;
                case uint uintValue:
                    writer.WriteNumber(key, uintValue);
                    break;
                case long longValue:
                    writer.WriteNumber(key, longValue);
                    break;
                case ulong ulongValue:
                    writer.WriteNumber(key, ulongValue);
                    break;
                case short shortValue:
                    writer.WriteNumber(key, shortValue);
                    break;
                case ushort ushortValue:
                    writer.WriteNumber(key, ushortValue);
                    break;
                case null:
                    writer.WriteNull(key);
                    break;
                default:
                    writer.WriteString(key, ToInvariantString(item.Value));
                    break;
            }
        }

        private static string ToInvariantString(object obj) => Convert.ToString(obj, CultureInfo.InvariantCulture);

        internal JsonConsoleFormatterOptions FormatterOptions { get; set; }

        private void ReloadLoggerOptions(JsonConsoleFormatterOptions options)
        {
            FormatterOptions = options;
        }

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }
    }
}
