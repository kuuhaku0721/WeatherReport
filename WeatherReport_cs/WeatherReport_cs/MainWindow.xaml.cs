using ConsoleApp1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.PeerToPeer.Collaboration;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WeatherReport_cs
{
    struct Today
    {
        public string province;   //省份
        public string city;       //城市
        public string type;       //天气类型
        public string tempature;  //温度
        public string fx;         //风向
        public string fl;         //风力
        public string humidity;   //湿度
        public string date;       //日期
        public string quality;    //空气质量  没有  设置默认值
    };
    struct Forecast
    {
        public string week;         //周几
        public string date;         //日期
        public string power;        //风力
        public string daytype;      //天气状况
        public string nighttype;    //天气状况
        public string high;         //最高气温
        public string low;          //最低气温
    };
    enum Status
    {
        // 消息格式：command+msg
        // 无加密，无格式控制，无帧标识，无帧定界，无序列化
        LOGIN = 123,
        SEARCH,
        LOGOUT,
        RESPONSE
    }

    public partial class MainWindow : Window
    {
        // 网络请求用的一些内容
        private string m_cityName;  //城市名
        private string m_cityCode = "410100";  //城市代码  410100为郑州
        private WeatherClient m_weatherClient;
        private List<string> citys;
        //保存当天及预报信息的结构体
        private Today today;
        private Forecast[] forecast = new Forecast[4];
        private string sunset = " 19:00:00";  //默认日落时间，用来判断属于白天还是晚上

        //用于鼠标控制移动窗口
        private bool isMousePressed;
        private Point mousePos;
        private string currentPath;

        //工具类，负责从文件里面读取城市代码出来
        WeatherTool weatherTool = new WeatherTool();
        bool bTableExist = false;
        private List<string> m_tableName;

        //一些图片定义，用来实现背景替换功能
        private Timer timer;
        private string bkground_uri;
        private string uri_weaUI = "Images/weaUI.png";
        private string uri_miya = "Images/米娅.png";
        private BitmapImage img_weaUI = new BitmapImage(new Uri("Images/weaUI.png", UriKind.Relative));
        private BitmapImage img_miya = new BitmapImage(new Uri("Images/米娅.png", UriKind.Relative));
        // 白天 晚上，傍晚 TODO 如果某些天气类型也需要，可以在后面追加，如果数量校多，请换用其他存储形式如map
        private BitmapImage img_Sunrise = new BitmapImage(new Uri("Images/米娅.png", UriKind.Relative));
        private BitmapImage img_Sunset = new BitmapImage(new Uri("Images/米娅.png", UriKind.Relative));
        private BitmapImage img_Night = new BitmapImage(new Uri("Images/米娅.png", UriKind.Relative));
        private string uri_Sunrise = "Images/米娅.png";
        private string uri_Sunset = "Images/米娅.png";
        private string uri_Night = "Images/米娅.png";

        //保存窗口内所有的label控件，用于控制窗口显示
        List<Label> lst_week = new List<Label>();
        List<Label> lst_date = new List<Label>();
        List<Label> lst_power = new List<Label>();
        List<Label> lst_type = new List<Label>();
        List<Label> lst_high = new List<Label>();
        List<Label> lst_low = new List<Label>();
        List<Label> lst_image = new List<Label>();
        List<Label> list_change = new List<Label>();

        public MainWindow()
        {
            InitializeComponent();

            //先登录
            LoginWindow loginProc = new LoginWindow();
            loginProc.ShowDialog();

            // 获取客户端(单例获取，这里已不需要连接)
            m_weatherClient = WeatherClient.GetInstance();

            //设置窗口背景
            Image_Loaded();

            //设置窗口属性
            this.WindowStyle = WindowStyle.None;   //无边框
            this.ResizeMode = ResizeMode.NoResize; //不允许改变大小
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //显示在正中央

            //设置logo
            lab_Logo.Content = new Image
            {
                Source = new BitmapImage(new Uri("Images/Logo.png", UriKind.Relative))
            };
            label_date.FontSize = 30;

            //初始化属性
            today.province = "NULL";
            today.city = "NULL";
            today.type = "NULL";
            today.tempature = "NULL";
            today.fx = "NULL";
            today.fl = "NULL";
            today.humidity = "NULL";
            today.date = "1970-01-01 00:00:00";
            today.quality = "凑合";

            for (int i = 0; i < 4; i++)
            {
                forecast[i].week = "星期0";
                forecast[i].date = "1970-01-01 00:00:00";
                forecast[i].power = "NULL";
                forecast[i].daytype = "NULL";
                forecast[i].nighttype = "NULL";
                forecast[i].high = "NULL";
                forecast[i].low = "NULL";
            }

            // 初始化列表项
            citys = new List<string>();
            citys.Add("郑州");
            citys.Add("开封");
            citys.Add("北京");

            // 初始化窗口中的列表
            foreach (string city in citys)
            {
                lstBox.Items.Add(city);
            }

            //获取当前路径
            string absPath = Assembly.GetExecutingAssembly().Location;
            string[] paths = absPath.Split('\\');
            string absolutePath = "";
            foreach (string s in paths)
            {
                if (s.Equals("bin")) break;
                absolutePath += s + "\\";
            }
            currentPath = absolutePath;

            //保存窗口中所有Label
            findLabelByName();

            //设置右键菜单
            Grid.MouseRightButtonDown += Element_MouseRightButtonDown;

            //获取网络信息
            GetWeatherInfo(m_cityCode);
        }

        /// <summary>
        /// 以下为前期准备工作
        /// </summary>
        private void findLabelByName()
        {
            for (int i = 1; i < 5; i++)
            {   //设置名字方便查找到对应的标签
                string weekname = "week0" + i.ToString();
                string datename = "date0" + i.ToString();
                string powername = "power0" + i.ToString();
                string typename = "type0" + i.ToString();
                string highname = "high0" + i.ToString();
                string lowname = "low0" + i.ToString();
                string imagename = "image0" + i.ToString();

                //找到对应标签添加标签
                Label weeklabel = (Label)FindName(weekname);
                Label datelabel = (Label)FindName(datename);
                Label powerlabel = (Label)FindName(powername);
                Label typelabel = (Label)FindName(typename);
                Label highlabel = (Label)FindName(highname);
                Label lowlabel = (Label)FindName(lowname);
                Label imagelabel = (Label)FindName(imagename);

                //添加标签到列表中
                lst_week.Add(weeklabel);
                lst_date.Add(datelabel);
                lst_power.Add(powerlabel);
                lst_type.Add(typelabel);
                lst_high.Add(highlabel);
                lst_low.Add(lowlabel);
                lst_image.Add(imagelabel);
            }

            //可以由用户自主控制样式的标签
            list_change.Add(lable_location);
            list_change.Add(label_temparture);
            list_change.Add(label_City);
            list_change.Add(label_shidu);
            list_change.Add(label_fengli);
            list_change.Add(label_fengxiang);
            list_change.Add(Before_city);
            list_change.Add(Before_shidu);
            list_change.Add(Before_fengxiang);
            list_change.Add(Before_fengli);
        }

        //获取搜索框的输入内容
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_cityName = t_search.Text;
        }

        private void b_search_Click(object sender, RoutedEventArgs e)
        {
            if(m_cityName != null && m_cityName != "")
            {
                m_cityCode = weatherTool.getCodeByName(m_cityName);
                GetWeatherInfo(m_cityCode);
            }
            else
            {
                MessageBox.Show("输入内容为空，请输入正确的城市名", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void b_refresh_Click(object sender, RoutedEventArgs e)
        {
            t_search.Text = "";
            GetWeatherInfo(m_cityCode);
        }

        //重写鼠标监听事件，控制窗口移动
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            isMousePressed = true;          //记录按下状态
            mousePos = e.GetPosition(this); //获取当前鼠标位置
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            isMousePressed = false;  //记录按下状态
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isMousePressed)
            {
                Point currentPosition = e.GetPosition(this);

                //计算鼠标变化位置
                double offsetX = currentPosition.X - mousePos.X;
                double offsetY = currentPosition.Y - mousePos.Y;

                //移动窗口
                this.Left += offsetX;
                this.Top += offsetY;

                //更新鼠标位置
                mousePos = currentPosition;
            }
        }

        //右键菜单控件函数
        private void on_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("第xx小组 \\\n 成员: \\\n *** 1111111111 \\\n *** 2222222222 \\\n *** 3333333333 \\\n *** 6666666666 \\", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void on_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 当鼠标右键按下时显示右键菜单
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                ContextMenu contextMenu = this.FindName("ContextMenu") as ContextMenu;
                contextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// 以下为网络部分函数
        /// </summary>
        private void GetWeatherInfo(string cityCode)
        {
            m_weatherClient.Send("SEARCH+" + cityCode);
            while (m_weatherClient.que.Count <= 0) { /* waiting */}
            int cnt = 0;
            while (cnt < 2)
            {
                if(m_weatherClient.que.Count > 0)
                {
                    string[] strs = m_weatherClient.que.Dequeue().Split('+');
                    Status stat = (Status)Enum.Parse(typeof(Status), strs[0]);
                    if (stat == Status.RESPONSE)
                    {
                        // MessageBox.Show("接收到的json消息为：\n" + strs[1]);
                        if (strs[1] != "error" && strs[1] == "today")
                        {
                            parseJson(strs[2]);
                            cnt++;
                        }
                        else if (strs[1] != "error" && strs[1] == "forecast")
                        {
                            parseJsonForecast(strs[2]);
                            cnt++;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 解析预报数据JSON
        /// </summary>
        /// <param name="responseBody"></param>
        private void parseJsonForecast(string responseBody)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(responseBody);
            //json对象中有一个forecasts数组，数组内只有一个json对象
            //数组内json对象内有一个casts数组 casts数组内存有四天的信息
            dynamic forecasts = null;
            if (jsonObj != null)
                forecasts = jsonObj.forecasts[0];
            int i = 0;
            foreach (var cast in forecasts.casts)
            {
                forecast[i].week = cast.week;
                forecast[i].date = cast.date;
                forecast[i].power = cast.daypower;
                forecast[i].daytype = cast.dayweather;
                forecast[i].nighttype = cast.nightweather;
                forecast[i].high = cast.daytemp;
                forecast[i].low = cast.nighttemp;
                i++;
            }
            setLabelContent();
        }

        /// <summary>
        /// 设置控件内容
        /// </summary>
        private void setLabelContent()
        {
            //设置预报数据
            for (int i = 0; i < 4; i++)
            {
                lst_week[i].Content = "星期" + forecast[i].week;

                string[] dateStr = forecast[i].date.Split(' ');
                string monthDay = dateStr[0];
                lst_date[i].Content = monthDay.Substring(5);

                lst_power[i].Content = forecast[i].power;
                lst_type[i].Content = forecast[i].daytype + "\n" + forecast[i].nighttype;
                lst_high[i].Content = forecast[i].high;
                lst_high[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                lst_low[i].Content = forecast[i].low;

                //根据事件切换显示
                lst_image[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                string uri = "Images/";
                if (Convert.ToDateTime(today.date) > Convert.ToDateTime(dateStr[0] + sunset))
                {
                    uri += "night/" + forecast[i].nighttype + ".png";
                }
                else
                {
                    uri += "day/" + forecast[i].daytype + ".png";
                }
                lst_image[i].Content = new Image
                {
                    Source = new BitmapImage(new Uri(uri, UriKind.Relative))
                };
            }
        }
        
        /// <summary>
        /// 解析当日数据JSON
        /// </summary>
        /// <param name="responseBody"></param>
        private void parseJson(string responseBody)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(responseBody);
            dynamic todayJson = jsonObj.lives[0];

            today.province = todayJson.province;
            today.city = todayJson.city;
            today.type = todayJson.weather;
            today.tempature = todayJson.temperature;
            today.fx = todayJson.winddirection;
            today.fl = todayJson.windpower;
            today.humidity = todayJson.humidity;
            today.date = todayJson.reporttime;

            setTodayContent();  //设置窗口显示
            judgeExist();       //判断表是否存在
            insertToDb();       //插入数据库
        }

        /// <summary>
        /// 设置控件内容
        /// </summary>
        private void setTodayContent()
        {
            //大标题属性
            string[] time = today.date.Split(' ');
            label_date.Content = time[0];
            label_temparture.Content = today.tempature + "°";
            lable_location.Content = today.city;

            string uri = "Images/";
            if (DateTime.ParseExact(today.date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) >
                DateTime.ParseExact(time[0] + sunset, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            {
                uri += "night/" + today.type + ".png";
                label_con.Content = "晚上好，希望您每天都能迎来美好的明天。";

                // TODO 设置晚上的背景图
                bkground.Source = img_Sunset;
                bkground_uri = uri_Sunset;
            }
            else
            {
                uri += "day/" + today.type + ".png";

                // TODO 设置白天的背景图
                bkground.Source = img_Sunrise;
                bkground_uri = uri_Sunrise;
            }
            label_weather.Content = new Image
            {
                Source = new BitmapImage(new Uri(uri, UriKind.Relative))
            };

            // TODO 根据天气类型设置背景图片，天气类型在today.type 是个字符串类型，可以直接判断

            //网格四个属性
            label_City.Content = today.province;
            label_shidu.Content = today.humidity;
            label_fengli.Content = today.fl;
            label_fengxiang.Content = today.fx;

            showData();  //显示数据库数据
        }

        /// <summary>
        /// 以下为数据库操作
        /// </summary>
        /*
         * 如果中文乱码，先去数据库里面执行这三行脚本
         * 
            alter database "{绝对路径}.mdf" set single_user with rollback immediate ;
            go

            alter database "{绝对路径}.mdf" collate Chinese_PRC_CI_AS ;
            go

            alter database "{绝对路径}.mdf" set multi_user;

         *因为数据库版本原因，直接设置是不行的，目前找到的解决办法中只有这一种可行
         */

        //判断表是否存在
        private void judgeExist()
        {
            m_tableName = new List<string>();

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + currentPath + "CityWeather.mdf;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 获取数据库中的所有表信息
                SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    m_tableName.Add(tableName);
                }

                reader.Close();
            }

            bTableExist = m_tableName.Contains(today.city);

        }
        //插入数据
        private void insertToDb()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + currentPath + "CityWeather.mdf;Integrated Security=True;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (bTableExist == false)  //表不存在，需要建表
                    {
                        string createTableQuery = $"CREATE TABLE [{today.city}] ( 日期 NVARCHAR (50) NOT NULL,省份 NVARCHAR (50) NULL,城市 NVARCHAR (50) NULL,天气类型 NVARCHAR (50) NULL,温度  NVARCHAR (50) NULL,风向  NVARCHAR (50) NULL,风力  NVARCHAR (50) NULL,湿度  NVARCHAR (50) NULL,空气质量 NCHAR(10) NULL, seq INT IDENTITY (1, 1) NOT NULL, PRIMARY KEY CLUSTERED ([seq] ASC));";
                        using (SqlCommand command = new SqlCommand(createTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        bTableExist = true;
                    }

                    //测试用：当测试用例的表重复时删除表用
                    //string delete = $"delete from {today.city}";
                    //using (SqlCommand command = new SqlCommand(delete, connection))
                    //{
                    //    command.ExecuteNonQuery();
                    //}

                    if (bTableExist)
                    {   //保险措施，保证表存在再往表内插入数据
                        string insertTableQuery = $"insert into [{today.city}] values('{today.date}' ,'{today.province}' ,'{today.city}' ,'{today.type}' ,'{today.tempature}' ,'{today.fx}' ,'{today.fl}' ,'{today.humidity}' ,'{today.quality}')";
                        using (SqlCommand command1 = new SqlCommand(insertTableQuery, connection))
                        {
                            command1.ExecuteNonQuery();
                        }
                    }

                    //关闭数据库连接
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库过程[insertToDb]出错", "错误", MessageBoxButton.OK);
                statMsg.Text += ex.Message + "\n";
            }

        }
        //展示数据
        private void showData()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + currentPath + "CityWeather.mdf;Integrated Security=True;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //在此处向Datagrid展示信息
                    string selectQuery = $"select * from [{today.city}]";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        // 将 DataTable 绑定到 DataGrid 控件
                        dataGrid.ItemsSource = dataTable.DefaultView;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("数据库过程[showData]出错", "错误", MessageBoxButton.OK);
                statMsg.Text += ex.Message + "\n";
            }
        }
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// 以下为右键菜单功能
        /// </summary>

        //查看天气图片
        private void on_OpenType_Click(object sender, RoutedEventArgs e)
        {
            //打开天气类型图片展示对话框
            WeatherTypeImage typeImage = new WeatherTypeImage();
            typeImage.ShowDialog();
        }
        //改变颜色
        private void on_AdjustColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, 252, 255, 0);

            //打开一个Dialog，选择颜色
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                color = System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B);
            }

            //根据获取的结果更改窗口显示效果
            foreach (Label label in list_change)
            {
                label.Foreground = new SolidColorBrush(color);
            }
        }
        //改变不透明度
        private void on_AdjustOpacity_Click(object sender, RoutedEventArgs e)
        {
            //打开一个Dialog 选择不透明度
            OpacityWindow opacitydlg = new OpacityWindow();
            opacitydlg.ShowDialog();

            //根据获取的结果更改窗口显示效果
            double opacity = opacitydlg.m_opacity;
            foreach (Label label in list_change)
            {
                label.Opacity = opacity;
            }
        }
        //更改背景图片
        private void on_bkground_Click(object sender, RoutedEventArgs e)
        {
            timer = new Timer(5000);
            timer.Elapsed += Timer_Elapsed;
            //更改背景
            if(bkground_uri.Equals(uri_weaUI))
            {
                bkground.Source = img_miya;
                bkground_uri = uri_miya;
            }
            else
            {
                bkground.Source = img_weaUI;
                bkground_uri = uri_weaUI;
            }

            //启动计时器
            timer.Start();
        }
        // 加载图片
        private void Image_Loaded()
        {
            bkground.Source = img_weaUI;
            bkground_uri = uri_weaUI;
        }
        //计时器，用来控制图片切换的
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("试用时间到，是否要保留现在的背景？", "残念", MessageBoxButton.YesNo, MessageBoxImage.Information);
            
            //只能在创建空间的线程上对控件进行操作，因此需要用这种方法把更改背景的请求转发给计时器线程
            if(result == MessageBoxResult.Yes)
            {
                //nothing  保留，那就是没有动作，等着计时器结束就行了
            }
            else
            {
                if (!bkground.Dispatcher.CheckAccess())  //安全保障
                {
                    if(bkground_uri.Equals(uri_miya))
                    {
                        bkground.Dispatcher.Invoke(() =>
                        {
                            bkground.Source = img_weaUI;
                            bkground_uri = uri_weaUI;
                        });
                    }
                    else
                    {
                        bkground.Dispatcher.Invoke(() =>
                        {
                            bkground.Source = img_miya;
                            bkground_uri = uri_miya;
                        });
                    }
                }
                else
                {
                    if (bkground_uri.Equals(uri_miya))
                    {
                        bkground.Dispatcher.Invoke(() =>
                        {
                            bkground.Source = img_weaUI;
                            bkground_uri = uri_weaUI;
                        });
                    }
                    else
                    {
                        bkground.Dispatcher.Invoke(() =>
                        {
                            bkground.Source = img_miya;
                            bkground_uri = uri_miya;
                        });
                    }
                }
            }
            
            timer.Stop();
        }
        //一键隐藏所有控件，可爱米娅纯享模式
        bool isPure = false;
        private void on_PureEnjoyment_click(object sender, RoutedEventArgs e)
        {
            if(!isPure)
            {
                isPure = true;

                //隐藏所有控件
                foreach(Label label in lst_week)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in lst_date)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in lst_power)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in lst_type)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in lst_high)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in lst_low)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in lst_image)
                {
                    label.Visibility = Visibility.Hidden;
                }
                foreach (Label label in list_change)
                {
                    label.Visibility = Visibility.Hidden;
                }
                label_date.Visibility = Visibility.Hidden;
                label_temparture.Visibility = Visibility.Hidden;
                lable_location.Visibility = Visibility.Hidden;
                label_weather.Visibility = Visibility.Hidden;
                lab_title.Visibility = Visibility.Hidden;
                lab_Logo.Visibility = Visibility.Hidden;
                label_con.Visibility = Visibility.Hidden;
                dataGrid.Visibility = Visibility.Hidden;
                statMsg.Visibility = Visibility.Hidden;
                t_search.Visibility = Visibility.Hidden;
                b_search.Visibility = Visibility.Hidden;
                b_refresh.Visibility = Visibility.Hidden;
                forecast_total.Visibility = Visibility.Hidden;
                today_total.Visibility= Visibility.Hidden;

                menu_mia.Header = "恢复";
            }
            else
            {
                isPure = false;

                //显示所有控件
                foreach (Label label in lst_week)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in lst_date)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in lst_power)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in lst_type)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in lst_high)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in lst_low)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in lst_image)
                {
                    label.Visibility = Visibility.Visible;
                }
                foreach (Label label in list_change)
                {
                    label.Visibility = Visibility.Visible;
                }
                label_date.Visibility = Visibility.Visible;
                label_temparture.Visibility = Visibility.Visible;
                lable_location.Visibility = Visibility.Visible;
                lab_title.Visibility = Visibility.Visible;
                lab_Logo.Visibility = Visibility.Visible;
                label_con.Visibility = Visibility.Visible;
                dataGrid.Visibility = Visibility.Visible;
                statMsg.Visibility = Visibility.Visible;
                t_search.Visibility = Visibility.Visible;
                b_search.Visibility = Visibility.Visible;
                b_refresh.Visibility = Visibility.Visible;
                forecast_total.Visibility = Visibility.Visible;
                today_total.Visibility = Visibility.Visible;

                menu_mia.Header = "米娅纯享";
            }
        }

        /// <summary>
        /// 附加功能
        /// </summary>

        //OpenSSL非对称加密
        private string opensslEncode(string password)
        {
            //openssl使用rsa加密所使用的公钥和私钥
            //如果启用了这个函数，记得把这两个东西挪到上面去
            string PRIVATE_KEY = @"MIICXgIBAAKBgQC0xP5HcfThSQr43bAMoopbzcCyZWE0xfUeTA4Nx4PrXEfDvybJ
                                   EIjbU/rgANAty1yp7g20J7+wVMPCusxftl/d0rPQiCLjeZ3HtlRKld+9htAZtHFZ
                                   osV29h/hNE9JkxzGXstaSeXIUIWquMZQ8XyscIHhqoOmjXaCv58CSRAlAQIDAQAB
                                   AoGBAJtDgCwZYv2FYVk0ABw6F6CWbuZLUVykks69AG0xasti7Xjh3AximUnZLefs
                                   iuJqg2KpRzfv1CM+Cw5cp2GmIVvRqq0GlRZGxJ38AqH9oyUa2m3TojxWapY47zye
                                   PYEjWwRTGlxUBkdujdcYj6/dojNkm4azsDXl9W5YaXiPfbgJAkEA4rlhSPXlohDk
                                   FoyfX0v2OIdaTOcVpinv1jjbSzZ8KZACggjiNUVrSFV3Y4oWom93K5JLXf2mV0Sy
                                   80mPR5jOdwJBAMwciAk8xyQKpMUGNhFX2jKboAYY1SJCfuUnyXHAPWeHp5xCL2UH
                                   tjryJp/Vx8TgsFTGyWSyIE9R8hSup+32rkcCQBe+EAkC7yQ0np4Z5cql+sfarMMm
                                   4+Z9t8b4N0a+EuyLTyfs5Dtt5JkzkggTeuFRyOoALPJP0K6M3CyMBHwb7WsCQQCi
                                   TM2fCsUO06fRQu8bO1A1janhLz3K0DU24jw8RzCMckHE7pvhKhCtLn+n+MWwtzl/
                                   L9JUT4+BgxeLepXtkolhAkEA2V7er7fnEuL0+kKIjmOm5F3kvMIDh9YC1JwLGSvu
                                   1fnzxK34QwSdxgQRF1dfIKJw73lClQpHZfQxL/2XRG8IoA==";
            string PUBLIC_KEY = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC0xP5HcfThSQr43bAMoopbzcCy
                                  ZWE0xfUeTA4Nx4PrXEfDvybJEIjbU/rgANAty1yp7g20J7+wVMPCusxftl/d0rPQ
                                  iCLjeZ3HtlRKld+9htAZtHFZosV29h/hNE9JkxzGXstaSeXIUIWquMZQ8XyscIHh
                                  qoOmjXaCv58CSRAlAQIDAQAB";

            //创建一个工具类实例，用来执行加密操作
            RSACryptoService ssl = new RSACryptoService(PRIVATE_KEY, PUBLIC_KEY);
            string encode = ssl.Encrypt(password);
            return encode;

            //如果需要解密，调用这个
            //string decode = ssl.Decrypt(encode);

            /*
             * 在本项目中对于用户的加密并不是主要功能，因此这个函数以及工具类并不启用
             * 简单的加密用MD5足矣，只要用户设置的密码稍微复杂一丢丢，MD5的安全系数是相当高的
             * 即使没必要，但我不能没有
             */
        }

        /// <summary>
        /// 列表中选择某一个城市，然后查询该城市的信息并显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = lstBox.SelectedItem;
            if(item != null)
            {
                string selectedCity = item.ToString();
                m_cityName = selectedCity;
                m_cityCode = weatherTool.getCodeByName(m_cityName);
                GetWeatherInfo(m_cityCode);
            }
        }
        /// <summary>
        /// 将当前的城市信息添加至列表中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_addToList_Click(object sender, RoutedEventArgs e)
        {
            // 忘了初始化设定的是啥了，总之全都判断了就好了
            if(m_cityName !=  null && m_cityName != "")
            {
                if (citys.Contains(m_cityName))
                {
                    MessageBox.Show("当前城市已存在，请更换为其他城市...");
                }
                else
                {
                    lstBox.Items.Add(m_cityName);
                }
            }
        }
        /// <summary>
        /// 清空列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clearList_Click(object sender, RoutedEventArgs e)
        {
            // 先清空选择项
            lstBox.SelectedItem = null;
            lstBox.Items.Clear();
        }
    }
}
