using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.Net.Pings
{
    public class NetworkPing
    {
        public static async Task<PingReply?> PingHost(string hostNameOrAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(hostNameOrAddress);

                    if (reply.Status == IPStatus.Success)
                    {
                        Console.WriteLine($"地址: {reply.Address}");
                        Console.WriteLine($"往返时间: {reply.RoundtripTime}ms");
                        Console.WriteLine($"生存时间(TTL): {reply.Options.Ttl}");
                        Console.WriteLine($"是否不分段: {reply.Options.DontFragment}");

                        return reply;
                    }
                    else
                    {
                        Console.WriteLine($"Ping 失败: {reply.Status}");
                        return null; // 表示失败
                    }
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"Ping 异常: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"一般异常: {ex.Message}");
                return null;
            }
        }
    }
}
