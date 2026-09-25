Imports System.Drawing

Module Probe
    Private Function Palette(seed As Integer) As (Color, Color)
        Return (Color.Red, Color.Blue)
    End Function

    Sub Main()
        Dim (c1, c2) = Palette(5)
        Dim bmp As New Bitmap(10, 10)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(c1)
        End Using
        Dim list As New List(Of (Date, Decimal))()
        list.Add((Date.Now, 1.5D))
        System.Console.WriteLine(c2.ToString() & " " & list.Count)
    End Sub
End Module
