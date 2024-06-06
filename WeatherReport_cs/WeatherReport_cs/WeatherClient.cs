using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0044
#pragma warning disable IDE0059
#pragma warning disable CS0168

namespace WeatherReport_cs
{
    internal class WeatherClient
    {
        /// <summary>
        /// 单例模式，供外界使用的客户端示例
        /// 使用单例模式：为了在登录窗口和主窗口仅连接一次服务器并且所有操作都基于单一客户端实例
        /// </summary>
        public static WeatherClient instance;
        public static WeatherClient GetInstance()
        {
            if(instance == null)
            {
                instance = new WeatherClient();
            }
            return instance;
        }

        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        public  Queue<string> que = new Queue<string>();
        public WeatherClient() 
        {
            client = new TcpClient();
            try
            {
                client.Connect("127.0.0.1", 10721);
            }
            catch (Exception ex)
            {
                return;
            }
            NetworkStream netStream = client.GetStream();
            reader = new StreamReader(netStream, Encoding.UTF8);
            writer = new StreamWriter(netStream, Encoding.UTF8);
            // 单独的线程，负责接收消息
            Thread threadReceive = new Thread(ReceiveData)
            {
                IsBackground = true
            };
            threadReceive.Start();
        }
        /// <summary>
        /// 接收消息的线程
        /// 接收到的消息放到公共缓冲区队列中
        /// 外界从队列中取用
        /// 因为单一线程，所以无需考虑线程冲突问题
        /// </summary>
        private void ReceiveData()
        {
            while (true)
            {
                string receiveStr = null;
                try
                {
                    receiveStr = reader.ReadLine();
                    Console.WriteLine("client receive: " + receiveStr);
                }
                catch (Exception ex)
                {
                    break;
                }
                // 接收到的消息加入缓冲区队列
                if(receiveStr != null)
                    que.Enqueue(receiveStr);
            }
        }
        /// <summary>
        /// 发送消息函数
        /// 负责将消息发送给服务器
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
            try
            {
                writer.WriteLine(msg);
                writer.Flush();
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
