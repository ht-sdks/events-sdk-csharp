using System.Collections;
using System.Collections.Generic;

namespace Hightouch.Events.Serialization
{
    /// <summary>
    /// Deep-merges dictionary overlays onto <see cref="JsonObject"/> values.
    /// Nested maps are merged; other values overwrite.
    /// </summary>
    internal static class JsonMerge
    {
        internal static void DeepMerge(JsonObject target, IDictionary<string, object> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var entry in source)
            {
                if (entry.Key == null)
                {
                    continue;
                }

                MergeValue(target, entry.Key, ToJsonElement(entry.Value));
            }
        }

        internal static void DeepMerge(JsonObject target, JsonObject source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var entry in source)
            {
                MergeValue(target, entry.Key, entry.Value);
            }
        }

        private static void MergeValue(JsonObject target, string key, JsonElement incoming)
        {
            JsonElement existing;
            if (target.TryGetValue(key, out existing) && existing is JsonObject existingObj && incoming is JsonObject incomingObj)
            {
                DeepMerge(existingObj, incomingObj);
                return;
            }

            target[key] = incoming;
        }

        internal static JsonElement ToJsonElement(object value)
        {
            if (value == null)
            {
                return JsonNull.Instance;
            }

            if (value is JsonElement element)
            {
                return element;
            }

            if (value is string s)
            {
                return s;
            }

            if (value is bool b)
            {
                return b;
            }

            if (value is byte || value is sbyte || value is short || value is ushort || value is int)
            {
                return System.Convert.ToInt32(value);
            }

            if (value is uint || value is long)
            {
                return System.Convert.ToInt64(value);
            }

            if (value is ulong || value is decimal || value is double || value is float)
            {
                return System.Convert.ToDouble(value);
            }

            var genericMap = AsStringObjectMap(value);
            if (genericMap != null)
            {
                return ToJsonObject(genericMap);
            }

            if (value is IDictionary map)
            {
                var nested = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in map)
                {
                    if (entry.Key == null || entry.Value == null)
                    {
                        continue;
                    }

                    nested[entry.Key.ToString()] = entry.Value;
                }

                return ToJsonObject(nested);
            }

            if (value is IEnumerable enumerable)
            {
                var arr = new JsonArray();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        arr.Add(ToJsonElement(item));
                    }
                }

                return arr;
            }

            return JsonUtility.FromJson<JsonObject>(JsonUtility.ToJson(value));
        }

        internal static JsonObject ToJsonObject(IDictionary<string, object> data)
        {
            var obj = new JsonObject();
            if (data == null)
            {
                return obj;
            }

            foreach (var entry in data)
            {
                if (entry.Key == null)
                {
                    continue;
                }

                obj[entry.Key] = ToJsonElement(entry.Value);
            }

            return obj;
        }

        private static IDictionary<string, object> AsStringObjectMap(object value)
        {
            if (value is IDictionary<string, object> dict)
            {
                return dict;
            }

            return null;
        }
    }
}
