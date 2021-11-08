using System;

namespace CouchDB.Driver.Logging
{
    public abstract class LoggerCategory<T>
    {
        public static string Name { get; } = ToName(typeof(T));

        public override string ToString() => Name;

        public static implicit operator string(LoggerCategory<T> loggerCategory)
            => loggerCategory.ToString();

        private static string ToName(Type loggerCategoryType)
        {
            const string outerClassName = "." + nameof(DbLoggerCategory);

            var name = loggerCategoryType.FullName!.Replace('+', '.');
            var index = name.IndexOf(outerClassName, StringComparison.Ordinal);
            if (index >= 0)
            {
                name = name[..index] + name[(index + outerClassName.Length)..];
            }

            return name;
        }
    }
}