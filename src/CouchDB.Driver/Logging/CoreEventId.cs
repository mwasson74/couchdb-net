using Microsoft.Extensions.Logging;

namespace CouchDB.Driver.Logging
{
    public static class CoreEventId
    {
        public const int CoreBaseId = 10000;

        private enum Id
        {
            Find = CoreBaseId
        }

        public static readonly EventId Find = MakeModelId(Id.Find);

        private static readonly string QueryPrefix = DbLoggerCategory.Query.Name + ".";
        private static EventId MakeModelId(Id id)
            => new((int)id, QueryPrefix + id);
    }
}