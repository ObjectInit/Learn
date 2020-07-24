Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Http
Imports System.Web.Http.Cors
Imports Newtonsoft.Json
Imports SJ.Global

Public Module WebApiConfig
    Public Sub Register(ByVal config As HttpConfiguration)
        ' Web API 配置和服务
        config.EnableCors(New EnableCorsAttribute("*", "*", "*"))
        ' Web API 路由
        config.MapHttpAttributeRoutes()

        'config.Routes.MapHttpRoute(
        '    name:="DefaultApi",
        '    routeTemplate:="api/{controller}/{id}",
        '    defaults:=New With {.id = RouteParameter.Optional}
        ')

        '实际WebApi入口,路由进入SJApiController Index方法
        config.Routes.MapHttpRoute(
            name:="ShijiApi",
            routeTemplate:="sj/{act}/{entity}",
            defaults:=New With {.controller = "SJApi", .action = "index",
            .entity = RouteParameter.Optional,
            .act = RouteParameter.Optional}
        )

        Dim formatters = config.Formatters
        formatters.Remove(formatters.XmlFormatter)
        Dim json = config.Formatters.JsonFormatter
        json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented

        json.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        json.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss"
        json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        json.SerializerSettings.NullValueHandling = NullValueHandling.Ignore '忽略null值
        json.SerializerSettings.Converters.Add(New SDataJsConvert)

    End Sub
End Module
