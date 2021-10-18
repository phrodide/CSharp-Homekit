using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP.Camera
{
    public class SampleFFMpegStream
    {
        public static void FFMPEG()
        {
            var c = Configuration.Global.Poco;
            c.StreamStatus.Status = StreamingStatusStatus.InUse;
            System.IO.File.WriteAllText("camerapoco.txt", System.Text.Json.JsonSerializer.Serialize(c));
            System.Threading.Thread.Sleep(50);//give the other thread 50 milliseconds to finish parsing the request.
            var videobytes = c.SetupEndpoints.SRTPParametersForVideo.SRTPCryptoKey.Concat(c.SetupEndpoints.SRTPParametersForVideo.SRTPMasterSalt).ToArray();
            var videosrtp = Convert.ToBase64String(videobytes);
            var audiosrtp = Convert.ToBase64String(c.SetupEndpoints.SRTPParametersForAudio.SRTPCryptoKey.Concat(c.SetupEndpoints.SRTPParametersForAudio.SRTPMasterSalt).ToArray());
            var command = Configuration.Global.CameraInput + 
                $" -an -sn -dn -c:v libx264 -pix_fmt yuv420p -color_range mpeg -r 30 -f rawvideo -b:v 299k -payload_type 99 " +
                $"-ssrc {c.SetupEndpoints.VideoSyncSource} -f rtp -srtp_out_suite AES_CM_128_HMAC_SHA1_80 -srtp_out_params {videosrtp} srtp://{c.SetupEndpoints.ControllerAddress.IPAddress}:{c.SetupEndpoints.ControllerAddress.VideoRTPPort}?rtcpport={c.SetupEndpoints.ControllerAddress.VideoRTPPort}&pkt_size=1316 " + 
                $"-vn -sn -dn -c:a libfdk_aac -profile:a aac_eld -flags +global_header -f null -ar 16k -b:a 24k -ac 1 -payload_type 110 " + 
                $"-ssrc {c.SetupEndpoints.AudioSyncSource} -f rtp -srtp_out_suite AES_CM_128_HMAC_SHA1_80 -srtp_out_params {audiosrtp} srtp://{c.SetupEndpoints.ControllerAddress.IPAddress}:{c.SetupEndpoints.ControllerAddress.AudioRTPPort}?rtcpport={c.SetupEndpoints.ControllerAddress.AudioRTPPort}&pkt_size=188 " +
                "-loglevel level -progress pipe:1";
            System.IO.File.WriteAllText("cameracommand.txt", command);
            Console.WriteLine("ffmpeg command " + command);
            Process proc = new();
            proc.StartInfo.FileName = "ffmpeg.exe";
            proc.StartInfo.Arguments = command;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;


            proc.Start();

            Configuration.Global.ActiveCamera = 1;

            while (Configuration.Global.ActiveCamera!=0)
            {
                System.Threading.Thread.Sleep(50);
            }

            proc.Kill();
            c.StreamStatus.Status = StreamingStatusStatus.Available;
        }
    }
}
