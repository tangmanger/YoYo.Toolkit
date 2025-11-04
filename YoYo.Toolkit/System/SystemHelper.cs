using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace YoYo.Toolkit.System
{
    public class SystemHelper
    {
        /// <summary>
        /// 硬盘id
        /// </summary>
        public static string HardDiskID { get; private set; } = "";
        /// <summary>
        /// 获取硬盘id
        /// </summary>
        /// <returns></returns>
        public static string InitSystemHardDiskID()
        {
            HardDiskID = string.Empty;
            try
            {
                char OSLetter = char.ToUpper(Environment.SystemDirectory[0]);
                foreach (ManagementObject Mo_Disk in new ManagementClass(@"Win32_Diskdrive").GetInstances())
                {
                    foreach (ManagementObject Mo_Partition in Mo_Disk.GetRelated("Win32_DiskPartition"))
                    {
                        foreach (ManagementObject Mbo_Logical in Mo_Partition.GetRelated("Win32_LogicalDisk"))
                        {
                            if (Mbo_Logical != null && Mbo_Logical["Name"] != null && OSLetter == char.ToUpper(Mbo_Logical["Name"].ToString().Trim()[0]))
                            {
                                HardDiskID = $"{Mo_Disk["SerialNumber"]?.ToString()?.Trim()}";
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message, ex);
            }
            return HardDiskID;
        }
        public static void Init()
        {
            InitSystemHardDiskID();
            GetServerId();
            ComputerName = Environment.MachineName;
        }
        public static string ServerId { get; private set; } = "";
        public static string? ComputerName { get; private set; }

        /// <summary>
        /// 获取主板id
        /// </summary>
        /// <returns></returns>
        public static void GetServerId()
        {
            foreach (ManagementObject Mo_Disk in new ManagementClass(@"Win32_BaseBoard").GetInstances())
            {

                foreach (var m in Mo_Disk.Properties)
                {
                    if (m.Name == "SerialNumber")
                    {
                        if (m != null && m.Value != null)
                            ServerId = m.Value.ToString();
                    }

                }
            }
        }
    }
}
