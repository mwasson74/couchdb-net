using System;
using System.Diagnostics;
using CouchDB.Driver.Helpers;
using Microsoft.Extensions.Logging;

namespace CouchDB.Driver.Logging
{
    public abstract class EventDefinitionBase
    {
        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="loggingOptions">Logging options.</param>
        /// <param name="eventId">The <see cref="Microsoft.Extensions.Logging.EventId" />.</param>
        /// <param name="level">The <see cref="LogLevel" /> at which the event will be logged.</param>
        /// <param name="eventIdCode">
        ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
        /// </param>
        protected EventDefinitionBase(
            ILoggingOptions loggingOptions,
            EventId eventId,
            LogLevel level,
            string eventIdCode)
        {
            Check.NotNull(loggingOptions, nameof(loggingOptions));
            Check.NotEmpty(eventIdCode, nameof(eventIdCode));

            EventId = eventId;
            EventIdCode = eventIdCode;

            var warningsConfiguration = loggingOptions.WarningsConfiguration;

            if (warningsConfiguration != null)
            {
                var levelOverride = warningsConfiguration.GetLevel(eventId);
                if (levelOverride.HasValue)
                {
                    level = levelOverride.Value;
                }

                var behavior = warningsConfiguration.GetBehavior(eventId);
                WarningBehavior = behavior
                    ?? (level == LogLevel.Warning
                        && warningsConfiguration.DefaultBehavior == WarningBehavior.Throw
                            ? WarningBehavior.Throw
                            : WarningBehavior.Log);
            }
            else
            {
                WarningBehavior = WarningBehavior.Log;
            }

            Level = level;
        }

        /// <summary>
        ///     The <see cref="EventId" />.
        /// </summary>
        public virtual EventId EventId { [DebuggerStepThrough] get; }

        /// <summary>
        ///     The <see cref="LogLevel" /> at which the event will be logged.
        /// </summary>
        public virtual LogLevel Level { [DebuggerStepThrough] get; }

        /// <summary>
        ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" /> to suppress this event
        ///     as an error.
        /// </summary>
        public virtual string EventIdCode { get; }

        /// <summary>
        ///     Returns a warning-as-error exception wrapping the given message for this event.
        /// </summary>
        /// <param name="message">The message to wrap.</param>
        protected virtual Exception WarningAsError(string message)
            => new InvalidOperationException(
                CoreStrings.WarningAsErrorTemplate(EventId.ToString(), message, EventIdCode));

        /// <summary>
        ///     The configured <see cref="WarningBehavior" />.
        /// </summary>
        public virtual WarningBehavior WarningBehavior { get; }

        internal sealed class MessageExtractingLogger : ILogger
        {
            private string? _message;

            public string Message
            {
                get => _message ?? throw new InvalidOperationException();
                private set => _message = value;
            }

            void ILogger.Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Message = formatter(state, exception);
            }

            bool ILogger.IsEnabled(LogLevel logLevel)
                => true;

            IDisposable ILogger.BeginScope<TState>(TState state)
                => throw new NotSupportedException();
        }
    }

    public class EventData
    {
        private readonly EventDefinitionBase _eventDefinition;
        private readonly Func<EventDefinitionBase, EventData, string> _messageGenerator;

        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition">The event definition.</param>
        /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
        public EventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator)
        {
            _eventDefinition = eventDefinition;
            _messageGenerator = messageGenerator;
        }

        /// <summary>
        ///     The <see cref="EventId" /> that defines the message ID and name.
        /// </summary>
        public virtual EventId EventId
            => _eventDefinition.EventId;

        /// <summary>
        ///     The <see cref="LogLevel" /> that would be used to log message for this event.
        /// </summary>
        public virtual LogLevel LogLevel
            => _eventDefinition.Level;

        /// <summary>
        ///     A string representing the code where this event is defined.
        /// </summary>
        public virtual string EventIdCode
            => _eventDefinition.EventIdCode;

        /// <summary>
        ///     A logger message describing this event.
        /// </summary>
        /// <returns>A logger message describing this event.</returns>
        public override string ToString()
            => _messageGenerator(_eventDefinition, this);
    }
}
