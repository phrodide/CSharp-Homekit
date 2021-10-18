using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NvrOrchestrator.Encryption.ChaCha20Poly1305;

namespace NvrOrchestrator.HomeKit.HTTP
{
    public class HTTP
    {
        /*
         * This HTTP server is janky. It parses just enough to get the information I need, and otherwise it dumps the info. Any non-HomeKit http needs should use another well established http server.
         */
        public static HTTPRequest ParseMessage(byte[] data, SessionVariables.Encryption enc = null)
        {
            if (enc?.IsEncrypted == true)
            {
                var aad = data.Take(2).ToArray();
                var len = BitConverter.ToUInt16(aad, 0);
                var crypTex = data.Skip(2).Take(len).ToArray();
                var authData = data.TakeLast(16).ToArray();
                var sanityCheck = data.Length - (2 + crypTex.Length + 16);//sanityCheck should be 0.
                var plaintextData = ChaChaPolyManager.ChaCha20Poly1305_Decrypt(Encoding.UTF8.GetBytes("\0\0\0\0").Concat(BitConverter.GetBytes(enc.ReadNonce++)).ToArray(), crypTex, aad, enc.ControllerToAccessoryKey, authData); 
                data = plaintextData;
            }
            //HTTP header information terminates with a double CRLF. split on the first occurrence of that to define headers from data.
            //HTTP headers come in one special way and then in a standard way. The first line should be a GET/POST then a URL then a version,
            ////then the rest of them are separted by a colon from their type and data.
            //Console.WriteLine("HTTP Message Inbound");
            string headers = Encoding.UTF8.GetString(data.Take(FindFirstOccurrence(data, Encoding.ASCII.GetBytes("\r\n\r\n"))).ToArray());
            List<string> headerArray = headers.Replace("\r\n", "\r").Split('\r').ToList();
            List<HTTPRequestHeader> ParsedHeaders = new();
            foreach (var singleHeader in headerArray.Skip(1))
            {
                var splitter = singleHeader.IndexOf(':');
                if (splitter == -1) continue;//if the header doesn't conform, remove.
                var p = new HTTPRequestHeader()
                {
                    Name = singleHeader.Substring(0, splitter),
                    Value = singleHeader.Substring(splitter + 1)
                };
                ParsedHeaders.Add(p);
            }
            byte[] body = data.Skip(FindFirstOccurrence(data, Encoding.ASCII.GetBytes("\r\n\r\n")) + 4).ToArray();
            var line1 = headerArray.First().Split(' ');
            var URL = line1[1].Split('?')[0];
            var Parameters = line1[1][URL.Length..].TrimStart('?');
            var r = new HTTPRequest()
            {
                Method = line1[0],
                URL = URL,
                Parameters = Parameters,
                Headers = ParsedHeaders.ToArray(),
                Body = body
            };
            return r;
        }

        public static int FindFirstOccurrence(byte[] data, byte[] search)
        {
            int found = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == search[found])
                {
                    found++;
                    if (found >= search.Length)
                    {
                        return (i + 1) - found;
                    }
                }
                else
                {
                    found = 0;
                }
            }
            return -1;
        }

        public static void SendResponse(System.IO.Stream stream, byte[] payload, string PayloadType = "application/pairing+tlv8", SessionVariables.Encryption enc = null, bool isEvent = false, bool isMultiPoint = false)
        {
            var message = 
                payload==null ? 
                Encoding.ASCII.GetBytes($"HTTP/1.1 204 No Content\r\n\r\n").ToArray() :
                (isMultiPoint) ?
                Encoding.ASCII.GetBytes($"HTTP/1.1 207 Multi-Status\r\n\r\n").ToArray() :
                (isEvent) ?
                Encoding.ASCII.GetBytes($"EVENT/1.0 200 OK\r\nContent-Type: {PayloadType}\r\nContent-Length: {payload.Length}\r\n\r\n").Concat(payload).ToArray() :
                //is not empty response and is not event...
                Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: {PayloadType}\r\nConnection: keep-alive\r\nContent-Length: {payload.Length}\r\n\r\n").Concat(payload).ToArray();
            if (enc?.IsEncrypted == true)
            {
                var sent = 0;
                var toSend = message.Length;
                while (sent < toSend)
                {
                    ushort length = (ushort)(toSend - sent > 1024 ? 1024 : toSend - sent);

                    var transmittedLength = BitConverter.GetBytes(length);
                    var crypTex = ChaChaPolyManager.ChaCha20Poly1305_Encrypt(
                        Encoding.UTF8.GetBytes("\0\0\0\0").Concat(BitConverter.GetBytes(enc.WriteNonce++)).ToArray(),
                        message[sent..(sent + length)], 
                        transmittedLength, 
                        enc.AccessoryToControllerKey, 
                        out byte[] authData);
                    
                    stream.Write(transmittedLength.Concat(crypTex).Concat(authData).ToArray());
                    sent += length;

                }
                message = Array.Empty<byte>();
            }
            stream.Write(message, 0, message.Length);
        }
    }

    public record HTTPRequest
    {
        public string Method { get; init; }
        public string URL { get; init; }
        public string Parameters { get; set; }
        public HTTPRequestHeader[] Headers { get; set; }
        public byte[] Body { get; init; }
    }

    public record HTTPRequestHeader
    {
        public string Name { get; init; }
        public string Value { get; init; }
    }
}
