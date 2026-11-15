using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace YoYo.Toolkit.Net.Pings
{
    /// <summary>
    /// 连通性检测。
    /// <para>
    /// 注意：<see cref="PingReply.Options"/> 并不可靠——本机回环、IPv6 链路本地地址、
    /// 部分驱动/虚拟网卡只回 ICMP 报文而不带 IP 选项，此时它返回 <c>null</c>，
    /// 直接取 <c>reply.Options.Ttl</c> 会抛 <see cref="NullReferenceException"/>。
    /// 这里统一按「可能为 null」处理，取值失败就退化成占位符，不影响检测结论。
    /// </para>
    /// </summary>
    public class NetworkPing
    {
        /// <summary>默认超时（毫秒）。</summary>
        public const int DefaultTimeout = 3000;

        /// <summary>
        /// 探测单台主机。可达返回 <see cref="PingReply"/>，不可达返回 <c>null</c>（不抛异常）。
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或 IP 地址。</param>
        /// <param name="timeout">超时毫秒数，默认 <see cref="DefaultTimeout"/>。</param>
        public static async Task<PingReply?> PingHost(string hostNameOrAddress, int timeout = DefaultTimeout)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(hostNameOrAddress, timeout);

                // Options 可能为 null：本机回环 / 未带 IP 选项的应答
                var options = reply.Options;

                Console.WriteLine($"地址: {reply.Address}");
                Console.WriteLine($"状态: {reply.Status}");
                Console.WriteLine($"往返时间: {reply.RoundtripTime}ms");
                Console.WriteLine($"生存时间(TTL): {(options is null ? "—" : options.Ttl.ToString())}");
                Console.WriteLine($"是否不分段: {(options is null ? "—" : options.DontFragment.ToString())}");

                return reply.Status == IPStatus.Success ? reply : null;
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
