namespace CouchDB.Driver.Logging
{
    public static class DbLoggerCategory
    {
        public const string Name = "CouchDB.Driver";

        public class Database : LoggerCategory<Database>
        {
        }

        public class Query : LoggerCategory<Query>
        {
        }

        public class Update : LoggerCategory<Update>
        {
        }
    }
}
