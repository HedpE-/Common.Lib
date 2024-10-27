using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace Common.Lib.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> ToDictionary(this DataRow row)
        {
            if (row == null)
                throw new ArgumentNullException();

            return row.Table.Columns
                      .Cast<DataColumn>()
                      .ToDictionary(c => c.ColumnName, c => row[c].ToString());
        }

        public static string ToJsonString(this IDictionary<string, string> dict)
        {
            return "{" + string.Join(", ", dict.Select(kvp => "\"" + kvp.Key + "\":\"" + kvp.Value + "\"")) + "}";
        }
        public static string ToJsonString<T>(this IDictionary<string, T> dict)
        {
            List<string> str = new List<string>();
            foreach (KeyValuePair<string, T> kvp in dict)
            {
                var line = "\"" + kvp.Key + "\":";

                if (kvp.Value is IDictionary<string, object> newDict)
                    line += newDict.ToJsonString();
                else
                {
                    if (kvp.Value == null)
                        line += "null";
                    else
                    {
                        if (kvp.Value.GetType() != typeof(string))
                        {
                            line += kvp.Value.ToString().Replace(',', '.');
                        }
                        else
                        {
                            line += "\"" + kvp.Value + "\"";
                        }
                    }
                }
                str.Add(line);
            }
            return "{" + string.Join(", ", str) + "}";
        }

        public static ReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if(dictionary != null)
                return new ReadOnlyDictionary<TKey, TValue>(dictionary);
            
            return null;
        }
    }
}