using ModelBridge;
using System.Reflection;
using System.Text.Json;

#pragma warning disable IDE0044

namespace WeatherReport_WebMVC.Models
{
    public class WeatherModelImplement : IWeatherModelService
    {
        private WeatherModel model;
        private Dictionary<string, string> dict_all;
        public WeatherModelImplement()
        {
            model = new WeatherModel();
            dict_all = new Dictionary<string, string>();
        }

        public WeatherModel GetWeather()
        {
            // 先找路径
            string userRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userRoot += "\\AppData\\Local\\Temp\\";

            // 测试用桌面路径
            // userRoot = "C:\\Users\\Accel\\Desktop\\";

            ReadDictionaryFromFile(userRoot + "dictionary.txt");
            if (dict_all == null 
                || dict_all.Count == 0)
            {
                Console.WriteLine("现在拿到的today的数量, dict_today.Count = " + WeatherInfoMap.GetInstance().dict_all.Count);
                Console.WriteLine("空转ing...");
                Console.WriteLine("[WRAN]");
                Console.WriteLine("如果在您第一次打开的时候显示的是一堆placeholder占位符\n" +
                    "不用担心，这是因为第一次打开没有数据，并不是出错，属于正常现象\n" +
                    "之后通过点击客户端左上角Logo再次打开浏览器页面即可看到数据");
                Console.WriteLine("[WRAN]");
                return model;
            }
            Console.WriteLine("获取到当天信息...");
            model.dict_all = dict_all;
            return model;
        }
        public void ReadDictionaryFromFile(string filePath)
        {
            try
            {
                Console.WriteLine($"从 {filePath} 读取内容出来");
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    dict_all = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);                   
                }
                else
                {
                    Console.WriteLine("ERROR: 文件不存在");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
    }
}
