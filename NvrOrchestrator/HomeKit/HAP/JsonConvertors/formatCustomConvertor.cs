using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class formatCustomConvertor : JsonConverter<CharacteristicValueFormat>
    {
        public override CharacteristicValueFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() switch
            {
                "bool" => CharacteristicValueFormat.Bool,
                "uint8" => CharacteristicValueFormat.uInt8,
                "uint16" => CharacteristicValueFormat.uInt16,
                "uint32" => CharacteristicValueFormat.uInt32,
                "uint64" => CharacteristicValueFormat.uInt64,
                "int" => CharacteristicValueFormat.Int,
                "float" => CharacteristicValueFormat.Float,
                "string" => CharacteristicValueFormat.String,
                "tlv8" => CharacteristicValueFormat.TLV,
                "data" => CharacteristicValueFormat.Data,
                _ => CharacteristicValueFormat.String
            };
        }

        public override void Write(Utf8JsonWriter writer, CharacteristicValueFormat value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value switch
            {
                CharacteristicValueFormat.Bool => "bool",
                CharacteristicValueFormat.uInt8 => "uint8",
                CharacteristicValueFormat.uInt16 => "uint16",
                CharacteristicValueFormat.uInt32 => "uint32",
                CharacteristicValueFormat.uInt64 => "uint64",
                CharacteristicValueFormat.Int => "int",
                CharacteristicValueFormat.Float => "float",
                CharacteristicValueFormat.String => "string",
                CharacteristicValueFormat.TLV => "tlv8",
                CharacteristicValueFormat.Data => "data",
                _ => "string"
            });
        }
    }
}
