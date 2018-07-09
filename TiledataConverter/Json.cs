using Newtonsoft.Json;
using System;
using System.IO;

namespace TiledataConverter
{
    public class Json
    {
        public static void SerializeToFile(string filename, object value, string comment = null)
        {
            using (var writer = new StreamWriter(filename))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.Indentation = 1;
                jsonWriter.IndentChar = '\t';

                if (comment != null)
                {
                    jsonWriter.WriteComment(comment);
                    jsonWriter.WriteWhitespace(Environment.NewLine);
                }
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                serializer.Serialize(jsonWriter, value);
            }
        }
    }
}
