using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModelBridge
{
    public class WeatherInfoMap
    {
        // 这里的单例，也没用。本来应该是有用的
        public static WeatherInfoMap instance;
        public static WeatherInfoMap GetInstance()
        {
            if (instance == null)
                instance = new WeatherInfoMap();
            return instance;
        }
        public Dictionary<string, string> dict_all { get; set; }
        // public Dictionary<string, string> dict_forecast { get; set; }
        public WeatherInfoMap()
        {
            dict_all = new Dictionary<string, string>();
            // dict_forecast = new Dictionary<string, string>();
        }

        public void SetTodayMap(Dictionary<string, string> dict)
        {
            Console.WriteLine($"调用写入文件函数, now dict.Count = {dict.Count}");
            // 这俩没用了，因为不想大动原函数所以就接着这个函数写了，但其实不是这个意思，可以单开一个函数去完成这项功能
            // dict_today.Clear();
            // dict_today = dict;

            // 先找到路径
            string userRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userRoot += "\\AppData\\Local\\Temp\\";

            userRoot = "C:\\Users\\Accel\\Desktop\\";

            WriteDictionaryToFile(dict_all, userRoot + "dictionary.txt");
        }
        // 把文件写到本地去，通过本地文件进行项目间的传递
        public void WriteDictionaryToFile(Dictionary<string, string> dictionary, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    string jsonString = JsonSerializer.Serialize(dictionary);
                    writer.Write(jsonString);
                }

                Console.WriteLine("文件写入到: " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
            //try
            //{
            //    if (File.Exists(filePath))
            //    {
            //        if (new FileInfo(filePath).Length > 0)
            //        {
            //            File.WriteAllText(filePath, string.Empty);
            //        }
            //    }

            //    // json是个好东西，多用用
            //    string jsonString = JsonSerializer.Serialize(dictionary);
            //    File.WriteAllText(filePath, jsonString);
            //    Console.WriteLine("文件写入到: " + filePath);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"ERROR: {ex.Message}");
            //}
        }



        // 下面的没用，希望有一天它们能有用

        public void SetForecastMap(Dictionary<string, string> dict)
        {
            // dict_forecast.Clear();
            // dict_forecast = dict;
        }

        public Dictionary<string, string> GetTodayMap()
        {
            // 但是下面拿到的全都是0,而且一直是0，就很奇怪
            if (dict_all != null) Console.WriteLine("不是空，能输出 Count = " + instance.dict_all.Count);
            return instance.dict_all;
        }

        public Dictionary<string, string> GetForecastMap()
        {
            // return dict_forecast;
            return null;
        }

        
    }
}
