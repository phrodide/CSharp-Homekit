using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.Configuration
{
    /*
     This config should change easily and persis across restarts.
    It should hold LTPK, LTSK, AccessoryID, all Devices with LTPK, the Accessory JSON, a Configuration # that increments on change.
    The configuration number should allow for textual changes. So we should sign the text file and if it changes, increment the configuration # and write the new signature to a separate file.
     */
    public static class Global
    {
        static Global()
        {
            Password = "525-69-424";
            if (System.IO.File.Exists("AccessoryLTPK.bin") && System.IO.File.Exists("AccessoryLTSK.bin"))
            {
                LTPK = System.IO.File.ReadAllBytes("AccessoryLTPK.bin");
                LTSK = System.IO.File.ReadAllBytes("AccessoryLTSK.bin");
            }
            else
            {
                byte[] randomBytes = new byte[32];
                System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
                byte[] _LTPK = null;
                byte[] _LTSK = null;
                Chaos.NaCl.Ed25519.KeyPairFromSeed(out _LTPK, out _LTSK, randomBytes);
                LTPK = LTPK;
                LTSK = LTSK;
            }
            if (System.IO.File.Exists("AccessoryPairingID.bin"))
            {
                ID = System.IO.File.ReadAllText("AccessoryPairingID.bin");
            }
            else
            {
                var networkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().First().GetPhysicalAddress();
                ID = string.Join(":", networkInterface.GetAddressBytes().Select(c => String.Format("{0:X2}", Convert.ToInt32(c))));
            }

            ConfigurationRevision = 2;
            StatusFlag = 0;
            TCPPort = 55343;
            //AccessoryJSON = System.IO.File.ReadAllText("AccessoryJSON.txt");

        }
        public static string Password { get; set; }
        public static byte[] LTPK { get; set; }
        public static byte[] LTSK { get; set; }
        public static string ID { get; set; }
        public static int StatusFlag { get; set; }
        public static IList<Devices> PairedDevices { get; set; }
        public static string AccessoryJSON { get; set; }
        public static int ConfigurationRevision { get; set; }
        public static ushort TCPPort { get; set; }

        public static HomeKit.HAP.Camera.Poco Poco { get; set; } = new HomeKit.HAP.Camera.Poco();

        public static List<HomeKit.Events.EventPoco> EventSubscriptions { get; set; } = new();

        public static int ActiveCamera { get; set; }

        public static string CameraInput { get; set; }

    }

    public static class Signature
    {
        public static byte[] ConfigurationSignature { get; set; }
    }

    public class Devices
    {
        public string DeviceName { get; set; }
        public string LTPK { get; set; }
    }
}
