using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Win32;
//using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace WS_Core.Tools
{
    /// <summary>
    /// 字节计量单位转换
    /// </summary>
    [Author("Linyee", "2019-02-01")]
    public static class CpuCount_Ex
    {
        /// <summary>
        /// 数值转计算机单位数值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToGMKB(this long value)
        {
            float val = value;
            string valutil = "BS";
            if (value >= 1024L*1024*1024*1024)//TB
            {
                val = val / (1024L * 1024 * 1024 * 1024);
                valutil = "TB";
            }
            else
            if (value >= 1024L * 1024 * 1024 )//GB
            {
                val = val / (1024L * 1024 * 1024);
                valutil = "GB";
            }
            else
            if (value >= 1024L * 1024 )//MB
            {
                val = val / (1024L * 1024);
                valutil = "MB";
            }
            else
            if (value >= 1024L )//KB
            {
                val = val / 1024L ;
                valutil = "KB";
            }

            return val.ToString("#0.00") + valutil;
        }
    }

    /// <summary>
    /// Cpu计数助手
    /// Linyee 2018-10-11
    /// </summary>
    public class CpuCount_Helper
    {
        private static long TotalMemory = 0;

        static CpuCount_Helper()
        {
            Process[] p = Process.GetProcesses();//获取进程信息
        }

        /// <summary>
        /// CPU个数
        /// </summary>
        private static readonly int m_ProcessorCount = Environment.ProcessorCount;

        /// <summary>
        /// 获取当前CPU值
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentCpuCount()
        {
            var proc = Process.GetCurrentProcess();
            var cpu = proc.TotalProcessorTime.Milliseconds;
            return cpu;
        }

        /// <summary>
        /// 获取当前进程名称
        /// </summary>
        /// <returns></returns>
        public static string GetProcessName()
        {
            var proc = Process.GetCurrentProcess();
            return proc.ProcessName;
        }

        /// <summary>
        /// 获取当前内存值
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentMem()
        {
            var proc = Process.GetCurrentProcess();
            return proc.WorkingSet64;
        }

        /// <summary>
        /// 获取当前内存比
        /// </summary>
        /// <returns></returns>
        public static double GetCurrentMemCount()
        {
            var proc = Process.GetCurrentProcess();
            var mem = proc.WorkingSet64;
            //Console.WriteLine("{0}",res.ToString("p"));
            Process[] p = Process.GetProcesses();//获取进程信息
            Int64 totalMem = 0;
            foreach (Process pr in p)
            {
                totalMem += pr.WorkingSet64;
            }
            TotalMemory = totalMem;

            double total = 0;
            foreach (Process pr in p)
            {
                var pp = (double)pr.WorkingSet64 / TotalMemory;
                total += pp;
                //if (pp >= 0.01)
                //{
                //    Console.WriteLine("{0}\t{1}\t{2}", pr.ProcessName, pr.WorkingSet64.ToGMKB(), pp.ToString("p2"));
                //}
            }

            //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "总计", TotalMemory.ToGMKB(), total.ToString("p2"), MySystem.get_StorageInfo().ToJsonString(), MySystem.get_utilization_rate());
            //foreach(string key in Environment.GetEnvironmentVariables().Keys)
            //{
            //    Console.WriteLine("{0}\t{1}", key, Environment.GetEnvironmentVariable(key));
            //}
            var res= (double)mem / TotalMemory;
            return res;
        }


        /// <summary>
        /// 获取当前内存使用率
        /// </summary>
        /// <returns></returns>
        public static uint GetCurMemUseRate()
        {
            return MySystem.get_StorageInfo().dwMemoryLoad;
        } 
    }


    /// <summary>
    /// 
    /// </summary>
    public class MySystem
    {
        /// <summary>
        /// 
        /// </summary>
        [DllImport("kernel32")]
        public static extern void GetWindowsDirectory(StringBuilder WinDir, int count);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("kernel32")]
        public static extern void GetSystemDirectory(StringBuilder SysDir, int count);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("kernel32")]
        private static extern void GlobalMemoryStatus(ref StorageInfo memibfo);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("kernel32")]
        private static extern void GlobalMemoryStatusEx(ref StorageInfo64 memibfo);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPUInfo cpuinfo);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("kernel32")]
        public static extern void GetSystemTime(ref SystemTimeInfo stinfo);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("Iphlpapi.dll")]
        public static extern int SendARP(Int32 DestIP, Int32 SrcIP, ref Int64 MacAddr, ref Int32 PhyAddrLen);
        /// <summary>
        /// 
        /// </summary>
        [DllImport("Ws2_32.dll")]
        public static extern Int32 inet_addr(string ipaddr);
        //内存信息结构体
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct StorageInfo //此处全是以字节为单位
        {
            /// <summary>
            /// 
            /// </summary>
            public uint dwLength;//长度
                                 /// <summary>
                                 /// 
                                 /// </summary>
            public uint dwMemoryLoad;//内存使用率
                                     /// <summary>
                                     /// 
                                     /// </summary>
            public uint dwTotalPhys;//总物理内存
                                    /// <summary>
                                    /// 
                                    /// </summary>
            public uint dwAvailPhys;//可用物理内存
                                    /// <summary>
                                    /// 
                                    /// </summary>
            public uint dwTotalPageFile;//交换文件总大小
                                        /// <summary>
                                        /// 
                                        /// </summary>
            public uint dwAvailPageFile;//可用交换文件大小
                                        /// <summary>
                                        /// 
                                        /// </summary>
            public uint dwTotalVirtual;//总虚拟内存
                                       /// <summary>
                                       /// 
                                       /// </summary>
            public uint dwAvailVirtual;//可用虚拟内存大小
        }
        //内存信息结构体
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct StorageInfo64 //此处全是以字节为单位
        {
            /// <summary>
            /// 
            /// </summary>
            public uint dwLength;//长度
                                 /// <summary>
                                 /// 
                                 /// </summary>
            public uint dwMemoryLoad;//内存使用率
                                     /// <summary>
                                     /// 
                                     /// </summary>
            public ulong dwTotalPhys;//总物理内存
                                     /// <summary>
                                     /// 
                                     /// </summary>
            public ulong dwAvailPhys;//可用物理内存
                                     /// <summary>
                                     /// 
                                     /// </summary>
            public ulong dwTotalPageFile;//交换文件总大小
                                         /// <summary>
                                         /// 
                                         /// </summary>
            public ulong dwAvailPageFile;//可用交换文件大小
                                         /// <summary>
                                         /// 
                                         /// </summary>
            public ulong dwTotalVirtual;//总虚拟内存
                                        /// <summary>
                                        /// 
                                        /// </summary>
            public ulong dwAvailVirtual;//可用虚拟内存大小
                                        /// <summary>
                                        /// 
                                        /// </summary>
            public ulong ullAvailExtendedVirtual;
        }
        //cpu信息结构体
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CPUInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public uint cpu的OemId;
            /// <summary>
            /// 
            /// </summary>
            public uint cpu页面大小;
            /// <summary>
            /// 
            /// </summary>
            public uint lpMinimumApplicationAddress;
            /// <summary>
            /// 
            /// </summary>
            public uint lpMaximumApplicationAddress;
            /// <summary>
            /// 
            /// </summary>
            public uint dwActiveProcessorMask;
            /// <summary>
            /// 
            /// </summary>
            public uint cpu个数;
            /// <summary>
            /// 
            /// </summary>
            public uint cpu类别;
            /// <summary>
            /// 
            /// </summary>
            public uint dwAllocationGranularity;
            /// <summary>
            /// 
            /// </summary>
            public uint cpu等级;
            /// <summary>
            /// 
            /// </summary>
            public uint cpu修正;
        }
        //系统时间信息结构体
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTimeInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public ushort wYear;
            /// <summary>
            /// 
            /// </summary>
            public ushort wMonth;
            /// <summary>
            /// 
            /// </summary>
            public ushort wDayOfWeek;
            /// <summary>
            /// 
            /// </summary>
            public ushort wDay;
            /// <summary>
            /// 
            /// </summary>
            public ushort wHour;
            /// <summary>
            /// 
            /// </summary>
            public ushort wMinute;
            /// <summary>
            /// 
            /// </summary>
            public ushort wSecond;
            /// <summary>
            /// 
            /// </summary>
            /// <summary>
            /// 
            /// </summary>
            public ushort wMilliseconds;
        }
        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns></returns>
        public static StorageInfo64 get_StorageInfo()
        {
            StorageInfo64 memInfor = new StorageInfo64();
            memInfor.dwLength = (uint)Marshal.SizeOf(typeof(StorageInfo64));
            GlobalMemoryStatusEx(ref memInfor);
            return memInfor;
        }
        //获取cpu信息
        /// <summary>
        /// 
        /// </summary>
        public static CPUInfo get_CPUInfo()
        {
            CPUInfo memInfor = new CPUInfo();
            GetSystemInfo(ref memInfor);
            return memInfor;
        }
        //获取系统时间信息
        /// <summary>
        /// 
        /// </summary>
        public static SystemTimeInfo get_SystemTimeInfo()
        {
            SystemTimeInfo memInfor = new SystemTimeInfo();
            GetSystemTime(ref memInfor);
            return memInfor;
        }
        /// <summary>
        /// 获取内存利用率函数
        /// </summary>
        /// <returns></returns>
        public static string get_utilization_rate()
        {
            StorageInfo64 memInfor = new StorageInfo64();
            memInfor.dwLength = (uint)Marshal.SizeOf(typeof(StorageInfo64));
            GlobalMemoryStatusEx(ref memInfor);
            return memInfor.dwMemoryLoad.ToString("0.0");
        }
        //获取系统路径
        /// <summary>
        /// 
        /// </summary>
        public static string get_system_path()
        {
            const int nChars = 128;
            StringBuilder Buff = new StringBuilder(nChars);
            GetSystemDirectory(Buff, nChars);
            return Buff.ToString();
        }
        //获取window路径
        /// <summary>
        /// 
        /// </summary>
        public static string get_window_path()
        {
            const int nChars = 128;
            StringBuilder Buff = new StringBuilder(nChars);
            GetWindowsDirectory(Buff, nChars);
            return Buff.ToString();
        }

        ////获取cpu的id号
        //public static string get_CPUID()
        //{
        //    try
        //    {
        //        string cpuInfo = "";//cpu序列号 
        //        ManagementClass mc = new ManagementClass("Win32_Processor");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        foreach (ManagementObject mo in moc)
        //        {
        //            cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
        //        }
        //        moc = null;
        //        mc = null;
        //        return cpuInfo;
        //    }
        //    catch
        //    {
        //        return "unknow";
        //    }
        //}
        ////获取设备硬件卷号
        //public static string get_Disk_VolumeSerialNumber()
        //{
        //    ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
        //    ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"d:\"");
        //    disk.Get();
        //    return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        //}
        ////获取mac地址
        //public static string get_mac_address()
        //{
        //    string mac = "";
        //    ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
        //    ManagementObjectCollection moc = mc.GetInstances();
        //    foreach (ManagementObject mo in moc)
        //    {
        //        if ((bool)mo["IPEnabled"] == true)
        //        {
        //            mac = mo["MacAddress"].ToString();
        //        }
        //        mo.Dispose();
        //    }
        //    return mac;
        //}
        //根据ip获取邻节点MAC地址
        /// <summary>
        /// 
        /// </summary>
        public static string get_remote_mac(string ip)
        {
            StringBuilder mac = new StringBuilder();
            try
            {
                Int32 remote = inet_addr(ip);
                Int64 macinfo = new Int64();
                Int32 length = 6;
                SendARP(remote, 0, ref macinfo, ref length);
                string temp = Convert.ToString(macinfo, 16).PadLeft(12, '0').ToUpper();
                int x = 12;
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                        mac.Append(temp.Substring(x - 2, 2));
                    else
                        mac.Append(temp.Substring(x - 2, 2) + "-");
                    x -= 2;
                }
                return mac.ToString();
            }
            catch
            {
                return mac.ToString();
            }
        }
        ////获取本机的ip地址
        //public static string get_ip()
        //{
        //    try
        //    {
        //        string st = "";
        //        ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        foreach (ManagementObject mo in moc)
        //        {
        //            if ((bool)mo["IPEnabled"] == true)
        //            {
        //                //st=mo["IpAddress"].ToString(); 
        //                System.Array ar;
        //                ar = (System.Array)(mo.Properties["IpAddress"].Value);
        //                st = ar.GetValue(0).ToString();
        //                break;
        //            }
        //        }
        //        moc = null;
        //        mc = null;
        //        return st;
        //    }
        //    catch
        //    {
        //        return "unknow";
        //    }
        //}
        ////获取硬盘id号
        //public static string get_disk_id()
        //{
        //    try
        //    {
        //        String HDid = "";
        //        ManagementClass mc = new ManagementClass("Win32_DiskDrive");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        foreach (ManagementObject mo in moc)
        //        {
        //            HDid = (string)mo.Properties["Model"].Value;
        //        }
        //        moc = null;
        //        mc = null;
        //        return HDid;
        //    }
        //    catch
        //    {
        //        return "unknow";
        //    }
        //}
        ////获得系统登陆用户名
        //public static string get_user()
        //{
        //    try
        //    {
        //        string st = "";
        //        ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        foreach (ManagementObject mo in moc)
        //        {
        //            st = mo["UserName"].ToString();
        //        }
        //        moc = null;
        //        mc = null;
        //        return st;
        //    }
        //    catch
        //    {
        //        return "unknow";
        //    }
        //}
        ////获得系统类型
        //public static string get_SystemType()
        //{
        //    try
        //    {
        //        string st = "";
        //        ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        foreach (ManagementObject mo in moc)
        //        {
        //            st = mo["SystemType"].ToString();
        //        }
        //        moc = null;
        //        mc = null;
        //        return st;
        //    }
        //    catch
        //    {
        //        return "unknow";
        //    }
        //}
        ////获得物理总内存
        //public static string get_TotalPhysicalMemory()
        //{
        //    try
        //    {
        //        string st = "";
        //        ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        foreach (ManagementObject mo in moc)
        //        {
        //            st = mo["TotalPhysicalMemory"].ToString();
        //        }
        //        moc = null;
        //        mc = null;
        //        return st;
        //    }
        //    catch
        //    {
        //        return "unknow";
        //    }
        //}
        //获得电脑名称
        /// <summary>
        /// 
        /// </summary>
        public static string get_ComputerName()
        {
            try
            {
                return System.Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow";
            }
        }
        ////
        //public static float 性能显示状况2(string CategoryName, string CounterName)
        //{
        //    PerformanceCounter pc = new PerformanceCounter(CategoryName, CounterName);
        //    Thread.Sleep(500);
        //    float xingneng = pc.NextValue();
        //    return xingneng;
        //}

        //public static float 性能显示状况3(string CategoryName, string CounterName, string InstanceName)
        //{
        //    PerformanceCounter pc = new PerformanceCounter(CategoryName, CounterName, InstanceName);
        //    Thread.Sleep(500); // wait for 1 second 
        //    float xingneng = pc.NextValue();
        //    return xingneng;
        //}
        //获取os版本信息
        /// <summary>
        /// 
        /// </summary>
        public static string get_OSVersion()
        {
            System.OperatingSystem version = System.Environment.OSVersion;
            switch (version.Platform)
            {
                case System.PlatformID.Win32Windows:
                    switch (version.Version.Minor)
                    {
                        case 0:
                            return "Windows 95";
                        case 10:
                            if (version.Version.Revision.ToString() == "2222A")
                                return "Windows 98 Second Edition";
                            else
                                return "Windows 98";
                        case 90:
                            return "Windows Me";
                    }
                    break;
                case System.PlatformID.Win32NT:
                    switch (version.Version.Major)
                    {
                        case 3:
                            return "Windows NT 3.51";
                        case 4:
                            return "Windows NT 4.0";
                        case 5:
                            if (version.Version.Minor == 0)
                                return "Windows 2000";
                            else
                                return "Windows XP";
                        case 6:
                            return "Windows 8";
                    }
                    break;
            }
            return "发现失败";

        }
    }
}
