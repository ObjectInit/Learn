Imports SJ.Global

Public Class TestGetlistHandler
    Inherits SJ.Global.HandlerWebApi

    Protected Overrides Sub HandlerBefore(Domain As String, Entity As String, PostData As SData)
        If Entity = "AcAccount" Then Throw New Exception("捕捉到 GetList AcAccount")
    End Sub

    Protected Overrides Sub HandlerAfter(Domain As String, Entity As String, PostData As SData)
        If Entity = "AcPayD" Then Throw New Exception("AcPayDAcPayDAcPayDAcPayDAcPayD")
    End Sub

End Class

Public Class MyHandler
    Inherits HandlerWebApi
    Protected Overrides Sub HandlerBefore(Domain As String, Entity As String, PostData As SData)
        If Entity = "AcAccount" Then Throw New Exception("捕捉到 GetList AcAccount")
    End Sub

    Protected Overrides Sub HandlerAfter(Domain As String, Entity As String, PostData As SData)
        If Entity = "AcAnalyType" Then PostData.Append("size", 1)
    End Sub
End Class
