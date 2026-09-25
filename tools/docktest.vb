Imports System.Windows.Forms
Imports System.Drawing

Public Class FStack
    Inherits Panel
    Protected Overrides Sub OnControlAdded(e As ControlEventArgs)
        MyBase.OnControlAdded(e)
        If e.Control.Dock <> DockStyle.None Then
            e.Control.BringToFront()
        End If
    End Sub
End Class

Module DockTest

    Sub Run(title As String, build As Action(Of Panel), Optional asStack As Boolean = True)
        Dim f As New Form() With {.ClientSize = New Size(300, 300)}
        Dim p As Panel
        If asStack Then p = New FStack() Else p = New Panel()
        p.Dock = DockStyle.Fill
        f.Controls.Add(p)
        build(p)
        f.PerformLayout()
        p.PerformLayout()
        f.PerformLayout()

        Console.Write(title & ": ")
        For Each c As Control In p.Controls
            Console.Write(c.Text & "(" & c.Top & ",h" & c.Height & ") ")
        Next
        Console.WriteLine()
        f.Dispose()
    End Sub

    Function L(text As String, height As Integer, Optional dock As DockStyle = DockStyle.Top) As Label
        Return New Label() With {.Text = text, .Height = height, .Dock = dock, .BackColor = Color.FromArgb(80, 160, 90)}
    End Function

    Sub Main()
        ' PLAIN panel, pure top stack
        Run("PLAIN tops A,B,C", Sub(p)
                                    p.Controls.Add(L("A", 40))
                                    p.Controls.Add(L("B", 40))
                                    p.Controls.Add(L("C", 40))
                                End Sub, False)

        ' FStack pure top stack
        Run("FSTACK tops A,B,C", Sub(p)
                                     p.Controls.Add(L("A", 40))
                                     p.Controls.Add(L("B", 40))
                                     p.Controls.Add(L("C", 40))
                                 End Sub, True)

        ' PLAIN panel mimicking Ui.Content: top autosize children inside
        Run("PLAIN T,F,T2", Sub(p)
                                p.Controls.Add(L("T", 60))
                                Dim fl As New Panel() With {.Dock = DockStyle.Fill, .BackColor = Color.FromArgb(120, 120, 200)}
                                p.Controls.Add(fl)
                                fl.Name = "F"
                                p.Controls.Add(L("T2", 40))
                            End Sub, False)

        ' PLAIN: does adding Fill LAST matter for ordering of tops above/below?
        Run("PLAIN T,T2,F", Sub(p)
                                p.Controls.Add(L("T", 60))
                                p.Controls.Add(L("T2", 40))
                                Dim fl As New Panel() With {.Dock = DockStyle.Fill, .BackColor = Color.FromArgb(120, 120, 200)}
                                p.Controls.Add(fl)
                                fl.Name = "F"
                            End Sub, False)
    End Sub
End Module
