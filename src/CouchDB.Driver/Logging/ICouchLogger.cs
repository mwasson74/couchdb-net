using Microsoft.Extensions.Logging;

namespace CouchDB.Driver.Logging
{
    public interface ICouchLogger
    {
        void Log(EventData eventData);
        bool ShouldLog(EventId eventId, LogLevel logLevel);
    }
}