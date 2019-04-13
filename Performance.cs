using Renci.SshNet;

namespace JabamiYumeko
{
    /// <summary>
    /// 系统检测线程
    /// </summary>
    public abstract class Performance
    {
        /// <summary>
        /// 命令执行成功
        /// </summary>
        public const string Success="success";
     
        /// <summary>
        /// 未知ip
        /// </summary>
        public const string UnknownIp= "unknown ip";
    
        /// <summary>
        /// 未知系统
        /// </summary>
        public const string UnknownOS="unknown os";
     
        /// <summary>
        /// 未知操作
        /// </summary>
        public const string UnknownOP="unknown op";

        /// <summary>
        /// 控制服务
        /// </summary>
        /// <param name="client">ssh</param>
        /// <param name="request">服务控制参数</param>
        /// <returns>操作结果</returns>
        public abstract string ControlService(SshClient client,ControlService_Request request);

        /// <summary>
        /// 供子类实现的获取主机快照函数
        /// </summary>
        /// <param name="client">ssh</param>
        /// <returns>用于设置主机快照信息的实例</returns>
        public abstract Host QueryHost(SshClient client);

        /// <summary>
        /// 供子类实现的获取服务快照函数
        /// </summary>
        /// <param name="client">ssh</param>
        /// <param name="serviceName">服务名</param>
        /// <returns>用于设置服务快照信息的实例</returns>
        public abstract Service QueryService(SshClient client, string serviceName);

    }
}
