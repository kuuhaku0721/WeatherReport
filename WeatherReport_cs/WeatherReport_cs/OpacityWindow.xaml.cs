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
    /// OpacityWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OpacityWindow : Window
    {
        public double m_opacity {  get; set; }
        public OpacityWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //显示在正中央
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string opacity_str = comboBox.SelectedItem.ToString().Substring(comboBox.SelectedItem.ToString().IndexOf(":") + 2);
            lab_test.Opacity = double.Parse(opacity_str);
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            string opacity_str = comboBox.SelectedItem.ToString().Substring(comboBox.SelectedItem.ToString().IndexOf(":") + 2);
            m_opacity = double.Parse(opacity_str);
            this.Close();
        }
    }
}
