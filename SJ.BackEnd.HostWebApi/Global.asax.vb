Imports System.Diagnostics.Eventing.Reader
Imports System.IO
Imports System.Web.Http
Imports SJ.BackEnd.Biz.AC
Imports SJ.BackEnd.Biz.Pub
Imports SJ.Global
Imports SJ.Pub

Public Class WebApiApplication
    Inherits System.Web.HttpApplication

    Protected Sub Application_Start()
        Dim other = New PubOthers()
        AreaRegistration.RegisterAllAreas()
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)


        'FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
        'RouteConfig.RegisterRoutes(RouteTable.Routes)
        'BundleConfig.RegisterBundles(BundleTable.Bundles)

        '--------------------------
        '   初始化注入
        '--------------------------
        '从Web.Config中获得配置参数

        Dim folder As String = String.Format("{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, "Log")

        If Not Directory.Exists(folder) Then
            Directory.CreateDirectory(folder)
        End If


        Env.ConfigFunc = Function(Key)
                             Return ConfigurationManager.AppSettings(Key)
                         End Function

        'Log写入Log目录下文件,分日期命名,例如:2019-08-21.log
        Env.LogFunc = Sub(s)
                          Dim fn As String = $"{folder}\{Format(Now, "yyyy-MM-dd HH")}.log"
                          Do While True
                              Try
                                  My.Computer.FileSystem.WriteAllText(fn, $"{ Now.ToString("HH:mm:ss.fff")} : {s}{vbCrLf}", True)
                                  Return
                              Catch ex As Exception
                                  Threading.Thread.Sleep(100)
                              End Try
                          Loop
                      End Sub

        Dim cacheInstance As ICache
        cacheInstance = CacheFactory.BuildInstance()
        'Session写入本地缓存(负载均衡时,应集中写入Memcache 或 Redis)
        Env.SessionSet = Sub(s As String, data As Object)
                             If data Is Nothing Then
                                 cacheInstance.RemoveSession(s)
                             Else
                                 cacheInstance.SetSession(s, data, TimeSpan.FromMinutes(Double.Parse(Env.Config("sessionTimeout", "20"))))
                             End If
                         End Sub
        Env.SessionGet = Function(s As String) As Object
                             Return cacheInstance.GetSession(s, TimeSpan.FromMinutes(Double.Parse(Env.Config("sessionTimeout", "20"))))
                         End Function

        ''Session写入本地缓存(负载均衡时,应集中写入Memcache 或 Redis)
        'Env.SessionSet = Sub(s As String, data As Object)
        '                     If data Is Nothing Then
        '                         Web.HttpRuntime.Cache.Remove(s)
        '                     Else
        '                         Web.HttpRuntime.Cache.Insert(s, data, Nothing, Date.MaxValue, TimeSpan.FromMinutes(Env.Config("sessionTimeout", 20)), Web.Caching.CacheItemPriority.Normal, Nothing) '// Nothing, Date.MaxValue, TimeSpan.FromMinutes(20), Web.Caching.CacheItemPriority.Normal, Nothing)
        '                         'Web.HttpRuntime.Cache.Insert(s, data, Nothing, Date.MaxValue, TimeSpan.FromMinutes(20), Web.Caching.CacheItemPriority.Normal, Nothing)
        '                     End If
        '                 End Sub
        'Env.SessionGet = Function(s As String) As Object
        '                     Return Web.HttpRuntime.Cache(s)
        '                 End Function


        '注入多账套策略
        Env.DomainDBConfigFunc = Function(domain As String)
                                     Dim dbconf As New Env.DBConfig()
                                     If domain.ToLower() = "sj" Then
                                         If Not String.IsNullOrEmpty(Env.Config("dbname")) Then

                                             dbconf.DbServer = Env.Config("dbserver")
                                             dbconf.DbName = Env.Config("dbname")
                                         Else
                                             dbconf.DbServer = Env.Config($"dbserver01")
                                             dbconf.DbName = Env.Config($"dbname01")
                                         End If
                                     Else
                                         dbconf.DbServer = Env.Config($"dbserver{domain}")
                                         dbconf.DbName = Env.Config($"dbname{domain}")
                                     End If

                                     dbconf.DbUser = Env.Config("dbuser")
                                     dbconf.DbPass = Env.Config("dbpass")
                                     Return dbconf

                                 End Function

        '设置语言包路径
        Env.LanguagePath = Server.MapPath("/lang")

        '全局切片接口示例
        'Env.AddCEventHandler("getlist", New TestGetlistHandler)

        Env.Log("Application_Start")
        Dim rpt = New Person() '防止找不到实体
    End Sub

End Class
