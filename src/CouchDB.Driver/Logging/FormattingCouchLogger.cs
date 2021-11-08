using System;
using System.Text;
using CouchDB.Driver.Helpers;
using Microsoft.Extensions.Logging;

namespace CouchDB.Driver.Logging
{
    internal class FormattingCouchLogger : ICouchLogger
    {
        private readonly Action<string> _sink;
        private readonly Func<EventId, LogLevel, bool> _filter;
        private readonly DbContextLoggerOptions _options;

        public FormattingCouchLogger(
            Action<string> sink,
            Func<EventId, LogLevel, bool> filter,
            DbContextLoggerOptions options)
        {
            _sink = sink;
            _filter = filter;
            _options = options;
        }

        public virtual void Log(EventData eventData)
        {
            Check.NotNull(eventData, nameof(eventData));

            var message = eventData.ToString();
            var logLevel = eventData.LogLevel;
            var eventId = eventData.EventId;

            if (_options != DbContextLoggerOptions.None)
            {
                var messageBuilder = new StringBuilder();

                if ((_options & DbContextLoggerOptions.Level) != 0)
                {
                    messageBuilder.Append(GetLogLevelString(logLevel));
                }

                if ((_options & DbContextLoggerOptions.LocalTime) != 0)
                {
                    messageBuilder.Append(DateTime.Now.ToShortDateString()).Append(DateTime.Now.ToString(" HH:mm:ss.fff "));
                }

                if ((_options & DbContextLoggerOptions.UtcTime) != 0)
                {
                    messageBuilder.Append(DateTime.UtcNow.ToString("o")).Append(' ');
                }

                if ((_options & DbContextLoggerOptions.Id) != 0)
                {
                    messageBuilder.Append(eventData.EventIdCode).Append('[').Append(eventId.Id).Append("] ");
                }

                if ((_options & DbContextLoggerOptions.Category) != 0)
                {
                    var lastDot = eventId.Name!.LastIndexOf('.');
                    if (lastDot > 0)
                    {
                        messageBuilder.Append('(').Append(eventId.Name.Substring(0, lastDot)).Append(") ");
                    }
                }

                const string padding = "      ";
                var preambleLength = messageBuilder.Length;

                if (_options == DbContextLoggerOptions.SingleLine) // Single line ONLY
                {
                    message = messageBuilder
                        .Append(message)
                        .Replace(Environment.NewLine, "")
                        .ToString();
                }
                else
                {
                    message = (_options & DbContextLoggerOptions.SingleLine) != 0
                        ? messageBuilder
                            .Append("-> ")
                            .Append(message)
                            .Replace(Environment.NewLine, "", preambleLength, messageBuilder.Length - preambleLength)
                            .ToString()
                        : messageBuilder
                            .AppendLine()
                            .Append(message)
                            .Replace(
                                Environment.NewLine, Environment.NewLine + padding, preambleLength, messageBuilder.Length - preambleLength)
                            .ToString();
                }
            }

            _sink(message);
        }

        /// <inheritdoc />
        public virtual bool ShouldLog(EventId eventId, LogLevel logLevel)
            => _filter(eventId, logLevel);

        private static string GetLogLevelString(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Trace => "trce: ",
                LogLevel.Debug => "dbug: ",
                LogLevel.Information => "info: ",
                LogLevel.Warning => "warn: ",
                LogLevel.Error => "fail: ",
                LogLevel.Critical => "crit: ",
                _ => "none",
            };
    }
}
