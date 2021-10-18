using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace NvrOrchestrator.Bonjour
{
    public class BonjourServer
    {
        private static Socket mcastSocket;
        private static readonly IPAddress multiCastIP = new(0xFB0000E0);//224.0.0.251
        private static readonly EndPoint remoteEndPoint = new IPEndPoint(multiCastIP, 5353);
        private static EndPoint AnyRemoteAddress = new IPEndPoint(IPAddress.Any, 0);
        private static readonly EndPoint AnyRemoteBonjourAddress = new IPEndPoint(IPAddress.Any, 5353);

        private static readonly List<BonjourPoco> AuthoritativeServices = new();
        public static void AddServices(BonjourPoco p)
        {
            AuthoritativeServices.Add(p);
        }

        public static Task ReceiveBroadcaseMessages()
        {
            mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            mcastSocket.Bind(AnyRemoteBonjourAddress);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multiCastIP));

            byte[] packet = new byte[1500];
            while(true)
            {
                mcastSocket.ReceiveFrom(packet, ref AnyRemoteAddress);
                var Questions = BonjourManager.DecodePacket(packet);
                var RelevantQuestions = Questions.Where(q => q.Category == mDNSResourceCategory.Question && 
                        ((q.ResourceType != mDNSResourceTypes.PTR && AuthoritativeServices.Where(a => a.ARecord == q.Name).Any()) ||
                        (q.ResourceType == mDNSResourceTypes.PTR && AuthoritativeServices.Where(a => a.PTRRecord == q.Name).Any())));
                List<mDNSResourcePoco> responses = new();
                foreach (var question in RelevantQuestions)
                {
                    switch (question.ResourceType)
                    {
                        case mDNSResourceTypes.PTR:
                            {
                                var authService = AuthoritativeServices.Where(a => a.PTRRecord == question.Name);
                                if (authService.Any())
                                {
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.Answer,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = question.Name,
                                        ResourceType = mDNSResourceTypes.PTR,
                                        TTL = 1200,
                                        PTR_DomainNamePointer = authService.First().ARecord
                                    });
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.AdditionalInfo,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = authService.First().ARecord,
                                        ResourceType = mDNSResourceTypes.TXT,
                                        TTL = 1200,
                                        TXT = authService.First().TXTRecords.ToList()
                                    });
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.AdditionalInfo,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = authService.First().ARecord,
                                        ResourceType = mDNSResourceTypes.SRV,
                                        TTL = 1200,
                                        SRV_Port = authService.First().Port,
                                        SRV_Priority = 0,
                                        SRV_Weight = 0,
                                        SRV_Target = authService.First().ARecord
                                    });
                                    foreach (var item in authService.First().ActiveListeners)
                                    {
                                        if (item.AddressFamily == AddressFamily.InterNetwork)
                                        {
                                            responses.Add(new()
                                            {
                                                Category = mDNSResourceCategory.AdditionalInfo,
                                                ResourceClass = mDNSResourceClass.Internet,
                                                Name = authService.First().ARecord,
                                                ResourceType = mDNSResourceTypes.A,
                                                TTL = 1200,
                                                A_IPAddress = item
                                            });
                                        }
                                        else
                                        {
                                            responses.Add(new()
                                            {
                                                Category = mDNSResourceCategory.AdditionalInfo,
                                                ResourceClass = mDNSResourceClass.Internet,
                                                Name = authService.First().ARecord,
                                                ResourceType = mDNSResourceTypes.AAAA,
                                                TTL = 1200,
                                                A_IPAddress = item
                                            });
                                        }
                                    }

                                }
                                else
                                {
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.Answer,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = question.Name,
                                        ResourceType = mDNSResourceTypes.NSEC,
                                        TTL = 1200,
                                        NSEC_NXTDomainName = question.NSEC_NXTDomainName,
                                        NSEC_Error = -1
                                    });
                                }
                            }
                            break;
                        case mDNSResourceTypes.TXT:
                            {
                                var authService = AuthoritativeServices.Where(a => a.ARecord == question.Name);
                                if (authService.Any())
                                {
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.Answer,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = question.Name,
                                        ResourceType = mDNSResourceTypes.TXT,
                                        TTL = 1200,
                                        TXT = authService.First().TXTRecords.ToList()
                                    });
                                }
                                else
                                {
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.Answer,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = question.Name,
                                        ResourceType = mDNSResourceTypes.NSEC,
                                        TTL = 1200,
                                        NSEC_NXTDomainName = question.NSEC_NXTDomainName,
                                        NSEC_Error = -1
                                    });
                                }
                            }
                            break;
                        case mDNSResourceTypes.SRV:
                            {
                                var authService = AuthoritativeServices.Where(a => a.ARecord == question.Name);
                                if (authService.Any())
                                {
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.Answer,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = question.Name,
                                        ResourceType = mDNSResourceTypes.SRV,
                                        TTL = 1200,
                                        SRV_Port = authService.First().Port,
                                        SRV_Priority = 0,
                                        SRV_Weight = 0,
                                        SRV_Target = authService.First().ARecord
                                    });
                                    foreach (var item in authService.First().ActiveListeners)
                                    {
                                        if (item.AddressFamily == AddressFamily.InterNetwork)
                                        {
                                            responses.Add(new()
                                            {
                                                Category = mDNSResourceCategory.AdditionalInfo,
                                                ResourceClass = mDNSResourceClass.Internet,
                                                Name = question.Name,
                                                ResourceType = mDNSResourceTypes.A,
                                                TTL = 1200,
                                                A_IPAddress = item
                                            });
                                        }
                                        else
                                        {
                                            responses.Add(new()
                                            {
                                                Category = mDNSResourceCategory.AdditionalInfo,
                                                ResourceClass = mDNSResourceClass.Internet,
                                                Name = question.Name,
                                                ResourceType = mDNSResourceTypes.AAAA,
                                                TTL = 1200,
                                                A_IPAddress = item
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    responses.Add(new()
                                    {
                                        Category = mDNSResourceCategory.Answer,
                                        ResourceClass = mDNSResourceClass.Internet,
                                        Name = question.Name,
                                        ResourceType = mDNSResourceTypes.NSEC,
                                        TTL = 1200,
                                        NSEC_NXTDomainName = question.NSEC_NXTDomainName,
                                        NSEC_Error = -1
                                    });
                                }
                            }
                            break;
                        default: break;
                    }
                }
                if (responses.Count!=0)
                {
                    mcastSocket.SendTo(BonjourManager.EncodePacket(responses), remoteEndPoint);
                }
            }
        }
    }
}
