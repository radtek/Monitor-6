using System;
using System.ComponentModel.DataAnnotations.Schema;
using Kakegurui.Core;
using Kakegurui.Protocol;

namespace JabamiYumeko
{
    /// <summary>
    /// 控制服务参数
    /// </summary>
    public enum ServiceConfig : byte
    {
        Start = 0,
        Stop = 1,
        Restart = 2,
        Auto = 3,
        Demand = 4
    };

    /// <summary>
    /// 服务状态
    /// </summary>
    public enum ServiceStatus : byte
    {
        Unknown = 0,
        Running = 1,
        Stopped = 2
    };

    /// <summary>
    /// 服务
    /// </summary>
    [Table("t_oms_service")]
    public class Service : Protocol
    {
        public override ushort Id => Convert.ToUInt16(ProtocolId.NoticeService);

        /// <summary>
        /// 服务主机地址
        /// </summary>
        [SerializeIndex(1)]
        public string Ip { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        [SerializeIndex(2)]
        public string Name { get; set; }

        /// <summary>
        /// 当前查询的时间戳
        /// </summary>
        [NotMapped]
        [SerializeIndex(3)]
        public long TimeStamp { get; set; }
        /// <summary>
        /// 服务状态
        /// </summary>
        [NotMapped]
        [SerializeIndex(4)]
        public byte Status { get; set; }
        /// <summary>
        /// 服务参数
        /// </summary>
        [NotMapped]
        [SerializeIndex(5)]
        public byte Config { get; set; }
        /// <summary>
        /// 服务进程号
        /// </summary>
        [NotMapped]
        [SerializeIndex(6)]
        public int Pid { get; set; }
        /// <summary>
        /// 线程数
        /// </summary>
        [NotMapped]
        [SerializeIndex(7)]
        public short ThreadCount { get; set; }
        /// <summary>
        /// cpu使用率
        /// </summary>
        [NotMapped]
        [SerializeIndex(8)]
        public float CPU_Used { get; set; }
        /// <summary>
        /// 虚拟内存峰值(KB)
        /// </summary>
        [NotMapped]
        [SerializeIndex(9)]
        public uint Vm_Peak { get; set; }
        /// <summary>
        /// 虚拟内存(KB)
        /// </summary>
        [NotMapped]
        [SerializeIndex(10)]
        public uint Vm_Used { get; set; }
        /// <summary>
        /// 物理内存峰值(KB)
        /// </summary>
        [NotMapped]
        [SerializeIndex(11)]
        public uint Mem_Peak { get; set; }
        /// <summary>
        /// 物理内存(KB)
        /// </summary>
        [NotMapped]
        [SerializeIndex(12)]
        public uint Mem_Used { get; set; }
        /// <summary>
        /// 磁盘写入kb/s
        /// </summary>
        [NotMapped]
        [SerializeIndex(13)]
        public uint Disk_Write { get; set; }
        /// <summary>
        /// 磁盘读取kb/s
        /// </summary>
        [NotMapped]
        [SerializeIndex(14)]
        public uint Disk_Read { get; set; }
        /// <summary>
        /// 网络发送(kb/s)
        /// </summary>
        [NotMapped]
        [SerializeIndex(15)]
        public uint Network_Transmit { get; set; }
        /// <summary>
        /// 网络接收(kb/s)
        /// </summary>
        [NotMapped]
        [SerializeIndex(16)]
        public uint Network_Receive { get; set; }
    }

    /// <summary>
    /// 控制服务请求
    /// </summary>
    public class ControlService_Request:Protocol
    {
        public override ushort Id => Convert.ToUInt16(ProtocolId.ControlService);
        [SerializeIndex(1)]
        public string Ip { get; set; }
        [SerializeIndex(2)]
        public string Name { get; set; }
        [SerializeIndex(3)]
        public byte Op { get; set; }
    }

    /// <summary>
    /// 控制服务响应
    /// </summary>
    public class ControlService_Response : Protocol
    {
        public override ushort Id => Convert.ToUInt16(ProtocolId.ControlService+1);
        [SerializeIndex(1)]
        public string Result { get; set; }
    }
}
