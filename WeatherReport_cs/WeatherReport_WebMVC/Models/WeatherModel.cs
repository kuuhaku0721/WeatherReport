namespace WeatherReport_WebMVC.Models
{
    public class WeatherModel
    {
        // 懒省事了一点，这一个map里面存的所有的信息，共计36条
        public Dictionary<string, string>? dict_all { get; set; }
    }
}
