using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using Kakegurui.Core;
using Kakegurui.Net;
using Kakegurui.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Renci.SshNet;

namespace JabamiYumeko
{
    /// <summary>
    /// 系统监控
    /// </summary>
    public class MonitorTask : TaskObject
    {
        /// <summary>
        /// 连接通道
        /// </summary>
        private SocketChannel _channel;

        /// <summary>
        /// 协议收发
        /// </summary>
        private readonly ProtocolMaid _protocolMaid = new ProtocolMaid();

        /// <summary>
        /// 构造函数
        /// </summary>
        public MonitorTask()
            :base("monitor")
        {

        }

        /// <summary>
        /// 控制器服务
        /// </summary>
        /// <param name="args">套接字包</param>
        private void ControlService(ReceivedEventArgs args)
        {
            ControlService_Request config = new ControlService_Request();
            ByteFormatter.Deserialize(config, args.Buffer, ProtocolHead.HeadSize);
            Host host = null;
            using (MySqlConnection con = new MySqlConnection(AppConfig.Config.GetValue<string>("mysql")))
            {
                try
                {
                    con.Open();
                    MySqlCommand hostCmd = new MySqlCommand(
                        $"Select Ip,Port,UserName,Password,OS From t_oms_host where Ip='{config.Ip}'", con);
                    MySqlDataReader hostReader = hostCmd.ExecuteReader();
                    while (hostReader.Read())
                    {
                        string ip = hostReader[0].ToString();
                        ushort port = Convert.ToUInt16(hostReader[1]);
                        string userName = hostReader[2].ToString();
                        string password = hostReader[3].ToString();
                        int os = Convert.ToInt32(hostReader[4]);
                        if (Enum.IsDefined(typeof(OperationSystem), os))
                        {
                            host = new Host
                            {
                                Ip = ip,
                                Port = port,
                                UserName = userName,
                                Password = password,
                                OS = (OperationSystem)os
                            };
                        }
                        else
                        {
                            LogPool.Logger.LogInformation("unknown os {0} {1}", ip, os);
                        }
                    }
                    hostReader.Close();
                }
                catch (MySqlException ex)
                {
                    LogPool.Logger.LogInformation(ex, "mysql");
                }
            }

            if (host != null)
            {
                Performance performance;
                if (host.OS == OperationSystem.CentOS6)
                {
                    performance = new CentOS6();
                }
                else if (host.OS == OperationSystem.CentOS7)
                {
                    performance = new CentOS7();
                }
                else if (host.OS == OperationSystem.Windows2008)
                {
                    performance = new Windows2008();
                }
                else
                {
                    return;
                }

                using (SshClient client = new SshClient(
                    host.Ip, host.Port, host.UserName, host.Password))
                {
                    try
                    {
                        client.Connect();
                        ControlService_Response response = new ControlService_Response
                        {
                            Result = performance.ControlService(client, config)
                        };
                        args.Channel.Send(args.RemoteEndPoint, Protocol.Response(args.TimeStamp, response));
                    }
                    catch (SocketException ex)
                    {
                        LogPool.Logger.LogInformation(ex, "ssh error {0}:{1} {2} {3}", host.Ip, host.Port, host.UserName, host.Password);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }
        }

        protected override void ActionCore()
        {        
            //连接地址
            string serviceAdderss = AppConfig.Config.GetValue<string>("service");
            LogPool.Logger.LogInformation("service address={0}", serviceAdderss);
            string[] datas;
            if (serviceAdderss == null|| (datas= serviceAdderss.Split(":")).Length<2)
            {
                return;
            }
            if (IPAddress.TryParse(datas[0], out IPAddress ip1)
                &&int.TryParse(datas[1],out int p))
            {
                IPEndPoint serviceEndPoint = new IPEndPoint(ip1, p);
                _channel=_protocolMaid.AddConnectEndPoint(new ProtocolHandler(), serviceEndPoint);
                _channel.Where(args => args.ProtocolId == Convert.ToUInt16(ProtocolId.ControlService))
                    .Subscribe(ControlService);
            }

            _protocolMaid.Start();
            while (!IsCancelled())
            {
                using (MySQLContext context = MySQLContext.CreateContext())
                {
                    List<Host> hosts = context.Hosts.ToList();
                    hosts.AsParallel().ForAll(host =>
                    {
                        using (MySQLContext serviceContext = MySQLContext.CreateContext())
                        {
                            List<Service> services = serviceContext.Services.Where(h => h.Ip == host.Ip).ToList();
                            Performance performance;
                            if (host.OS == OperationSystem.CentOS6)
                            {
                                performance = new CentOS6();
                            }
                            else if (host.OS == OperationSystem.CentOS7)
                            {
                                performance = new CentOS7();
                            }
                            else if (host.OS == OperationSystem.Windows2008)
                            {
                                performance = new Windows2008();
                            }
                            else
                            {
                                return;
                            }

                            using (SshClient client = new SshClient(
                                host.Ip, host.Port, host.UserName, host.Password))
                            {
                                try
                                {
                                    client.Connect();

                                    Host hostSnapshot = performance.QueryHost(client);
                                    hostSnapshot.Ip = host.Ip;
                                    _channel?.Send(null, Protocol.Request(hostSnapshot).Item1);

                                    foreach (var service in services)
                                    {
                                        Service serviceSnapshot = performance.QueryService(client, service.Name);
                                        serviceSnapshot.Ip = service.Ip;
                                        serviceSnapshot.Name = service.Name;
                                        _channel?.Send(null, Protocol.Request(serviceSnapshot).Item1);
                                    }
                                }
                                catch (SocketException e)
                                {
                                    LogPool.Logger.LogInformation(e, "ssh error {0}:{1} {2} {3}", host.Ip, host.Port, host.UserName, host.Password);
                                }
                                finally
                                {
                                    client.Disconnect();
                                }
                            }
                        }
                    });
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            _protocolMaid.Stop();
        }
    }
}
