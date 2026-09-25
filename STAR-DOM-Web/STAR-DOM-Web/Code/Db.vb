Imports System.Configuration
Imports MySql.Data.MySqlClient

Namespace STAR_DOM.Database

    ''' <summary>Core MySQL access for the web app. All SQL flows through here with parameters only.</summary>
    Public Module Db

        Public Function ConnString() As String
            Dim envUrl As String = Environment.GetEnvironmentVariable("MYSQL_URL")
            If Not String.IsNullOrEmpty(envUrl) Then
                Return envUrl
            End If

            Dim dbHost As String = Environment.GetEnvironmentVariable("DB_HOST")
            If Not String.IsNullOrEmpty(dbHost) Then
                Dim dbPort As String = If(Environment.GetEnvironmentVariable("DB_PORT"), "3306")
                Dim dbName As String = If(Environment.GetEnvironmentVariable("DB_NAME"), "stardom")
                Dim dbUser As String = If(Environment.GetEnvironmentVariable("DB_USER"), "root")
                Dim dbPass As String = If(Environment.GetEnvironmentVariable("DB_PASSWORD"), "")
                Dim sslMode As String = If(Environment.GetEnvironmentVariable("DB_SSLMODE"), "none")
                Return String.Format("Server={0};Port={1};Database={2};User Id={3};Password={4};SslMode={5};AllowPublicKeyRetrieval=True;CharSet=utf8mb4;Convert Zero Datetime=True;", dbHost, dbPort, dbName, dbUser, dbPass, sslMode)
            End If

            Dim csSetting = ConfigurationManager.ConnectionStrings("STAR_DOM")
            If csSetting IsNot Nothing Then
                Return csSetting.ConnectionString
            End If

            Return "Server=localhost;Port=3306;Database=stardom;User Id=root;Password=;SslMode=none;AllowPublicKeyRetrieval=True;CharSet=utf8mb4;Convert Zero Datetime=True;"
        End Function

        Public Function Ping() As Boolean
            Try
                Using conn As New MySqlConnection(ConnString())
                    conn.Open()
                    Return True
                End Using
            Catch
                Return False
            End Try
        End Function

        Public Function NewConnection() As MySqlConnection
            Return New MySqlConnection(ConnString())
        End Function

        Public Function OpenConnection() As MySqlConnection
            Dim c As MySqlConnection = NewConnection()
            c.Open()
            Return c
        End Function

        Public Function P(name As String, value As Object) As MySqlParameter
            If value Is Nothing Then value = DBNull.Value
            Return New MySqlParameter(name, value)
        End Function

        Public Function Exec(sql As String, ParamArray ps() As MySqlParameter) As Integer
            Using conn As MySqlConnection = OpenConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    If ps IsNot Nothing AndAlso ps.Length > 0 Then cmd.Parameters.AddRange(ps)
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Run an INSERT (or any statement) and return the last auto-increment id.
        ''' The id is read on the SAME connection, so it is never tainted by a pooled
        ''' connection's LAST_INSERT_ID() from another session.
        ''' </summary>
        Public Function ExecIdentity(sql As String, ParamArray ps() As MySqlParameter) As Integer
            Using conn As MySqlConnection = OpenConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    If ps IsNot Nothing AndAlso ps.Length > 0 Then cmd.Parameters.AddRange(ps)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim o As Object = cmd.ExecuteScalar()
                    If o Is Nothing OrElse o Is DBNull.Value Then Return 0
                    Return Convert.ToInt32(o)
                End Using
            End Using
        End Function

        Public Function ScalarInt(sql As String, ParamArray ps() As MySqlParameter) As Integer
            Dim o As Object = RawScalar(sql, ps)
            If o Is Nothing OrElse o Is DBNull.Value Then Return 0
            Return Convert.ToInt32(o)
        End Function

        Public Function ScalarStr(sql As String, ParamArray ps() As MySqlParameter) As String
            Dim o As Object = RawScalar(sql, ps)
            If o Is Nothing OrElse o Is DBNull.Value Then Return ""
            Return Convert.ToString(o)
        End Function

        Public Function ScalarDec(sql As String, ParamArray ps() As MySqlParameter) As Decimal
            Dim o As Object = RawScalar(sql, ps)
            If o Is Nothing OrElse o Is DBNull.Value Then Return 0D
            Return Convert.ToDecimal(o)
        End Function

        Public Function ScalarDate(sql As String, ParamArray ps() As MySqlParameter) As Date
            Dim o As Object = RawScalar(sql, ps)
            If o Is Nothing OrElse o Is DBNull.Value Then Return Date.MinValue
            Return Convert.ToDateTime(o)
        End Function

        Private Function RawScalar(sql As String, ps() As MySqlParameter) As Object
            Using conn As MySqlConnection = OpenConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    If ps IsNot Nothing AndAlso ps.Length > 0 Then cmd.Parameters.AddRange(ps)
                    Return cmd.ExecuteScalar()
                End Using
            End Using
        End Function

        Public Function Query(sql As String, ParamArray ps() As MySqlParameter) As DataTable
            Dim dt As New DataTable()
            Using conn As MySqlConnection = OpenConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    If ps IsNot Nothing AndAlso ps.Length > 0 Then cmd.Parameters.AddRange(ps)
                    Using da As New MySqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
            End Using
            Return dt
        End Function

        Public Function Rows(sql As String, ParamArray ps() As MySqlParameter) As List(Of DataRow)
            Dim dt As DataTable = Query(sql, ps)
            Return dt.Rows.Cast(Of DataRow)().ToList()
        End Function

        ''' <summary>Log a technical error to the AppErrors table (created on demand).</summary>
        Public Sub LogError(context As String, ex As Exception)
            Try
                Exec("CREATE TABLE IF NOT EXISTS AppErrors (" &
                     "Id INT AUTO_INCREMENT PRIMARY KEY, Context VARCHAR(100), " &
                     "Message TEXT, StackTrace TEXT, CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP, " &
                     "INDEX IDX_AppErrors_Created (CreatedAt))")
                Exec("INSERT INTO AppErrors (Context, Message, StackTrace) VALUES (@c, @m, @s)",
                     P("@c", context), P("@m", ex.Message), P("@s", ex.ToString()))
            Catch
                ' never let logging break the app
            End Try
        End Sub

        ''' <summary>Trim old error-log rows so AppErrors never grows without bound.</summary>
        Public Sub PruneErrors(Optional days As Integer = 30)
            Try
                Exec("DELETE FROM AppErrors WHERE CreatedAt < DATE_SUB(NOW(), INTERVAL @d DAY)", P("@d", days))
            Catch
                ' best effort; never break startup
            End Try
        End Sub

    End Module

    ''' <summary>Safe typed readers over a DataRow (handles DBNull everywhere).</summary>
    Public Module RowReader

        Public Function AsInt(row As DataRow, col As String, Optional def As Integer = 0) As Integer
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return def
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return def
            Try
                Return Convert.ToInt32(o)
            Catch
                Return def
            End Try
        End Function

        Public Function AsStr(row As DataRow, col As String, Optional def As String = "") As String
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return def
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return def
            Return Convert.ToString(o)
        End Function

        Public Function AsDec(row As DataRow, col As String, Optional def As Decimal = 0D) As Decimal
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return def
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return def
            Try
                Return Convert.ToDecimal(o)
            Catch
                Return def
            End Try
        End Function

        Public Function AsDate(row As DataRow, col As String) As Date
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return Date.MinValue
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return Date.MinValue
            Try
                Return Convert.ToDateTime(o)
            Catch
                Return Date.MinValue
            End Try
        End Function

        Public Function AsNullableDate(row As DataRow, col As String) As Date?
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return Nothing
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return Nothing
            Try
                Return Convert.ToDateTime(o)
            Catch
                Return Nothing
            End Try
        End Function

        Public Function AsNullableInt(row As DataRow, col As String) As Integer?
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return Nothing
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return Nothing
            Try
                Return Convert.ToInt32(o)
            Catch
                Return Nothing
            End Try
        End Function

        Public Function AsNullableDec(row As DataRow, col As String) As Decimal?
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return Nothing
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return Nothing
            Try
                Return Convert.ToDecimal(o)
            Catch
                Return Nothing
            End Try
        End Function

        Public Function AsBool(row As DataRow, col As String) As Boolean
            If row Is Nothing OrElse Not row.Table.Columns.Contains(col) Then Return False
            Dim o As Object = row(col)
            If o Is Nothing OrElse o Is DBNull.Value Then Return False
            Try
                If TypeOf o Is Boolean Then Return CBool(o)
                Return Convert.ToInt32(o) <> 0
            Catch
                Return False
            End Try
        End Function

    End Module

End Namespace
