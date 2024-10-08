﻿using System.Text;

#if NETSTANDARD2_0
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

namespace Hightouch.Events.Serialization
{
    public static class JsonUtility
    {
        private static readonly string[] s_escapeChars = GetEscapeChars();

        public static string PrintQuoted(string content)
        {
            var builder = new StringBuilder();
            builder.Append('"');

            int lastPos = 0;
            int length = content.Length;

            for (int index = 0; index < length; index++)
            {
                int character = content[index];

                if (character >= s_escapeChars.Length)
                {
                    continue;
                }

                string esc = s_escapeChars[character];
                if (esc == null)
                {
                    continue;
                }

                builder.Append(content, lastPos, index - lastPos);
                builder.Append(esc);
                lastPos = index + 1;
            }

            builder.Append(content, lastPos, content.Length - lastPos);

            builder.Append('"');
            return builder.ToString();
        }

        private static string[] GetEscapeChars()
        {
            string[] ascii = new string[128];

            // the first 32 chars (0 - 1f) in ascii table should escape in hex form \uXXXX
            for (int character = 0; character <= 0x1f; character++)
            {
                char c1 = ToHexChar(character >> 12);
                char c2 = ToHexChar(character >> 8);
                char c3 = ToHexChar(character >> 4);
                char c4 = ToHexChar(character);

                ascii[character] = "\\u" + c1 + c2 + c3 + c4;
            }

            ascii['"'] = "\\\"";
            ascii['\\'] = "\\\\";
            ascii['\t'] = "\\t";
            ascii['\b'] = "\\b";
            ascii['\n'] = "\\n";
            ascii['\r'] = "\\r";
            ascii[0x0c] = "\\f";

            return ascii;
        }

        private static char ToHexChar(int i)
        {
            // use 1111 in binary to keep the last for digits
            int d = i & 0xf;

            // convert d to hex char (0-9, a-f)
            if (d < 10)
            {
                return (char)(d + '0');
            }
            else
            {
                return (char)(d - 10 + 'a');
            }
        }

        public static string ToJson(object value, bool pretty = false)
        {
#if NETSTANDARD2_0
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver
                {
                    Modifiers = { JsonContract.AddPublicFieldsModifier }
                },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = pretty
            };

            return JsonSerializer.Serialize(value, options);
#else
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            return JsonConvert.SerializeObject(value, pretty ? Formatting.Indented : Formatting.None, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                DateParseHandling = DateParseHandling.None
            });
#endif
        }

        public static T FromJson<T>(string json)
        {
#if NETSTANDARD2_0
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<T>(json, options);
#else
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                DateParseHandling = DateParseHandling.None
            });
#endif
        }
    }
}
