using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WeatherReport_cs
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string username;
        private string password;

        private bool isMousePressed;
        private Point mousePos;
        public LoginWindow()
        {
            InitializeComponent();

            //要保证username和password有值，即不为NULL，以防出现NULLPointerException
            //只是习惯性保险措施，后面也有安全性验证，不赋初值也没事
            username = "NULL";
            password = "NULL";

            //设置窗口属性
            this.WindowStyle = WindowStyle.None;   //无边框
            this.ResizeMode = ResizeMode.NoResize; //不允许改变大小
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //显示在正中央

            //设置默认显示
            text_userName.Text = "kuuhaku";
            text_password.Password = "123456";

            //设置右键菜单
            Grid.MouseRightButtonDown += Element_MouseRightButtonDown;
        }
        
        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            username = text_userName.Text;
            password = text_password.Password;
            if (username == null || password == null)
            {
                text_stat.Content = "用户名或密码为空，请检查输入";
                text_password.Clear();
                text_password.Focus();
                return;
            }

            if (isReleased())
            {
                text_stat.Content = "登录成功";

                //融合进去原项目时打开注释
                this.Close();
            }
            else
            {
                text_stat.Content = "用户名或密码错误";
                text_password.Clear();
                text_password.Focus();
            }
        }
        private bool isReleased()
        {
            //加密密码
            string md5Pwd = Encode();  //密码: 123456 --> 49BA59ABBE56E057

            //以用户名为条件，在数据库中查找密码
            using (var c = new UserEntities())
            {
                try
                {
                    var query = from u in c.UserManager
                                where u.name == username
                                select u;

                    //查询结束后比对
                    var user = query.FirstOrDefault();
                    if(user != null) 
                    {
                        if (md5Pwd.Equals(user.pwd))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch(Exception ex)
                {
                    text_stat.Content = "用户名或密码错误";
                    text_password.Clear();
                    text_password.Focus();
                    return false;
                }
            }
        }
        private string Encode()
        {   //将密码加密为16为MD5字符串
            var md5 = new MD5CryptoServiceProvider();
            string md5Str = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(password)), 4, 8);
            md5Str = md5Str.Replace("-", "");
            return md5Str;
        }
        private void btn_register_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("功能没做，欸嘿 վ'ᴗ' ի", "啊吧啊吧", MessageBoxButton.OK);
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
            MessageBox.Show("登录信息: \\\n kuuhaku 123456 \\\n admin 123456 \\", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void on_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void on_Secret_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("恭喜你，解锁了隐藏功能", "恭喜！！", MessageBoxButton.OK, MessageBoxImage.Warning);
            SecretDialog dlg = new SecretDialog();
            dlg.ShowDialog();
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
    }
}
