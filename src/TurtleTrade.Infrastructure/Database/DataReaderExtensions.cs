using System;
using System.Data;

namespace TurtleTrade.Database
{
    internal static class DataReaderExtensions
    {
        public static bool TryGetValue<T>(this IDataReader reader, string ColumnName, out T value)
        {
            value = default(T);
            object objValue = reader.GetValue(reader.GetOrdinal(ColumnName));

            if (objValue == DBNull.Value)
            {
                return false;
            }

            value = (T)objValue;

            return true;
        }

        public static T? GetValue<T>(this IDataReader reader, string ColumnName) where T : struct
        {
            object objValue = reader.GetValue(reader.GetOrdinal(ColumnName));

            return objValue == DBNull.Value ? null : (T?)objValue;
        }

        public static string GetStringValue(this IDataReader reader, string ColumnName)
        {
            object objValue = reader.GetValue(reader.GetOrdinal(ColumnName));

            return objValue == DBNull.Value ? string.Empty : objValue.ToString();
        }

        public static Object GetValueOrDBNull(this decimal? input)
        {
            return input.HasValue ? (object)input.Value : DBNull.Value;
        }

        public static Object GetValueOrDBNull(this string input)
        {
            return string.IsNullOrEmpty(input) ? (Object)DBNull.Value : input;
        }

        public static Object GetValueOrDBNull(this Boolean? input)
        {
            return input.HasValue ? (object)input.Value : DBNull.Value;
        }
    }
}
