using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public static class HAPManager
    {
        public static GetCharacteristicsContainer SerializeFromParameterList(string ids, Camera.Poco c)
        {
            var charList = new List<GetCharacteristics>();

            var fullList = SerializeFromPoco(c);

            foreach (var id in ids.Replace("id=","").Split(','))
            {
                try
                {
                    var aid = Convert.ToInt32(id.Split('.')[0]);
                    var iid = Convert.ToInt32(id.Split('.')[1]);
                    var value = fullList.accessories.Where(f => f.aid == aid).Single().services.SelectMany(s => s.characteristics).Where(cc => cc.iid == iid).Single().value;
                    charList.Add(new() { value = value, aid = aid, iid = iid });

                }
                catch (Exception)
                {
                    charList.Add(new() { status = -70402, aid = Convert.ToInt32(id.Split('.')[0]), iid = Convert.ToInt32(id.Split('.')[1]) });
                }
            }
            return new GetCharacteristicsContainer() { characteristics = charList };
        }
        public static HAPContainer SerializeFromPoco(Camera.Poco c)
        {
            int increment = 1;

            Dictionary<string, Services> services = new();
            var properties = c.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var format = prop.PropertyType;
                if (prop.GetCustomAttributes(typeof(ServiceAttribute), true).FirstOrDefault() is ServiceAttribute service && 
                    prop.GetCustomAttributes(typeof(CharacteristicAttribute), true).FirstOrDefault() is CharacteristicAttribute characteristic && 
                    format!=null)
                {
                    var result = services.TryGetValue(service.UUID, out Services s);
                    if (result==false)
                    {
                        s = new()
                        {
                            type = new Guid(service.UUID),
                            characteristics = new List<Characteristics>(),
                            iid = increment,
                        };
                        increment++;
                        services.Add(service.UUID, s);                        
                    }
                    object v = null;
                    if ((characteristic.Permissions & CharacteristicPermissions.PairedRead)== CharacteristicPermissions.PairedRead)
                    {
                        v = prop.GetGetMethod().Invoke(c, null);
                        if (TypeToCharacteristicValueFormat(format)==CharacteristicValueFormat.TLV)
                        {
                            v = Convert.ToBase64String(GenerateTLV(prop, v));
                        }
                    }
                    (s.characteristics as List<Characteristics>).Add(new()
                    {
                        type = new Guid(characteristic.UUID),
                        iid = increment,
                        format = TypeToCharacteristicValueFormat(format),
                        perms = PermissionsToStringArray(characteristic.Permissions),
                        value = v
                    });
                    increment++;
                }
            }
            HAPContainer hap = new()
            {
                accessories = new Accessories[]
                {
                    new Accessories()
                    {
                        aid = 1,//as I never intend to be a bridge, I will always have one accessory. If this does not stay true, then this will need to be modified.
                        services = services.Values.ToArray()                        
                    }
                }
            };
            return hap;
        }

        private static byte[] GenerateTLV(System.Reflection.PropertyInfo prop, object v, System.Reflection.PropertyInfo[] preObtainedProperties = null)
        {
            //TODO: Need to understand IEnumerable Properties so that we can properly generate the TLVs for them.
            var subProperties = prop.PropertyType.GetProperties();
            if (prop.PropertyType!=typeof(byte[]) && prop.PropertyType.IsGenericType==true)//THERE HAS GOT TO BE A BETTER WAY
            {
                subProperties = preObtainedProperties;
            }
            List<TLV.TLV> tlvOutput = new();
            foreach (var SubProp in subProperties)
            {
                if (SubProp.GetCustomAttributes(typeof(TLVCharacteristicAttribute), true).Where(t => (t as TLVCharacteristicAttribute).IsReadable).FirstOrDefault() is TLVCharacteristicAttribute char_tlv)
                {
                    var tlvData = SubProp.GetGetMethod().Invoke(v, null);
                    var clrType = tlvData.GetType();
                    if (clrType != typeof(byte[]) && clrType.IsArray==true)
                    {
                        bool first = true;
                        foreach (var item in (Array)tlvData)
                        {
                            var property = item.GetType().GetProperties();
                            var data = (TypeToCharacteristicValueFormat(item.GetType()), char_tlv.Length) switch
                            {
                                (CharacteristicValueFormat.Int, TLVCharacteristicLength.One) => new byte[] { (byte)((int)item) },
                                (CharacteristicValueFormat.Int, TLVCharacteristicLength.Two) => BitConverter.GetBytes((short)((int)item)),
                                (CharacteristicValueFormat.Int, TLVCharacteristicLength.Four) => BitConverter.GetBytes((int)item),
                                (CharacteristicValueFormat.uInt64, TLVCharacteristicLength.Eight) => BitConverter.GetBytes((ulong)item),
                                (_, TLVCharacteristicLength.GUID) => ((Guid)item).ToByteArray(),
                                (CharacteristicValueFormat.Data, TLVCharacteristicLength.N) => item as byte[],
                                (CharacteristicValueFormat.String, TLVCharacteristicLength.N) => Encoding.UTF8.GetBytes(item as string),
                                (_, TLVCharacteristicLength.N) => GenerateTLV(SubProp, item, property),
                                _ => null
                            };
                            if (!first)
                            {
                                tlvOutput.Add(new(0, new byte[] { }));
                            }
                            tlvOutput.Add(new((TLV.TLV_Type)char_tlv.Type, data));
                            first = false;
                        }
                    }
                    else
                    {
                        var data = (TypeToCharacteristicValueFormat(tlvData.GetType()), char_tlv.Length) switch
                        {
                            (CharacteristicValueFormat.Int, TLVCharacteristicLength.One) => new byte[] { (byte)((int)tlvData) },
                            (CharacteristicValueFormat.Int, TLVCharacteristicLength.Two) => BitConverter.GetBytes((short)((int)tlvData)),
                            (CharacteristicValueFormat.Int, TLVCharacteristicLength.Four) => BitConverter.GetBytes((int)tlvData),
                            (CharacteristicValueFormat.uInt64, TLVCharacteristicLength.Eight) => BitConverter.GetBytes((ulong)tlvData),
                            (_, TLVCharacteristicLength.GUID) => ((Guid)tlvData).ToByteArray(),
                            (CharacteristicValueFormat.Data, TLVCharacteristicLength.N) => tlvData as byte[],
                            (CharacteristicValueFormat.String, TLVCharacteristicLength.N) => Encoding.UTF8.GetBytes(tlvData as string),
                            (_, TLVCharacteristicLength.N) => GenerateTLV(SubProp, tlvData),
                            _ => null
                        };
                        tlvOutput.Add(new((TLV.TLV_Type)char_tlv.Type, data));
                    }
                }
            }
            return TLV.TLVManager.Encode(tlvOutput);//.OrderBy(t => t.Type));//Make sure that we are sending them in ascending order. I don't know if HomeKit cares or not... :(
        }

        private static CharacteristicValueFormat TypeToCharacteristicValueFormat(Type a)
        {
            if (a==typeof(string))
            {
                return CharacteristicValueFormat.String;
            }
            if (a == typeof(bool))
            {
                return CharacteristicValueFormat.Bool;
            }
            if (a == typeof(int) || a.IsEnum)
            {
                return CharacteristicValueFormat.Int;
            }
            if (a == typeof(uint))
            {
                return CharacteristicValueFormat.uInt32;
            }
            if (a == typeof(ushort))
            {
                return CharacteristicValueFormat.uInt16;
            }
            if (a == typeof(ulong))
            {
                return CharacteristicValueFormat.uInt64;
            }
            if (a == typeof(byte) || a==typeof(byte?))
            {
                return CharacteristicValueFormat.uInt8;
            }
            if (a == typeof(float))
            {
                return CharacteristicValueFormat.Float;
            }
            if (a == typeof(byte[]))
            {
                return CharacteristicValueFormat.Data;
            }
            return CharacteristicValueFormat.TLV;

        }

        private static IEnumerable<string> PermissionsToStringArray(CharacteristicPermissions cp)
        {
            List<string> output = new();
            if ((cp & CharacteristicPermissions.PairedRead) == CharacteristicPermissions.PairedRead)
            {
                output.Add("pr");
            }
            if ((cp & CharacteristicPermissions.PairedWrite) == CharacteristicPermissions.PairedWrite)
            {
                output.Add("pw");
            }
            if ((cp & CharacteristicPermissions.Events) == CharacteristicPermissions.Events)
            {
                output.Add("ev");
            }
            if ((cp & CharacteristicPermissions.AdditionalAuthorization) == CharacteristicPermissions.AdditionalAuthorization)
            {
                output.Add("aa");
            }
            if ((cp & CharacteristicPermissions.Hidden) == CharacteristicPermissions.Hidden)
            {
                output.Add("hd");
            }
            if ((cp & CharacteristicPermissions.TimedWrite) == CharacteristicPermissions.TimedWrite)
            {
                output.Add("tw");
            }
            if ((cp & CharacteristicPermissions.WriteResponse) == CharacteristicPermissions.WriteResponse)
            {
                output.Add("wr");
            }
            return output;
        }

        public static object GetValueSingleCharacteristic(int aid, int iid, Camera.Poco c)
        {
            var prop = GetSingleProperty(c, iid);
            if (prop!=null)
            {
                object v = null;
                var characteristic = prop.GetCustomAttributes(typeof(CharacteristicAttribute), true).FirstOrDefault() as CharacteristicAttribute;
                if ((characteristic.Permissions & CharacteristicPermissions.PairedRead) == CharacteristicPermissions.PairedRead)
                {
                    v = prop.GetGetMethod().Invoke(c, null);
                    if (TypeToCharacteristicValueFormat(prop.PropertyType) == CharacteristicValueFormat.TLV)
                    {
                        v = Convert.ToBase64String(GenerateTLV(prop, v));
                    }
                }
                return v;

            }
            return null;
        }

        public static System.Reflection.PropertyInfo GetSingleProperty(Camera.Poco c, int iid)
        {
            var props = c.GetType().GetProperties();
            Dictionary<string, Services> services = new();
            int increment = 1;
            foreach (var prop in props)
            {
                var service = prop.GetCustomAttributes(typeof(ServiceAttribute), true).FirstOrDefault() as ServiceAttribute;
                var result = services.TryGetValue(service.UUID, out Services s);
                if (result == false)
                {
                    s = new()
                    {
                        type = new Guid(service.UUID),
                        characteristics = new List<Characteristics>(),
                        iid = increment,
                    };
                    increment++;
                    services.Add(service.UUID, s);
                }
                if (iid==increment)
                {
                    return prop;
                }
                increment++;

            }
            return null;

        }

        public static void WriteChanges(Camera.Poco c, PutCharacteristicsContainer inbound)
        {
            foreach (var item in inbound.characteristics.Where(ib => ib.value!=null))
            {
                var prop = GetSingleProperty(c, item.iid);
                var characteristic = prop.GetCustomAttributes(typeof(CharacteristicAttribute), true).FirstOrDefault() as CharacteristicAttribute;
                if (TypeToCharacteristicValueFormat(prop.PropertyType) == CharacteristicValueFormat.TLV)
                {
                    //with a poco and a binary TLV, this should be all I need. Right??
                    
                    
                    var tlv = TLV.TLVManager.Decode(Convert.FromBase64String(item.value.ToString()));
                    object v = prop.GetGetMethod().Invoke(c, null);
                    WriteTLV(v, tlv);
                }
                else if (TypeToCharacteristicValueFormat(prop.PropertyType) == CharacteristicValueFormat.uInt8)
                {
                    var i = Convert.ToByte(item.value.ToString());
                    prop.GetSetMethod().Invoke(c, new object[] { i });
                }
                else
                {
                    prop.GetSetMethod().Invoke(c, new object[] { item.value });
                }
            }
            //the final frontier
            return;
        }
        public static void WriteTLV(object v, IEnumerable<TLV.TLV> tlv)
        {
            var props = v.GetType().GetProperties();
            foreach (var TLVitem in tlv)
            {
                var t = (int)TLVitem.Type;
                var prop = props.Where(
                    p => p.GetCustomAttributes(typeof(TLVCharacteristicAttribute), true)
                    .Where(tt => (tt as TLVCharacteristicAttribute).IsWritable && (tt as TLVCharacteristicAttribute).Type == t)
                    .Any()).FirstOrDefault();
                var att = prop?.GetCustomAttributes(typeof(TLVCharacteristicAttribute), true)
                    .Where(tt => (tt as TLVCharacteristicAttribute).IsWritable && (tt as TLVCharacteristicAttribute).Type == t)
                    .FirstOrDefault() as TLVCharacteristicAttribute;
                if (prop==null)
                {
                    Console.WriteLine($"{v.ToString()} does not have a TLV of number {(int)TLVitem.Type}.");
                    continue;
                }
                if (prop.PropertyType==typeof(string))
                {
                    prop.GetSetMethod().Invoke(v, new object[] { Encoding.UTF8.GetString(TLVitem.Value) });
                }
                else if (att.Length == TLVCharacteristicLength.N && prop.PropertyType!=typeof(byte[]))
                {
                    //with a poco and a binary TLV, this should be all I need. Right??
                    object vv = prop.GetGetMethod().Invoke(v, null);
                    WriteTLV(vv, TLV.TLVManager.Decode(TLVitem.Value));
                }
                else if (att.Length == TLVCharacteristicLength.GUID)
                {
                    prop.GetSetMethod().Invoke(v, new object[] { new System.Guid(TLVitem.Value) });
                }
                else if (att.Length == TLVCharacteristicLength.One && prop.PropertyType == typeof(IEnumerable<int>))
                {
                    var value = new int[] { TLVitem.Value[0] };
                    prop.GetSetMethod().Invoke(v, new object[] { value });
                }
                else if (att.Length == TLVCharacteristicLength.One && (prop.PropertyType==typeof(int) || prop.PropertyType.IsEnum))
                {
                    var value = (int)(TLVitem.Value[0]);
                    prop.GetSetMethod().Invoke(v, new object[] { value });
                }
                else if (att.Length == TLVCharacteristicLength.Two && (prop.PropertyType == typeof(int) || prop.PropertyType.IsEnum))
                {
                    var value = (int)BitConverter.ToUInt16(TLVitem.Value);
                    prop.GetSetMethod().Invoke(v, new object[] { value });
                }
                else if (att.Length == TLVCharacteristicLength.Four && (prop.PropertyType == typeof(int) || prop.PropertyType.IsEnum))
                {
                    var value = (int)BitConverter.ToInt32(TLVitem.Value);
                    prop.GetSetMethod().Invoke(v, new object[] { value });
                }
                else
                {
                    prop.GetSetMethod().Invoke(v, new object[] { TLVitem.Value });
                }

            }
        }

        
    }
}
