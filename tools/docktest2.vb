Imports System.Windows.Forms
Imports System.Drawing

Module DockTest2

    Function L(text As String, height As Integer, Optional dock As DockStyle = DockStyle.Top) As Label
        Return New Label() With {.Text = text, .Height = height, .Dock = dock, .BackColor = Color.FromArgb(80, 160, 90)}
    End Function

    Sub Dump(title As String, f As Form)
        f.PerformLayout()
        For Each c As Control In f.Controls
            Console.Write("  " & c.Text & "(" & c.Top & "," & c.Left & ",h" & c.Height & ",w" & c.Width & ")")
        Next
        Console.WriteLine()
    End Sub

    Sub DumpP(title As String, p As Control)
        Console.Write(title & ":")
        For Each c As Control In p.Controls
            Console.Write("  " & c.Text & "(" & c.Top & ",h" & c.Height & ")")
        Next
        Console.WriteLine()
    End Sub

    Sub Main()
        ' A) Canonical MainForm pattern on a plain Form
        Dim f1 As New Form() With {.ClientSize = New Size(900, 600)}
        Dim side As New Panel() With {.Dock = DockStyle.Left, .Width = 200, .BackColor = Color.White}
        Dim hdr As New Panel() With {.Dock = DockStyle.Top, .Height = 50, .BackColor = Color.LightGray}
        Dim host As New Panel() With {.Dock = DockStyle.Fill, .BackColor = Color.AliceBlue}
        side.Name = "SIDE" : hdr.Name = "HDR" : host.Name = "HOST"
        f1.Controls.Add(side) : f1.Controls.Add(hdr) : f1.Controls.Add(host)
        Console.Write("A) form L,T,F:")
        For Each c As Control In f1.Controls
            Console.Write("  " & c.Name & "(" & c.Top & "," & c.Left & ",h" & c.Height & ",w" & c.Width & ")")
        Next
        Console.WriteLine()
        f1.Dispose()

        ' B) Ui.Content replicate: Fill+AutoScroll page with one Dock=Top AutoSize child,
        '    that child receives three Dock=Top children A,B,C (visual order add).
        Dim f2 As New Form() With {.ClientSize = New Size(900, 600)}
        Dim page As New Panel() With {.Dock = DockStyle.Fill, .AutoScroll = True}
        f2.Controls.Add(page)
        Dim content As New Panel() With {.Dock = DockStyle.Top, .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink}
        page.Controls.Add(content)
        content.Controls.Add(L("SecA", 80))
        content.Controls.Add(L("SecB", 90))
        content.Controls.Add(L("SecC", 70))
        f2.PerformLayout()
        page.PerformLayout()
        f2.PerformLayout()
        Console.Write("B) content children:")
        For Each c As Control In content.Controls
            Console.Write("  " & c.Text & "(" & c.Top & ",h" & c.Height & ")")
        Next
        Console.WriteLine()
        Console.WriteLine("   content h=" & content.Height & " page.AutoScrollMinSize=" & page.AutoScrollMinSize.ToString())
        f2.Dispose()

        ' C) same as B but content children added in REVERSE visual order
        Dim f3 As New Form() With {.ClientSize = New Size(900, 600)}
        Dim page3 As New Panel() With {.Dock = DockStyle.Fill, .AutoScroll = True}
        f3.Controls.Add(page3)
        Dim content3 As New Panel() With {.Dock = DockStyle.Top, .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink}
        page3.Controls.Add(content3)
        content3.Controls.Add(L("SecC", 70))
        content3.Controls.Add(L("SecB", 90))
        content3.Controls.Add(L("SecA", 80))
        f3.PerformLayout()
        page3.PerformLayout()
        f3.PerformLayout()
        Console.Write("C) content children (rev add):")
        For Each c As Control In content3.Controls
            Console.Write("  " & c.Text & "(" & c.Top & ",h" & c.Height & ")")
        Next
        Console.WriteLine()
        f3.Dispose()
    End Sub
End Module
