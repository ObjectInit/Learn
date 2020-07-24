# 后端WebApi (后端开发调试环境)


栏目|内容
-|-
创建|彭峰
日期|2019-07-18
语言|VB
提供|无需混淆DLL
备注|需引用:sj.global SJ.BackEnd.Base 及其他SJ.BackEnd.Biz.*
Nuget|Microsoft.AspNet.WebApi.Cors


---

# 注意

+ 应在Web.Config中配置  Shiji后端Webapi应用配置
+ 应在Global.asax进行初始化注入
+ 应在WebApiConfig.vb 中注册路由 SJApiController Index方法
+ 应建立Log目录存放Log文件
+ 可建立单独类库加入解决方案,例如:SJ.BackEnd.PICC
+ 每个开发人员建立个人目录,每个类添加应用前缀,例如:/alex/PIArticle

# 主要工作
+ 继承实现业务实体
+ 继承实现数据库升级类

# 测试
+ 内部测试,参见:SJ.BackEnd.HostWebApi.Tests中ControllerTest.vb
+ 使用 SJ.WebApi.Client 进行测试


# 可使用客户端 SJ.WebApi.Client 访问


# 直接访问方法可参见SJ.WebApi.Client



更新日志:

