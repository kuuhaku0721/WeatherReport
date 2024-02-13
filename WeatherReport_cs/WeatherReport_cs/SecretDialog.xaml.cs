using System;
using System.Collections.Generic;
using System.Linq;
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
    /// SecretDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SecretDialog : Window
    {
        public SecretDialog()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            lab_welcome.Content = "欢迎使用打赏功能";
            lab_welcome.HorizontalContentAlignment = HorizontalAlignment.Center;
            lab_welcome.FontSize = 20;
            lab_yahaha.Content = new Image
            {
                Source = new BitmapImage(new Uri("Images/yahaha.png", UriKind.Relative))
            };
        }
    }
}
