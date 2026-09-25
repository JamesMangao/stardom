Imports System.Security.Cryptography
Imports System.Text

Module GenHash
    Sub Main(args As String())
        Dim passwords As String() = {
            "password123",
            "customer123",
            "admin123",
            "merchant123"
        }
        For Each p As String In passwords
            Console.WriteLine(p & " => " & Hash(p))
        Next
    End Sub

    Function Hash(password As String) As String
        Dim salt(15) As Byte
        Using rng As New RNGCryptoServiceProvider()
            rng.GetBytes(salt)
        End Using
        Dim hashBytes As Byte() = Derive(password, salt)
        Return "PBKDF2$10000$" & Convert.ToBase64String(salt) & "$" & Convert.ToBase64String(hashBytes)
    End Function

    Function Derive(password As String, salt As Byte()) As Byte()
        Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256)
            Return pbkdf2.GetBytes(32)
        End Using
    End Function
End Module