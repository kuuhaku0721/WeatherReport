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
    /// WeatherTypeImage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherTypeImage : Window
    {
        public WeatherTypeImage()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            string selectName = comboBox.SelectedItem.ToString();
            string imgName = selectName.Substring(selectName.IndexOf(":") + 2);
            string imgpath_day = "Images/day/" + imgName + ".png";
            string imgpath_night = "Images/night/" + imgName + ".png";
            lab_day.Content = new Image
            {
                Source = new BitmapImage(new Uri(imgpath_day, UriKind.Relative))
            };
            lab_night.Content = new Image
            {
                Source = new BitmapImage(new Uri(imgpath_night, UriKind.Relative))
            };
        }
    }
}
