using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class typeCustomConvertor : JsonConverter<System.Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var input = reader.GetString();
            //is less than 36 bytes, it is an apple defined iid. 00000000-0000-1000-8000-0026BB765291. See page 60 on how they shorten the string.
            if (input.Length >= 36)
            {
                return Guid.Parse(input);
            }
            return Guid.Parse(input.PadLeft(8,'0') + "-0000-1000-8000-0026BB765291");
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            string tempString = value.ToString().ToUpper().Replace("-0000-1000-8000-0026BB765291", "");
            if (tempString.Length < 36)
            {
                tempString = tempString.TrimStart('0');
            }
            writer.WriteStringValue(tempString);
        }
    }
}
