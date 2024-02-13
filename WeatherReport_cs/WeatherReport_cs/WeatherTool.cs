using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherReport_cs
{
    //工具类
    //作用：读取文件，将城市名和代码保存为map
    internal class WeatherTool
    {
        private string filename = "citycode-2023.json";     //文件名 保存在bin/Debug下
        private Dictionary<string, string> m_mapCity2Code;  
        private string fileInfo;

        public WeatherTool() 
        {
            m_mapCity2Code = new Dictionary<string, string>();
            //1、读取文件，保存为一整个字符串
            readFile();

            //2、parseJson，将文件中的json文件转为键值对
            if(fileInfo != null)
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(fileInfo);
                dynamic citys = jsonObj.citycode;
                foreach (var city in citys)
                {
                    try
                    {
                        m_mapCity2Code.Add((string)city.city_name, (string)city.city_code);
                    }
                    catch
                    {   //有的区可能会重名，所以用catch捕获，把重名的名字后跟上区号
                        m_mapCity2Code.Add((string)city.city_name + (string)city.area_code, (string)city.city_code);
                    }
                }
            }
        }
        private void readFile()
        {
            if(File.Exists(filename) == false) 
            {
                MessageBox.Show("未查询到文件内容", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            fileInfo = File.ReadAllText(filename);
        }

        public string getCodeByName(string cityName)
        {
            string cityname = "NULL";
            foreach(var v in m_mapCity2Code)
            {
                if(v.Key.Contains(cityName))
                    cityname = v.Key;
            }
            if(cityname == "NULL")
            {
                MessageBox.Show("当前搜索城市国内不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                return "110000";
            }

            string cityCode = m_mapCity2Code[cityname];
            return cityCode;
        }
    }
}
