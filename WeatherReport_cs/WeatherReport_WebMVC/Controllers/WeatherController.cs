using Microsoft.AspNetCore.Mvc;
using WeatherReport_WebMVC.Models;

namespace WeatherReport_WebMVC.Controllers
{
    public class WeatherController : Controller
    {
        private IWeatherModelService service;
        public WeatherController(IWeatherModelService service)
        {
            this.service = service;
        }
        public IActionResult Index()
        {
            WeatherModel weather = service.GetWeather();
            if (weather.dict_all != null)
                return View(weather);
            else
            {
                // INFO: 设置占位符的原因，程序第一次启动的时候，浏览器起的比客户端快，这就导致数据还没拿到
                //       浏览器就需要去展示了，所以需要有一个占位符，在没有数据的时候展示页面，以保证页面能显示出来东西

                // 设置当天的占位符
                weather.dict_all = new Dictionary<string, string>();
                weather.dict_all["province"] = "placeholder";
                weather.dict_all["city"] = "placeholder";
                weather.dict_all["type"] = "placeholder";
                weather.dict_all["temperature"] = "placeholder";
                weather.dict_all["fx"] = "placeholder";
                weather.dict_all["fl"] = "placeholder";
                weather.dict_all["humidity"] = "placeholder";
                weather.dict_all["date"] = "placeholder";
                // 设置预报信息的占位符
                for (int i = 0; i < 4; i++)
                {
                    weather.dict_all["" + i + "week"] = "placeholder";
                    weather.dict_all["" + i + "date"] = "placeholder";
                    weather.dict_all["" + i + "power"] = "placeholder";
                    weather.dict_all["" + i + "daytype"] = "placeholder";
                    weather.dict_all["" + i + "nighttype"] = "placeholder";
                    weather.dict_all["" + i + "high"] = "placeholder";
                    weather.dict_all["" + i + "low"] = "placeholder";
                }
                return View(weather);
            }
        }
    }
}
