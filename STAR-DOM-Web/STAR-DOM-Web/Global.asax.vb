Imports System.Web
Imports STAR_DOM.Database

Namespace STAR_DOM.Web

    Public Class GlobalApp
        Inherits HttpApplication

        Sub Application_Start(sender As Object, e As EventArgs)
            ' Keep the AppErrors log bounded; prune rows older than the retention window.
            Db.PruneErrors()
        End Sub

        Sub Application_Error(sender As Object, e As EventArgs)
            Dim ex As Exception = Server.GetLastError()
            If ex IsNot Nothing Then
                Db.LogError("Web", ex)
            End If
            Dim base As Exception = If(ex IsNot Nothing, ex.GetBaseException(), Nothing)
            Session("LastError") = If(base IsNot Nothing, base.Message, "An unexpected error occurred.")
            ' Logged + user-friendly page; do not let the raw exception reach the client.
        End Sub

        Sub Session_Start(sender As Object, e As EventArgs)
        End Sub

        Sub Session_End(sender As Object, e As EventArgs)
        End Sub

    End Class

End Namespace
