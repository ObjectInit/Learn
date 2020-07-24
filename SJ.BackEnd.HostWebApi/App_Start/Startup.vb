Imports System.Web.Http
Imports Owin
Imports SJ.BackEnd.Biz.AC
Imports SJ.BackEnd.Biz.Pub

Public Class Startup
    Public Sub Configuration(ByVal app As IAppBuilder)
        Dim configuraton = New HttpConfiguration()
        WebApiConfig.Register(configuraton)

        Dim type = New PubOthers()

        Dim receipt = New AcReceipt()

        app.UseWebApi(configuraton)
    End Sub
End Class
