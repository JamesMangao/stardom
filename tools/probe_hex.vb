Imports System.Drawing

Module ProbeHex
    Sub Main()
        Dim c As Color = Color.Red
        Dim h As String = HexColor(c)
        System.Console.WriteLine(h)
    End Sub

    Function HexColor(c As Color) As String
        Return "#" & c.R.ToString("X2") & c.G.ToString("X2") & c.B.ToString("X2")
    End Function
End Module
