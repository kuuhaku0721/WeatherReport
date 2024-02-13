alter database "D:\C#\code\WeatherReportUI\WeatherReport_cs - 副本\WeatherReport_cs\CityWeather.mdf" set single_user with rollback immediate ;
go

alter database "D:\C#\code\WeatherReportUI\WeatherReport_cs - 副本\WeatherReport_cs\CityWeather.mdf" collate Chinese_PRC_CI_AS ;
go

alter database "D:\C#\code\WeatherReportUI\WeatherReport_cs - 副本\WeatherReport_cs\CityWeather.mdf" set multi_user;