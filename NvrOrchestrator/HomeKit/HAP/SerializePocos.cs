using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class HAPContainer
    {
        public IEnumerable<Accessories> accessories { get; set; }
    }
    public class Accessories
    {
        public int aid { get; set; }//must be persistent and unique per server, starting with 1. 
        public IEnumerable<Services> services { get; set; }//cannot be empty.

    }


    public class Services
    {
        [JsonConverter(typeof(typeCustomConvertor))]
        public Guid type { get; set; }//page 60 of HAP Spec
        public int iid { get; set; }//persistent and unique **per accessory** (can dup beyond that). "Accessory Information must be "1"
        public IEnumerable<Characteristics> characteristics { get; set; }
        public bool? hidden { get; set; }//if true, not shown to user.
        public bool? primary { get; set; }//if true, this is the primary service on the accessory
        public IEnumerable<int> linked { get; set; }//an array of instance ids of linked services.
    }

    public class Characteristics
    {
        public Characteristics()
        {

        }

        public Characteristics(Guid guid, CharacteristicValueFormat format, int iid)
        {
            this.type = guid;
            this.format = format;
            this.iid = iid;
        }

        [JsonConverter(typeof(typeCustomConvertor))]
        public Guid type { get; set; }//page 60 of HAP Spec.
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public object value { get; set; }//the actual value of said property.
        public IEnumerable<string> perms { get; set; }//an array of permission strings (page 56)

        [JsonConverter(typeof(formatCustomConvertor))]
        public CharacteristicValueFormat format { get; set; }//format of the value (page 57)        
        public int iid { get; set; }//same rules as service iid. ALSO MUST BE UNIQUE WITH SERVICES IID IN THE SAME DOMAIN
        public bool? ev { get; set; }//boolean indicating if event notifications are enabled for this characteristic
        public string description { get; set; }//string description of the characteristic on a manufacturer specific basis
        public string unit { get; set; }//Unit of the value (page 57)
        public int? minValue { get; set; }//min value of number formats
        public int? maxValue { get; set; }//max value of number formats
        public int? minStep { get; set; }//value must step from minValue in increments of minStep
        public int? maxLen { get; set; }//max length of string formats
        public int? maxDataLen { get; set; }//max characters if format is data. default is 2097152 (2MB)
        public IEnumerable<int> validValues { get; set; }//An array of numbers where each element represents a valid value
        public IEnumerable<int> validValuesRange { get; set; }//A 2 element array representing the starting value and ending value of the range of valid values.
        public int? TTL { get; set; }//max TTL in milliseconds to securely execute a write command
        public ulong? pid { get; set; }//assigned by the controller to uniquely identify the times write transaction

    }

    public class GetCharacteristicsContainer
    {
        public IEnumerable<GetCharacteristics> characteristics { get; set; }
    }
    public class GetCharacteristics
    {
        public int aid { get; set; }
        public int iid { get; set; }
        public object value { get; set; }
        public int? status { get; set; } = null;
    }

    public class PutCharacteristicsContainer
    {
        public IEnumerable<PutCharacteristics> characteristics { get; set; }
    }

    public class PutCharacteristics
    {
        public int aid { get; set; }
        public int iid { get; set; }
        public object value { get; set; }
        public bool? ev { get; set; }
    }

    public enum CharacteristicValueFormat
    {
        Bool,
        uInt8,
        uInt16,
        uInt32,
        uInt64,
        Int,
        Float,
        String,
        TLV,
        Data
    }

}
