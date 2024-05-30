using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace WeatherServerdotNet
{
    enum Status
    {
        LOGIN = 123,
        SEARCH,
        LOGOUT,
        RESPONSE
    }
    class ClientHandler
    {
        public TcpClient m_sock { get; private set; }
        public StreamReader reader { get; private set; }
        public StreamWriter writer { get; private set; }

        public ClientHandler(TcpClient sock)
        {
            m_sock = sock;
            if (m_sock != null)
            {
                NetworkStream netStream = m_sock.GetStream();
                reader = new StreamReader(netStream, Encoding.UTF8);
                writer = new StreamWriter(netStream, Encoding.UTF8);
            }
        }

        public void SendTo(string msg)
        {
            Console.WriteLine("send to client: " + msg + "\n");
            try
            {
                writer.WriteLine(msg);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server send to {m_sock} msg: {msg} failed\n" +
                                  $"error message: {ex.Message}\n" +
                                  $"please retry later...");
            }
        }
    }

    internal class Server
    {
        public TcpListener m_server;
        public List<ClientHandler> m_clients = new List<ClientHandler>();

        //使用方式 jsonUrl = url + cityCode + key [+ extension]
        private string url = "http://restapi.amap.com/v3/weather/weatherInfo?city=";
        private string key = "&key=440d2191d8abbf736b3e24e18ec71488";
        private string extension = "&extensions=all";
        public void Start()
        {
            m_server = new TcpListener(IPAddress.Any, 10721);
            m_server.Start();
            Console.WriteLine($"Server start success on {IPAddress.Any}:10721");

            while (true)
            {
                TcpClient newClient = new TcpClient();
                try
                {
                    newClient = m_server.AcceptTcpClient();
                    Console.WriteLine("a client accept success...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"accept error：{ex.Message}");
                    break;
                }
                ClientHandler client = new ClientHandler(newClient);
                m_clients.Add(client);
                Thread recvThread = new Thread(HandleClient);
                recvThread.Start(client);
            }
        }
        public void HandleClient(object obj)
        {
            ClientHandler client = (ClientHandler)obj;
            while (true)
            {
                string msg = null;
                try
                {
                    msg = client.reader.ReadLine();
                    Console.WriteLine("received：" + msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"receive msg failed：{ex.Message}");
                    break;
                }
                if (msg != null)
                {
                    string[] strs = msg.Split('+');
                    ParseMessage(client, (Status)Enum.Parse(typeof(Status), strs[0]), strs[1]);
                }
                else
                {
                    Console.WriteLine("server received error...");
                }

            }
            m_clients.Remove(client);
            client.m_sock.Close();
        }

        private void ParseMessage(ClientHandler client, Status stat, string msg)
        {
            switch (stat)
            {
                case Status.LOGIN:
                    Handle_Login(client, msg);
                    break;
                case Status.SEARCH:
                    Handle_Search(client, msg);
                    break;
                case Status.LOGOUT:
                    Handle_Logout(client, msg);
                    break;
            }
        }
        /// <summary>
        /// 处理登录逻辑，用户发来的信息里面会包含用户名和密码
        /// 去数据库里面搜索，验证，返回验证信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void Handle_Login(ClientHandler client, string msg)
        {
            // msg格式：用户名,密码
            string[] strs = msg.Split(',');
            string username = strs[0];
            string password = strs[1];
            //以用户名为条件，在数据库中查找密码
            using (var c = new UserInfoEntities())
            {
                try
                {
                    var query = from u in c.UserManager
                                where u.name == username
                                select u;

                    //查询结束后比对
                    var user = query.FirstOrDefault();
                    if (user != null)
                    {
                        if (password.Equals(user.pwd))
                        {
                            client.SendTo("RESPONSE+success");
                        }
                    }
                    client.SendTo("RESPONSE+error");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("search database error...\n" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 查询天气信息功能，用户发来的是个城市代码指令
        /// 拿到指令之后发http请求，获取返回数据是个json
        /// 再把这个json发送回去给用户客户端
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void Handle_Search(ClientHandler client, string msg)
        {
            // 这里的msg拿到的是cityCode城市代码
            _ = getWeatherInfo(client, true, msg);
            _ = getWeatherInfo(client, false, msg);
        }
        //获取网络信息
        private async Task getWeatherInfo(ClientHandler client, bool isToday, string code)
        {
            //测试用url：http://restapi.amap.com/v3/weather/weatherInfo?city=410100&key=440d2191d8abbf736b3e24e18ec71488
            string jsonUrl = url + code + key;
            if (!isToday)
                jsonUrl += extension;

            //使用 HttpClient 类获取网络请求 
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    //发送 GET 请求并获取响应
                    HttpResponseMessage response = await httpClient.GetAsync(jsonUrl);

                    //确保请求成功
                    response.EnsureSuccessStatusCode();

                    //读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();

                    //解析Json数据
                    if (isToday)  //当天
                        client.SendTo("RESPONSE+today+" + responseBody);
                    else          //预报
                        client.SendTo("RESPONSE+forecast+" + responseBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("未查询天气信息，或许是网络问题\n错误信息:" + ex.Message);
                }
            }
        }
        private void Handle_Logout(ClientHandler client, string msg)
        {
            Console.WriteLine($"{client.m_sock} exit.");
            m_clients.Remove(client);
            client.m_sock.Close();
        }
    }
}
