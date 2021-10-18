using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.Bonjour
{
    public class BonjourPoco
    {
        public IEnumerable<System.Net.IPAddress> ActiveListeners { get; set; }
        public ushort Port { get; set; }
        public string ARecord { get; set; }//used to answer A records. FQDN
        public string PTRRecord { get; set; }//used to qualify SRV record
        public IEnumerable<string> TXTRecords { get; set; }
    }

    public class mDNSResourcePoco
    {
        public mDNSResourceCategory Category { get; set; }
        public string Name { get; set; }
        public mDNSResourceTypes ResourceType { get; set; }
        public mDNSResourceClass ResourceClass { get; set; }
        public uint TTL { get; set; }
        public virtual byte[] ResourceData { get; set; }
        public ushort SRV_Priority { get; set; }
        public ushort SRV_Weight { get; set; }
        public ushort SRV_Port { get; set; }
        public string SRV_Target { get; set; }
        public string PTR_DomainNamePointer { get; set; }
        public System.Net.IPAddress AAAA_IPAddress { get; set; }
        public System.Net.IPAddress A_IPAddress { get; set; }
        public List<string> TXT { get; set; } = new();
        public string NSEC_NXTDomainName { get; set; }
        public int NSEC_Error { get; set; }
        public string NS_NameServer { get; set; }

    }

    public enum mDNSResourceCategory
    {
        Question,
        Answer,
        NameServer,
        AdditionalInfo
    }

    public enum mDNSResourceClass
    {
        Internet = 1
    }

    public enum mDNSResourceTypes
    {
        A = 1,
        AAAA = 28,
        NSEC = 47,
        PTR = 12,
        OPT = 41,
        NS = 2,
        TXT = 16,
        SRV = 33,
    }

    public enum BonjourStatusFlags
    {
        OK=0,
        NotPaired=1,
        ProblemDetected=4
    }

    public enum PairingFeatureFlags
    {
        UseThis = 0,
        Unknown1 = 1,
        Unknown2 = 2,
        Unknown4 = 4
    }

    public enum AccessoryCategory//I welcome someone using this for more than I am. Simply start here, and work your way to better things.
    {
        IPCamera=17,
        VideoDoorbell=18
    }

}
