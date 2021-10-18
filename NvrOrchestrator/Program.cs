/*
 * Where to start? You have to start somewhere and I found the desire to have a HomeKit camera and the actual market to be dismal. Thankfully, 
 * the community surrounding Homebridge showed me that you can have cameras that are normal, ONVIF cameras tie into HomeKit and they work. Nice!
 * 
 * But then I got picky. Like, I got a panoramic camera and did not like that you get a circle. LAME. So I made an xmap/ymap in FFMpeg and 
 * created multiple minature pictures from the large circle. But this created other, side effect issues for me. First, nobody that wrapped 
 * FFMpeg liked that. Second, filters didn't like that. Third, it was slow.
 * 
 * So I started investigating why it was slow and I found that it had to do more with initialization than actual latency. So, no problem, 
 * fire up a RTSP server and start publishing, right? RiGhT? Turns out I knew nothing about RTSP, nothing about what video formats looked 
 * great when (or how), when they worked, etc. I was sad to see my beautiful lawn reduced to large artifacts. But I just needed to adjust 
 * the compression, tweak a few settings, and read some user guides. And then I would be good?
 * 
 * Kind of.
 * 
 * Homekit (or, more precisely, Homebridge and the wonderful ffmpeg plugin) can be set to grab one frame of the video and call that a 
 * snapshot. Didn't work. Like at all. The reason? The initialization problem again. So I decided to grab jpegs of the stills and call 
 * that good. WORKED GREAT! Except now I'm dropping a new jpg to my high performance SSD a couple times a second, I have an HTTP server 
 * fired up, and I have yet another FFMPEG process working on doing that.
 * 
 * So to recap on the complexity metric, I have Homebridge, the ffmpeg plugin, a small HTTP server, a small RTSP server, 2 instances of 
 * FFMpeg running per desired camera stream, and I am still lacking on the following features:
 * * Motion detection
 * * Time Lapse
 * * Pruning of old videos
 * * Self healing of terminated processes
 * * RAM disk (or just not saving the blasted jpegs to my SSD!)
 * * Serving the videos in a reasonable manner to my phone (ahem HomeKit)
 * * An alerting mechanism for said motion detection
 * 
 * I was noticing I didn't need to be a video expert to do this, but I did need to leverage FFMpeg's excellent library and simply wrap 
 * around it. I could create triggers to do most of what I wanted, scheduled tasks, and otherwise handle whatever I needed via a simple 
 * ASP.NET Core running within this said process. It seems simple, and this is how I ended up here.
 * 
 * Oh, and when .NET 6 comes out, a good enough (TM) app to show stored videos. MAUI looks so awesome...
 * 
 * Did I mention the xmap/ymap thing doesn't let me use GPU encoding / decoding?
 * 
 * 
 * 2021-09-07 I finally got all encryption working so that the accessory adds to the Home in HomeKit. I'm pretty certain I broke Home, 
 * because now it crashes each time I try to click on my accessory. It's OK, because at this exact phase of my dev work I'm sending 
 * hard coded strings for Characteristics and Accessories.
 * 
 * I wanted to mention that I didn't try to make this in a black hole. There are so many people that worked the trenches before me, that 
 * willingly gave code for me to compare and inspect. I have a requirement: C# only. Often, this meant making test cases and trying to 
 * make the outputs the same through trial and error. Regardless, I wanted to thank the following projects on Github:
 * 
 * Homebridge (HAP-NodeJS specifically) as it provided excellent test cases
 * The HomeKit spec (because this bad boy is poorly organized, I couldn't thank it first). The description of encryption leaves a lot to 
 *      be desired. I got used to barely any notation of how encryption works. Like the tiniest of notation that the 12 byte nonce is 
 *      8 bytes for Apple, and the first 4 bytes are always 0 on ChaCha. Yikes.
 * The guy that made the C# SRP. I'm sorry I couldn't use your code. It wasn't homekit friendly and I found it easier just to read the 
 *      spec and make new. You did inspire me though.
 * The wonderful people that made the C# implementation of ChaCha20Poly1305, and Curve25519. I have no words to the level of smarts I 
 *      see in those projects.
 * 
 * And while we are at it, my apologies for the janky HTTP server. I made a new one instead of TCP wrapping a battle hardened one like 
 *     HAP-NodeJS did, and I kept wondering if this was going to bite me in the ass. Surprisingly, it hasn't yet.
 *     
 * I didn't plan on doing HKSV when I made this, but I'm seriously contemplating putting it on the roadmap. I had fully intended on storing
 *    the video myself. I likely will still do that. My chief complaint of Ring cameras is the lack of full time video. HKSV looks to be
 *    in the same boat, as it doesn't record full time. Only on motion. But, if we do both, then a HomeKit friendly setup will be able to use
 *    the HomeKit stuff primarily and use mine as a backup, should something not record.
 *    
 * 2021-09-17 I've been struggling with Apple's overuse of TLVs. When you work the encryption piece, it makes sense. When you work the onion 
 *    like layers of camera parameters, you realize someone did a horrible job. I hope my implementation is easy to read, as that was my 
 *    objective over speed.
 *
 * 2021-09-19 I finally got my /accessories to output correct JSON (emitting null values, tsk tsk) and HomeKit likes me again. :)
 *
 * 2021-09-20 I got the /accessories done (and I like it) and GET /characteristics done (and I mostly like it) and I still need to work the PUT characteristics and EVENTS.
 *    I've really been thinking about events.
 *    What I want to do is have really tight, portable code. There's nice stuff out there that's not free, or it seriously bloats the system. 
 *    Other stuff messes with pointers and goes into unsafe territory.
 *    I looked to see what Entity Framework did, and the docs say they have a state, and compare the state on SaveChanges.
 *    This seems like the best way to do it, but this is a pull method with a factored in delay, when the spec really wants to see a push implementation.
 *    Being that I have to be thread safe, and I have to boil it up to the particular thread that wants the event, I think the easiest thing is to implement states.
 *      I have a serializer already implemented so if I work that some more for events, I should be OK.
 *      It's late and I'm about to head to bed. But if I have a state machine be a list of subscribed identifiers, then I can shortcut the reflection to only those characteristics. Then, I can
 *      make the state machine work at the end of any data request, per thread. This should meet all requirements on page 79.
 *      
 * 2021-10-03 Yesterday and today I made my Bonjour Service an honorable class. It still has some ways to go to have every feature I want,
 *    but as it sits, it works fine. I just need the Config# to increment without restarting the app, and to not manually change the pair indicator.
 *    Worlds ahead of what I had before. The week prior I made my own ChaCha and Poly implementation. Why? Becuase I want to have 100%, managed, safe code.
 *    It is not the fastest out there (it resembles more the reference implementation) but I am writing this in C#, after all. Computer
 *    speeds are not the reason to choose C#. If I wanted that, I'd use rust or C.
 *    
 *    I presently use 0 github or nuget packages. I still need to:
 *      Integrate with FFMPEG
 *      Write my Time Lapse, Motion Detection, Self Healing, RTSP. Do I need RTSP?
 *      
 * 2021-10-11 I've been slow to work on this and yesterday I picked it up a little, and today I picked it up a little more. I now know a few things I need:
 *     I need to finish PUT. Presently I serialize it, send it to a function with the poco, and so what I really need to do is to detect the ID values and 
 *       then write a conversion routine. This outta be fantastic.
 *     I do need RTSP. So what will happen is FFMPEG will be feeding me a stream constantly. Then on setup I will be given a key, salt, and a destination 
 *       address, and I need to send the information there, encrypted. Homebridge had FFMPEG do this, but it was SLLOOOOOOOOOOOWW. If I handle the encryption 
 *       (which is AES anyway) then it should be as fast as I can get it.
 *     I still need to make the nonHomekit stuff
 *     I still need to make a better config file standard
 *     I still need to make this work with multiple cameras
 *     Then I need to github publish it.
 *     
 *  I've thought about the github publishing more, as I literally have no backup of this code today. But I'm not really ready to show it to the world, 
 *  either. I have to prohibit commercial use of this, since I used the Non Commercial version of the spec to make it. However, if a company wants to 
 *  W-2 me briefly, I can certainly tweak this to be commercial viable with their MFI license. It should be really, really close.
 *  
 *  2021-10-14 I made the PUT routine. I noticed there is a "write response" characteristic that can really cloud this up, but none of my routines use
 *     this, so I didn't implement it. I now have a predictable communication line with the ios device: 
 *     1. Are you Available? (Streaming Status)
 *     2. Setup Endpoints (write)
 *     3. Setup Endpoints (read)
 *     4. GO! (Selected RTP Stream)
 *     However, after item 4, it never takes my RTP stream. I'm feeding the configuration to ffmpeg and ffmpeg sends the data, but the key differentiator
 *     I see is that the ios device never contacts my port for RTCP commands. It is supposed to every half second (its choice). It never asks for a refresh
 *     of any setting, so it's not like it is doing another "Are you available?" and I'm answering wrong.......
 * 
 *  2021-10-15 I found my bug. strings were not being serialized properly to a TLV structure. That's why it never attempted to contact me. I was able to 
 *     wire up the start / stop commands. I didn't attempt the reconfigure command...
 *     
 *  2021-10-17 OK WOW. I moved 4 cameras to this today. There are some things I need to reliably do:
 *     . Not have to manually set a persistent ID
 *     . Not have to manually pick a TCP port (I can just grab one......)
 *     . Have a monitor that stops / starts the executable if something goes wrong
 *     
 *     I'm walking away for a bit. I love the progress made in just a few days' time, but I'm ready to tackle something else.
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using NvrOrchestrator.HomeKit.TLV;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using NvrOrchestrator.Encryption.SRP;
using NvrOrchestrator.Bonjour;
using System.Diagnostics;

namespace NvrOrchestrator
{
    class Program
    {
        public static SRP srp = new(new(Encryption.SRP.Params.N), new(Encryption.SRP.Params.g));

        static void Main(string[] args)
        {
            if (args.Length!=4)
            {
                Console.WriteLine("USAGE: NAME ID PORT INPUT\r\nIf you want the name to have spaces, enclose the name in double quotes. Always enclose the input in double quotes.");
            }
            var poco = Configuration.Global.Poco;
            Configuration.Global.ID = args[1];
            poco.Name = args[0];// "Walkway";
            Configuration.Global.TCPPort = Convert.ToUInt16(args[2]);
            Configuration.Global.CameraInput = args[3];//"-i rtsp://192.168.1.253:7070/stream1 -i Walkway\\xmap.pgm -i Walkway\\ymap.pgm";
            poco.SupportedVideoConfiguration.VideoCodecConfiguration = new HomeKit.HAP.VideoCodecConfiguration[]
            {
                new HomeKit.HAP.VideoCodecConfiguration()
                {
                    VideoCodecParameters = new HomeKit.HAP.VideoCodecParameters()
                    {
                        ProfileID = new int[] { 0, 1, 2 },
                        Level = new int[] { 0, 1, 2 },
                        PacketizationMode = new int[] { 0 },
                    },
                    VideoAttributes = new HomeKit.HAP.VideoAttributes[]
                    {
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 320, ImageHeight = 180 },//HAP Non Commercial Page 245.
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 15, ImageWidth = 320, ImageHeight = 240 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 320, ImageHeight = 240 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 480, ImageHeight = 270 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 480, ImageHeight = 360 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 640, ImageHeight = 360 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 640, ImageHeight = 480 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 1280,ImageHeight = 720 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 1280,ImageHeight = 960 },
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 1920,ImageHeight = 1080},
                        new HomeKit.HAP.VideoAttributes() { FrameRate = 30, ImageWidth = 1600,ImageHeight = 1200}
                    }
                }
            };
            poco.SetupEndpoints.AccessoryAddress.IPAddress = NetworkingHelpers.IPHelper.ActiveListeners().Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault().ToString();
            poco.SetupEndpoints.AccessoryAddress.IPAddressVersion = HomeKit.HAP.ControllerAddressIPAddressVersion.IPv4;
            poco.SetupEndpoints.AccessoryAddress.AudioRTPPort = 53067;
            poco.SetupEndpoints.AccessoryAddress.VideoRTPPort = 53083;
            poco.SetupEndpoints.VideoSyncSource = new Random().Next() & 0x00ffffff;
            poco.SetupEndpoints.AudioSyncSource = new Random().Next() & 0x00ffffff;

            var SRPb = new byte[32];
            RandomNumberGenerator.Create().GetBytes(SRPb);

            var SRPs = new byte[16];
            RandomNumberGenerator.Create().GetBytes(SRPs);

            srp.s = new(SRPs);
            srp.I = new("Pair-Setup");
            srp.p = new(Configuration.Global.Password);
            srp.b = new(SRPb);
            _ = srp.x;//this forces v to be created;

            BonjourServer.AddServices(
                BonjourManager.ConfigureBonjour(
                    poco.Name, //Friendly Name. :)
                    Configuration.Global.ID, //Same ID used within HomeKit
                    Configuration.Global.TCPPort,//Port in use for the HomeKit HAP Server
                    Configuration.Global.ConfigurationRevision, //This should increment over time.
                    PairingFeatureFlags.UseThis, //this should be 0. HomeKit spec says there is bit 1 and 2, but not what they are for...
                    Directory.GetFiles(poco.Name + "\\Devices\\").Any() ? BonjourStatusFlags.OK : BonjourStatusFlags.NotPaired, //This says we are paired. 
                    AccessoryCategory.VideoDoorbell));//IP Camera

            _ = Task.Run(BonjourServer.ReceiveBroadcaseMessages);
            _ = Task.Run(RetainImage);
            _ = Task.Run(RTCPReturnServer);
            _ = Task.Run(RTCPReturnServer2);
            HTTPServer();
            Console.ReadKey();
        }

        public static byte[] ImageBuffer = null;

        public static Task RetainImage()
        {
            Process proc = new();
            proc.StartInfo.FileName = "ffmpeg.exe";
            proc.StartInfo.Arguments = "-threads 16 -hide_banner -loglevel error -thread_queue_size 32 " + Configuration.Global.CameraInput + " -f image2pipe -";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            

            proc.Start();

            System.IO.MemoryStream ms = new();
            byte[] buffer = new byte[0x1000];
            while(true) 
            {
                System.IO.FileStream fs = proc.StandardOutput.BaseStream as FileStream;
                int count = fs.Read(buffer, 0, 0x1000);
                bool found = false;
                for (int i = 1; i < count; i++)
                {
                    if (buffer[i-1]==0xFF && buffer[i]==0xD9)
                    {
                        //EOI marker. we have a complete image now.
                        ms.Write(buffer, 0, i + 1);
                        ImageBuffer = ms.ToArray();
                        ms.Position = 0;
                        ms.SetLength(0);
                        ms.Write(buffer, i + 1, count - (i + 1));
                        found = true;
                        break;
                    }
                }
                if (!found)
                    ms.Write(buffer, 0, count);
            }
        }
        public static void RTCPReturnServer()
        {
            EndPoint AnyRemoteBonjourAddress = new IPEndPoint(IPAddress.Any, 53083);
            EndPoint AnyRemoteAddress = new IPEndPoint(IPAddress.Any, 0);

            var mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mcastSocket.Bind(AnyRemoteBonjourAddress);
            byte[] packet = new byte[1500];
            while (true)
            {
                mcastSocket.ReceiveFrom(packet, ref AnyRemoteAddress);
            }
        }
        public static void RTCPReturnServer2()
        {
            EndPoint AnyRemoteBonjourAddress = new IPEndPoint(IPAddress.Any, 53067);
            EndPoint AnyRemoteAddress = new IPEndPoint(IPAddress.Any, 0);

            var mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mcastSocket.Bind(AnyRemoteBonjourAddress);
            byte[] packet = new byte[1500];
            while (true)
            {
                mcastSocket.ReceiveFrom(packet, ref AnyRemoteAddress);
            }
        }

        public static async void HTTPServer()
        {
            var server = new TcpListener(IPAddress.Any, Configuration.Global.TCPPort);
            server.Start();
            while (true)
            {
                var client = await server.AcceptTcpClientAsync().ConfigureAwait(false);
                var cw = new Connection(client);
                _ = Task.Run(cw.DoSomethingWithClientAsync);
            }
        }

        public class Connection
        {
            public TcpClient client;
            public Connection(TcpClient client)
            {
                this.client = client;
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task DoSomethingWithClientAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                //Console.WriteLine("Connection accepted!");
                List<HomeKit.Events.EventPoco> subscribedEvents = new();
                try
                {
                    MemoryStream ms = new();
                    byte[] input = new byte[1024];
                    HomeKit.SessionVariables.Encryption enc = null;
                    using var stream = client.GetStream();
                    while (true)
                    {
                        if (!client.Connected) break;

                        //Check to see if Events changed...
                        foreach (var item in subscribedEvents)
                        {
                            var newState = HomeKit.HAP.HAPManager.GetValueSingleCharacteristic(item.aid, item.iid, Configuration.Global.Poco);
                            if ((item.LastState!=null && !item.LastState.Equals(newState)) || item.Triggered)
                            {
                                Console.WriteLine($"Event {item.iid} changed, emitting EVENT...");
                                string data = "";
                                if (item.Triggered)
                                {
                                    item.Triggered = false;
                                    var container = new HomeKit.HAP.GetCharacteristicsContainer()
                                    {
                                        characteristics = new HomeKit.HAP.GetCharacteristics[]
                                        {
                                            new HomeKit.HAP.GetCharacteristics()
                                            {
                                                aid = item.aid,
                                                iid = item.iid,
                                                value = item.TriggerValue
                                            }
                                        }
                                    };
                                    data = JsonSerializer.Serialize(container, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                                }
                                else
                                {
                                    data = JsonSerializer.Serialize(HomeKit.HAP.HAPManager.SerializeFromParameterList($"id={item.aid}.{item.iid}", Configuration.Global.Poco), new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                                    item.LastState = newState;
                                }
                                HomeKit.HTTP.HTTP.SendResponse(stream, Encoding.UTF8.GetBytes(data), "application/hap+json", enc, true);

                            }
                        }

                        while (stream.DataAvailable)
                        {
                            int bytesRead = stream.Read(input, 0, input.Length);
                            ms.Write(input, 0, bytesRead);

                        }

                        if (ms.Length != 0)
                        {
                            //we've received at least one packet.
                            //packets can be split, and we rely upon the higher level on the stack to know if it has. Realistically, though, modern networks don't fragment unless they have to.
                            var request = HomeKit.HTTP.HTTP.ParseMessage(ms.ToArray(), enc);
                            ms = new MemoryStream();
                            switch (request.URL.ToLower())
                            {
                                case "/pair-setup":
                                    Pair_Setup.PairSetup(stream, TLVManager.Decode(request.Body));
                                    break;
                                case "/pair-verify":
                                    Pair_Verify.PairVerify(stream, TLVManager.Decode(request.Body), ref enc);
                                    break;
                                case "/pairings":
                                    Pairing.Pairings(stream, TLVManager.Decode(request.Body), enc);
                                    break;
                                case "/accessories":
                                    Accessories.GetAccessories(enc, stream);
                                    break;
                                case "/characteristics":
                                    Characteristic.Parser(subscribedEvents, enc, stream, request);
                                    break;
                                case "/resource":
                                    HomeKit.HTTP.HTTP.SendResponse(stream, ImageBuffer, "image/jpeg", enc: enc);
                                    break;
                                case "/testdoorbell":
                                    foreach ( var ev in Configuration.Global.EventSubscriptions.Where(ev => ev.iid==26))
                                    {
                                        ev.TriggerValue = 0;
                                        ev.Triggered = true;
                                    }
                                    HomeKit.HTTP.HTTP.SendResponse(stream, Encoding.UTF8.GetBytes("doorbell triggered!!!!!"), "text/plain");
                                    break;
                                default:
                                    Console.WriteLine("new URL found: " + request.URL);
                                    HomeKit.HTTP.HTTP.SendResponse(stream, Encoding.UTF8.GetBytes("unknown"), "text/plain");
                                    break;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                foreach (var item in subscribedEvents)
                                {
                                    if (item.Triggered)
                                        break;//check every 50 milliseconds...if set to true, jump right to the code that will find the event change...
                                }
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {

                    
                    if (client != null)
                    {
                        (client as IDisposable).Dispose();
                        client = null;
                    }
                }
                Console.WriteLine("Connection closed.");
            }

        }
    }

    public static class TestMessages
    {

        public static byte[] ToBytes(this string hex)
        {
            var hexAsBytes = new byte[hex.Length / 2];

            for (var i = 0; i < hex.Length; i += 2)
            {
                hexAsBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return hexAsBytes;
        }

        public static byte[] K = "1c50c38b84c83deb009cbe30d83f0d0c3926cf1a73a56f539c11cb8b6b34a1746b50847041096a41d5113b00f99d4c06393615845924b676dc84f5812884c159".ToBytes();
        public static byte[] material = "0b5f670bfe7ab0edc8340b8aa2b0cacca586894f6b4ce0ca340418f1aa87f3bd43433a32323a33443a45333a43453a33304b2b642ca52df5f62b372dc42763e315b725c84ef486c3ddb2797c981368a008".ToBytes();
        public static byte[] privateKey = "e7211a865f8dd21fe2332a18c407054902a2a4a326c9cd76f9c972e71a18f25f4b2b642ca52df5f62b372dc42763e315b725c84ef486c3ddb2797c981368a008".ToBytes();
        public static byte[] message = "011143433a32323a33443a45333a43453a333003204b2b642ca52df5f62b372dc42763e315b725c84ef486c3ddb2797c981368a0080a40e2a369fedddaff4571811fdffa5541f1c085897ed898d25e2eae9659ea61e651039d3e6a9d442e3fc174c6516e8cbae8a89943b6e4385e182ac6b5996e097a09".ToBytes();
        public static byte[] encKey = "97b50e0c88dccb42bb7928e9a487574489306e3933ba2759c00e467278592bfc".ToBytes();
        public static byte[] ecipherText = "dfadb1fde27cdf2dd470cc6d3d9b875b15726680a05f28abfc1e1ce8e8851174414b206b3636ab8ed737413ab04c842a2cb1c2f4d896515bb04a490da0f8efb8d38bfdb335431e32df973543201e584c6f9c1b511bdcd217dd6da0b21c7968bc1c8ad002ef97b73c9013e2de232929536cdfc5564afdce".ToBytes();
        public static byte[] eauthTag = "2140cca22294ad6e1fc7c1bff9e53fc8".ToBytes();
        public static string GetAccessories = @"{
""accessories"" : [
{
""aid"" : 1,
""services"" : [
{
""type"" : ""3E"",
""iid"" : 1,
""characteristics"" : [
{
""type"" : ""23"",
""value"" : ""Acme Light Bridge"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 2
},
{
""type"" : ""20"",
""value"" : ""Acme"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 3
},
{
""type"" : ""30"",
""value"" : ""037A2BABF19D"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 4
},
{
""type"" : ""21"",
""value"" : ""Bridge1,1"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 5
},
{
""type"" : ""14"",
""value"" : null,
""perms"" : [ ""pw"" ],
""format"" : ""bool"",
""iid"" : 6
},
{
""type"" : ""52"",
""value"" : ""100.1.1"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 7
},
]
},
{
""type"" : ""A2"",
""iid"" : 8,
""characteristics"" : [
{
""type"" : ""37"",
""value"" : ""01.01.00"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 9
}
]
}
]
},
{
""aid"" : 2,
""services"" : [
{
""type"" : ""3E"",
""iid"" : 1,
""characteristics"" : [
{
""type"" : ""23"",
""value"" : ""Acme LED Light Bulb"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 2
},
{
""type"" : ""20"",
""value"" : ""Acme"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 3
},
{
""type"" : ""30"",
""value"" : ""099DB48E9E28"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 4
},
{
""type"" : ""21"",
""value"" : ""LEDBulb1,1"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 5
},
{
""type"" : ""14"",
""value"" : null,
""perms"" : [ ""pw"" ],
""format"" : ""bool"",
""iid"" : 6
}
]
},
{
""type"" : ""43"",
""iid"" : 7,
""characteristics"" : [
{
""type"" : ""25"",
""value"" : true,
""perms"" : [ ""pr"", ""pw"" ],
""format"" : ""bool"",
""iid"" : 8
},
{
""type"" : ""8"",
""value"" : 50,
""perms"" : [ ""pr"", ""pw"" ],
""iid"" : 9,
""maxValue"" : 100,
""minStep"" : 1,
""minValue"" : 20,
""format"" : ""int"",
""unit"" : ""percentage""
}
]
}
]
},
{
""aid"" : 3,
""services"" : [
{
""type"" : ""3E"",
""iid"" : 1,
""characteristics"" : [
{
""type"" : ""23"",
""value"" : ""Acme LED Light Bulb"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 2
},
{
""type"" : ""20"",
""value"" : ""Acme"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 3
},
{
""type"" : ""30"",
""value"" : ""099DB48E9E28"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 4
},
{
""type"" : ""21"",
""value"" : ""LEDBulb1,1"",
""perms"" : [ ""pr"" ],
""format"" : ""string"",
""iid"" : 5
},
{
""type"" : ""14"",
""value"" : null,
""perms"" : [ ""pw"" ],
""format"" : ""bool"",
""iid"" : 6
}
]
},
{
""type"" : ""43"",
""iid"" : 7,
""characteristics"" : [
{
""type"" : ""25"",
""value"" : true,
""perms"" : [ ""pr"", ""pw"" ],
""format"" : ""bool"",
""iid"" : 8
},
{
""type"" : ""8"",
""value"" : 50,
""perms"" : [ ""pr"", ""pw"" ],
""iid"" : 9,
""maxValue"" : 100,
""minStep"" : 1,
""minValue"" : 20,
""format"" : ""int"",
""unit"" : ""percentage""
}
]
}
]
}
]
}";
    }
}


/*
 
48 54 54 50 2F 31 2E 31 20 32 30 30 20 4F 4B 0D 0A 43 6F 6E 74 65 6E 74 2D 54 79 70 65 3A 20 61 70 70 6C 69 63 61 74 69 6F 6E 2F 70 61 69 72 69 6E 67 2B 74 6C 76 38 0D 0A 44 61 74 65 3A 20 54 75 65 2C 20 31 30 20 41 75 67 20 32 30 32 31 20 31 38 3A 32 30 3A 32 36 20 47 4D 54 0D 0A 43 6F 6E 6E 65 63 74 69 6F 6E 3A 20 6B 65 65 70 2D 61 6C 69 76 65 0D 0A 54 72 61 6E 73 66 65 72 2D 45 6E 63 6F 64 69 6E 67 3A 20 63 68 75 6E 6B 65 64 0D 0A 0D 0A 31 39 39 0D 0A 06 01 02 02 10 30 C2 44 07 59 95 8A 6E 0D E6 2A 3D F4 FF 13 29 03 FF 90 F7 BC 68 57 7E 3E 87 97 1D 1C F2 5A E3 F8 8F 2E 44 AB A4 18 37 6A 75 62 7E B2 1A D2 6F 21 2E 05 3A 71 73 77 E3 F2 A5 6F 01 4F 5C C5 68 78 AB C1 E4 17 7C B8 06 A2 35 B7 F5 32 6E 2C 93 D7 E9 69 15 99 1C 07 D1 C1 B8 5E 8D A4 3F 5F A5 95 04 63 8D F2 CB 30 CB 02 C3 4A C0 A5 4F 6A 75 45 C9 FD CA 4D D3 43 5E 07 55 74 56 29 0E AF AD B3 EF 02 61 61 10 BD 17 07 1A F8 AC 56 85 EB 78 89 C3 E9 C4 B9 FE A9 6A 91 1D DE D7 F4 82 C6 88 2C 1A 92 A9 95 0A 03 3F 39 04 13 40 08 6D 78 5C 77 5D DB 08 47 35 C3 AF F3 14 B3 24 0B 55 E8 1C 0B F4 EE 14 E8 A7 36 FC 3B 10 6A 99 65 3C 61 0E 67 36 AA 6A 46 52 D9 76 B5 93 9D ED 8E F9 EA 2E A1 7A 86 B3 E0 0B 49 83 E0 6E 1E 5F 70 F1 A0 8D 29 8F 55 11 87 0F 96 A5 56 B5 A8 60 8D 1B 73 11 94 BC 5C B1 60 9A D9 E0 40 84 0B 51 78 EA FD E3 1F 03 81 59 6E AD 44 38 36 04 B6 EE 13 CE 76 F2 81 42 1C B5 00 C1 DF 70 50 11 7D 7E D4 DD 29 6E AB B4 73 88 12 27 2C 04 AA 7E F8 B5 2C 60 92 DE D6 DC 79 5E 57 FE 25 92 40 8A 76 60 22 24 9C 80 0D 76 0C 9A 8C 9A 06 29 2E 79 21 6C 24 64 74 FA D0 46 88 44 DC DB 3D 77 83 CE D3 3D F5 D1 44 4A 7D C6 62 7F 59 52 F5 A6 C2 E9 23 AB D7 FA 41 8B E0 91 F1 D3 AA BE B6 48 C4 E6 A0 00 7E 0D 30 44 E6 07 D9 88 0D 0A 30 0D 0A 0D 0A 
HTTP/1.1 200 OK..Content-Type: application/pairing+tlv8..Date: Tue, 10 Aug 2021 18:20:26 GMT..Connection: keep-alive..Transfer-Encoding: chunked....199.......0ÂD.Yn.æ*=ôÿ.).ÿ÷¼hW~>..òZãø.D«¤.7jub~².Òo!..:qswãò¥o.O\Åhx«Áä.|¸.¢5·õ2n,×éi...ÑÁ¸^¤?_¥.còË0Ë.ÃJÀ¥OjuEÉýÊMÓC^.UtV).¯­³ï.aa.½...ø¬V
ëxÃéÄ¹þ©j.Þ×ôÆ,.©..?9..@.mx\w]Û.G5Ã¯ó.³$.Uè..ôî.è§6ü;.je<a.g6ªjFRÙvµíùê.¡z³à.Iàn._pñ )U..¥Vµ¨`.s.¼\±`Ùà@.Qxêýã..Yn­D86.¶î.ÎvòB.µ.ÁßpP.}~ÔÝ)n«´s.',.ª~øµ,`ÞÖÜy^Wþ%@v`"$.v..).y!l$dtúÐFDÜÛ=wÎÓ=õÑDJ}ÆbYRõ¦Âé#«×úAàñÓª¾¶HÄæ .~.0Dæ.Ù..0....
 
 */