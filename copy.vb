Imports System.Data.Odbc
Imports System.Text.RegularExpressions


Public Class frmCopy
    Private Structure fieldStruct
        Public fid As String
        Public label As String
        Public type As String
        Public parentFieldID As String
        Public unique As Boolean
        Public required As Boolean
        Public base_type As String
        Public decimal_places As Integer
    End Structure
    Private Const AppName = "QuNectCopy"
    Private destinationFieldNodes As Dictionary(Of String, fieldStruct)
    Private sourceFieldNodes As Dictionary(Of String, fieldStruct)
    Private rids As HashSet(Of String)
    Private keyfid As String
    Private uniqueExistingFieldValues As Dictionary(Of String, HashSet(Of String))
    Private uniqueNewFieldValues As Dictionary(Of String, HashSet(Of String))
    Private sourceLabelToFieldType As Dictionary(Of String, String)
    Private sourceFieldNames As New Dictionary(Of String, Integer)
    Private destinationLabelsToFids As Dictionary(Of String, String)
    Private sourceLabelsToFids As Dictionary(Of String, String)
    Private isBooleanTrue As Regex = New Regex("y|tr|c|[1-9]", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
    Private Class qdbVersion
        Public year As Integer
        Public major As Integer
        Public minor As Integer
    End Class
    Private qdbVer As qdbVersion = New qdbVersion
    Enum mapping
        source
        destination
    End Enum
    Enum filter
        source
        booleanOperator
        criteria
    End Enum
    Enum comparisonResult
        equal
        notEqual
        greater
        less
        null
    End Enum
    Enum errorTabs
        missing
        conversions
        required
        unique
        malformed
    End Enum
    Enum tableType
        source
        destination
        sourceCatalog
    End Enum
    Public sourceOrDestination As tableType
    Private Sub restore_Load(sender As Object, e As EventArgs) Handles Me.Load
        Text = "QuNect Copy 1.0.0.9" ' & ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString
        txtUsername.Text = GetSetting(AppName, "Credentials", "username")
        txtPassword.Text = GetSetting(AppName, "Credentials", "password")
        txtServer.Text = GetSetting(AppName, "Credentials", "server", "www.quickbase.com")
        txtAppToken.Text = GetSetting(AppName, "Credentials", "apptoken", "b2fr52jcykx3tnbwj8s74b8ed55b")
        lblCatalog.Text = GetSetting(AppName, "config", "sourcecatalog", "")
        lblCatalog.Tag = GetSetting(AppName, "config", "sourcecatalogdbid", "")
        lblSourceTable.Text = GetSetting(AppName, "config", "sourcetable", "")
        lblDestinationTable.Text = GetSetting(AppName, "config", "destinationtable", "")

        Dim detectProxySetting As String = GetSetting(AppName, "Credentials", "detectproxysettings", "0")
        If detectProxySetting = "1" Then
            ckbDetectProxy.Checked = True
        Else
            ckbDetectProxy.Checked = False
        End If

        Dim myBuildInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)
        If lblCatalog.Text = "" Then
            btnDestination.Visible = False
            btnSource.Visible = False
            dgMapping.Visible = False
        End If


    End Sub
    Private Sub lblCatalog_TextChanged(sender As Object, e As EventArgs) Handles lblCatalog.TextChanged
        SaveSetting(AppName, "config", "sourcecatalog", lblCatalog.Text)
        If Not lblCatalog.Tag Is Nothing Then
            SaveSetting(AppName, "config", "sourcecatalogdbid", lblCatalog.Tag)
        End If
    End Sub
    Private Sub txtServer_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtServer.TextChanged
        SaveSetting(AppName, "Credentials", "server", txtServer.Text)
    End Sub
    Private Sub ckbDetectProxy_CheckStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ckbDetectProxy.CheckStateChanged
        If ckbDetectProxy.Checked Then
            SaveSetting(AppName, "Credentials", "detectproxysettings", "1")
        Else
            SaveSetting(AppName, "Credentials", "detectproxysettings", "0")
        End If
    End Sub
    Private Sub txtAppToken_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAppToken.TextChanged
        SaveSetting(AppName, "Credentials", "apptoken", txtAppToken.Text)
    End Sub
    Private Sub lblDestinationTable_TextChanged(sender As Object, e As EventArgs) Handles lblDestinationTable.TextChanged
        SaveSetting(AppName, "config", "destinationtable", lblDestinationTable.Text)
        btnImport.Visible = False
    End Sub

    Private Sub lblSourceTable_TextChanged(sender As Object, e As EventArgs) Handles lblSourceTable.TextChanged
        SaveSetting(AppName, "config", "sourcetable", lblSourceTable.Text)
        btnImport.Visible = False
    End Sub
    Private Sub txtUsername_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUsername.TextChanged
        SaveSetting(AppName, "Credentials", "username", txtUsername.Text)
    End Sub

    Private Sub txtPassword_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPassword.TextChanged
        SaveSetting(AppName, "Credentials", "password", txtPassword.Text)
    End Sub

    Private Function getConnectionString(usefids As Boolean, useAppDBID As Boolean) As String

        getConnectionString = "Driver={QuNect ODBC for QuickBase};FIELDNAMECHARACTERS=all;file=binary;uid=" & txtUsername.Text & ";pwd=" & txtPassword.Text & ";QUICKBASESERVER=" & txtServer.Text & ";APPTOKEN=" & txtAppToken.Text
        If usefids Then
            getConnectionString &= ";USEFIDS=1"
        End If
        If useAppDBID Then
            getConnectionString &= ";APPID=" & lblCatalog.Tag
        End If
    End Function

    Private Function getHashSetofFieldValues(dbid As String, fid As String) As HashSet(Of String)
        Dim strSQL As String = "SELECT fid" & fid & " FROM " & dbid
        getHashSetofFieldValues = New HashSet(Of String)
        Dim connectionString As String = getConnectionString(True, False)
        Using quNectConn As OdbcConnection = getquNectConn(connectionString)
            Using quNectCmd As OdbcCommand = New OdbcCommand(strSQL, quNectConn)
                Dim dr As OdbcDataReader
                Try
                    dr = quNectCmd.ExecuteReader()
                    If Not dr.HasRows Then
                        Exit Function
                    End If
                Catch excpt As Exception
                    Exit Function
                End Try
                While (dr.Read())
                    getHashSetofFieldValues.Add(dr.GetValue(0))
                End While
            End Using
        End Using
    End Function
    Private Function copyTable(checkForErrorsOnly As Boolean, previewOnly As Boolean) As Boolean
        Dim destinationFields As New ArrayList
        Dim sourceFields As New ArrayList
        Dim fidsForImport As New HashSet(Of String)
        Dim strSQL As String = "INSERT INTO """ & lblDestinationTable.Text & """ (fid"
        For i As Integer = 0 To dgMapping.Rows.Count - 1
            Dim destComboBoxCell As DataGridViewComboBoxCell = DirectCast(dgMapping.Rows(i).Cells(mapping.destination), System.Windows.Forms.DataGridViewComboBoxCell)
            If destComboBoxCell.Value Is Nothing Then Continue For
            Dim destDDIndex = destComboBoxCell.Items.IndexOf(destComboBoxCell.Value)
            If destDDIndex = 0 Then Continue For

            Dim fieldNode As fieldStruct
            fieldNode = sourceFieldNodes(sourceLabelsToFids(dgMapping.Rows(i).Cells(mapping.source).Value))
            sourceFields.Add(fieldNode.fid)
            fieldNode = destinationFieldNodes(destinationLabelsToFids(destComboBoxCell.Value))
            If fidsForImport.Contains(fieldNode.fid) Then
                MsgBox("You cannot import two different columns into the same field: " & destComboBoxCell.Value, MsgBoxStyle.OkOnly, AppName)
                Return False
            End If
            fidsForImport.Add(fieldNode.fid)
            destinationFields.Add(fieldNode.fid)
        Next
        Try
            strSQL &= String.Join(", fid", destinationFields.ToArray) & ") SELECT fid" & String.Join(", fid", sourceFields.ToArray) & " FROM """ & lblSourceTable.Text & """"
            Using quNectConn As OdbcConnection = getquNectConn(getConnectionString(True, False))
                Using command As OdbcCommand = New OdbcCommand(strSQL, quNectConn)
                    Dim i As Integer = command.ExecuteNonQuery()
                    Dim msg As String = i & " records were copied."
                    If i = -1 Then msg = "Success!"
                    MsgBox(msg)
                End Using
            End Using
        Catch ex As Exception
            MsgBox("Could not copy because " & ex.Message)
        End Try

        Return True

    End Function
    Private Function getquNectConn(connectionString As String) As OdbcConnection

        Dim quNectConn As OdbcConnection = New OdbcConnection(connectionString)
        Try
            quNectConn.Open()
        Catch excpt As Exception
            Me.Cursor = Cursors.Default
            If excpt.Message.StartsWith("Error [IM003]") Or excpt.Message.Contains("Data source name Not found") Then
                MsgBox("Please install QuNect ODBC For QuickBase from http://qunect.com/download/QuNect.exe and try again.")
            Else
                MsgBox(excpt.Message.Substring(13))
            End If
            Return Nothing
            Exit Function
        End Try

        Dim ver As String = quNectConn.ServerVersion
        Dim m As Match = Regex.Match(ver, "\d+\.(\d+)\.(\d+)\.(\d+)")
        qdbVer.year = CInt(m.Groups(1).Value)
        qdbVer.major = CInt(m.Groups(2).Value)
        qdbVer.minor = CInt(m.Groups(3).Value)
        If (qdbVer.major < 7) Or (qdbVer.major = 7 And qdbVer.minor < 11) Then
            MsgBox("You are running the " & ver & " version of QuNect ODBC for QuickBase. Please install the latest version from http://qunect.com/download/QuNect.exe")
            quNectConn.Close()
            Me.Cursor = Cursors.Default
            Return Nothing
            Exit Function
        End If
        Return quNectConn
    End Function
    Private Function showErrors(previewOnly As Boolean, ByRef conversionErrors As String, ByRef improperlyFormattedLines As String, ByRef uniqueFieldErrors As String, ByRef requiredFieldsErrors As String, ByRef missingRIDs As String) As Boolean
        If conversionErrors <> "" Or improperlyFormattedLines <> "" Or uniqueFieldErrors <> "" Or requiredFieldsErrors <> "" Or missingRIDs <> "" Then
            frmErr.lblMalformed.Text = improperlyFormattedLines
            frmErr.lblUnique.Text = uniqueFieldErrors
            frmErr.lblRequired.Text = requiredFieldsErrors
            frmErr.lblConversions.Text = conversionErrors
            frmErr.lblMissing.Text = missingRIDs
            If improperlyFormattedLines <> "" Then
                frmErr.TabControlErrors.SelectedIndex = errorTabs.malformed
                frmErr.TabMalformed.Text = "*Malformed lines"
            Else
                frmErr.TabMalformed.Text = "Malformed lines"
            End If
            If uniqueFieldErrors <> "" Then
                frmErr.TabControlErrors.SelectedIndex = errorTabs.unique
                frmErr.TabUnique.Text = "*Unique"
            Else
                frmErr.TabUnique.Text = "Unique"
            End If
            If requiredFieldsErrors <> "" Then
                frmErr.TabControlErrors.SelectedIndex = errorTabs.required
                frmErr.TabRequired.Text = "*Required"
            Else
                frmErr.TabRequired.Text = "Required"
            End If
            If conversionErrors <> "" Then
                frmErr.TabControlErrors.SelectedIndex = errorTabs.conversions
                frmErr.TabConversions.Text = "*Conversion"
            Else
                frmErr.TabConversions.Text = "Conversion"
            End If
            If missingRIDs <> "" Then
                frmErr.TabControlErrors.SelectedIndex = errorTabs.missing
                frmErr.TabMissing.Text = "*Missing Record ID#s"
            Else
                frmErr.TabMissing.Text = "Missing Record ID#s"
            End If
            If previewOnly Then
                frmErr.pnlButtons.Visible = False
            Else
                frmErr.pnlButtons.Visible = True
            End If
            Dim diagResult As DialogResult = frmErr.ShowDialog()
            If frmErr.rdbCancel.Checked Then
                Return False
            Else
                Return True
            End If
        End If
        Return True
    End Function
    Function setODBCParameter(val As String, fid As String, fieldLabel As String, command As OdbcCommand, ByRef fileLineCounter As Integer, checkForErrorsOnly As Boolean, ByRef conversionErrors As String) As Boolean
        Dim qdbType As OdbcType = command.Parameters("@fid" & fid).OdbcType
        Try
            Select Case qdbType
                Case OdbcType.Int
                    command.Parameters("@fid" & fid).Value = Convert.ToInt32(val)
                Case OdbcType.Double, OdbcType.Numeric
                    command.Parameters("@fid" & fid).Value = Convert.ToDouble(val)
                Case OdbcType.Date
                    command.Parameters("@fid" & fid).Value = Date.Parse(val)
                Case OdbcType.DateTime
                    command.Parameters("@fid" & fid).Value = DateTime.Parse(val)
                Case OdbcType.Time
                    command.Parameters("@fid" & fid).Value = TimeSpan.Parse(val)
                Case OdbcType.Bit
                    Dim match As Match = isBooleanTrue.Match(val)
                    If match.Success Then
                        command.Parameters("@fid" & fid).Value = True
                    Else
                        command.Parameters("@fid" & fid).Value = False
                    End If
                Case Else
                    command.Parameters("@fid" & fid).Value = val
            End Select
        Catch ex As Exception
            If checkForErrorsOnly And val <> "" Then
                conversionErrors &= vbCrLf & "line " & fileLineCounter + 1 & " '" & val & "' to " & qdbType.ToString() & " for field " & fieldLabel
                If conversionErrors.Length > 1000 Then
                    conversionErrors &= vbCrLf & "There may be additional errors beyond the ones above."
                    Return True
                End If
            End If
            If frmErr.rdbSkipRecords.Checked Then
                fileLineCounter += 1
                Return False
            End If
            If Not checkForErrorsOnly Then
                Select Case qdbType
                    Case OdbcType.Int, OdbcType.Double, OdbcType.Numeric
                        Dim nullDouble As Double
                        command.Parameters("@fid" & fid).Value = nullDouble
                    Case OdbcType.Date
                        Dim nulldate As Date
                        command.Parameters("@fid" & fid).Value = nulldate
                    Case OdbcType.DateTime
                        Dim nullDateTime As DateTime
                        command.Parameters("@fid" & fid).Value = nullDateTime
                    Case OdbcType.Time
                        Dim nullTimeSpan As TimeSpan
                        command.Parameters("@fid" & fid).Value = nullTimeSpan
                    Case OdbcType.Bit
                        command.Parameters("@fid" & fid).Value = False
                    Case Else
                        command.Parameters("@fid" & fid).Value = ""
                End Select
            End If
        End Try
        Return False
    End Function
    Private Function getODBCTypeFromQuickBaseFieldNode(fieldNode As fieldStruct) As OdbcType
        Select Case fieldNode.base_type
            Case "text"
                Return OdbcType.VarChar
            Case "float"
                If fieldNode.decimal_places > 0 Then
                    Return OdbcType.Numeric
                Else
                    Return OdbcType.Double
                End If

            Case "bool"
                Return OdbcType.Bit
            Case "int32"
                Return OdbcType.Int
            Case "int64"
                Select Case fieldNode.type
                    Case "timestamp"
                        Return OdbcType.DateTime
                    Case "timeofday"
                        Return OdbcType.Time
                    Case "duration"
                        Return OdbcType.Double
                    Case Else
                        Return OdbcType.Date
                End Select
            Case Else
                Return OdbcType.VarChar
        End Select
        Return OdbcType.VarChar
    End Function



    Private Sub btnSource_Click(sender As Object, e As EventArgs) Handles btnSource.Click
        sourceOrDestination = tableType.source
        listTables()
    End Sub


    Private Sub hideButtons()
        btnPreview.Visible = False
        btnImport.Visible = False
        dgMapping.Visible = False
    End Sub
    Private Sub btnDestination_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDestination.Click
        sourceOrDestination = tableType.destination
        listTables()
    End Sub
    Private Sub listTables()
        Me.Cursor = Cursors.WaitCursor
        Dim connectionString As String = getConnectionString(False, False)
        If sourceOrDestination = tableType.source Then
            connectionString = getConnectionString(False, True)
        End If
        Try
            Using quNectConn As OdbcConnection = getquNectConn(connectionString)
                Dim tables As DataTable = quNectConn.GetSchema("Tables")
                Dim views As DataTable = quNectConn.GetSchema("Views")
                tables.Merge(views)
                listTablesFromGetSchema(tables)
                quNectConn.Close()
                quNectConn.Dispose()
            End Using
        Catch ex As Exception
            MsgBox("Could not list tables because " & ex.Message)
        End Try
    End Sub
    Sub listCatalogs()
        Me.Cursor = Cursors.WaitCursor
        Dim connectionString As String = getConnectionString(False, False)
        Try
            frmTableChooser.tvAppsTables.BeginUpdate()
            frmTableChooser.tvAppsTables.Nodes.Clear()
            Using quNectConn As OdbcConnection = getquNectConn(connectionString)
                Using quNectCmd = New OdbcCommand("SELECT * FROM CATALOGS", quNectConn)
                    Dim dr As OdbcDataReader = quNectCmd.ExecuteReader()
                    While (dr.Read())
                        Dim applicationName As String = dr.GetString(0)
                        Dim appDBID As String = dr.GetString(4)
                        Dim appNode As TreeNode = frmTableChooser.tvAppsTables.Nodes.Add(applicationName)
                        appNode.Tag = appDBID
                    End While
                End Using
            End Using

        Catch ex As Exception

        Finally
            frmTableChooser.tvAppsTables.EndUpdate()
            frmTableChooser.Show()
            Me.Cursor = Cursors.Default
        End Try

    End Sub
    Sub listTablesFromGetSchema(tables As DataTable)
        frmTableChooser.tvAppsTables.BeginUpdate()
        frmTableChooser.tvAppsTables.Nodes.Clear()
        frmTableChooser.tvAppsTables.ShowNodeToolTips = True
        Dim dbName As String
        Dim applicationName As String = ""
        Dim prevAppName As String = ""
        Dim dbid As String
        pb.Value = 0
        pb.Visible = True
        pb.Maximum = tables.Rows.Count
        Dim getDBIDfromdbName As New Regex("([a-z0-9~]+)$")


        For i = 0 To tables.Rows.Count - 1
            pb.Value = i
            Application.DoEvents()
            dbName = tables.Rows(i)(2)
            applicationName = tables.Rows(i)(0)
            Dim dbidMatch As Match = getDBIDfromdbName.Match(dbName)
            dbid = dbidMatch.Value
            If applicationName <> prevAppName Then

                Dim appNode As TreeNode = frmTableChooser.tvAppsTables.Nodes.Add(applicationName)
                appNode.Tag = dbid
                prevAppName = applicationName
            End If
            Dim tableName As String = dbName
            If applicationName.Length And dbName.Length > applicationName.Length Then
                tableName = dbName.Substring(applicationName.Length + 2)
            End If
            If applicationName.Length Then
                Dim tableNode As TreeNode = frmTableChooser.tvAppsTables.Nodes(frmTableChooser.tvAppsTables.Nodes.Count - 1).Nodes.Add(tableName)
                tableNode.Tag = dbid
            Else
                Dim tableNode As TreeNode = frmTableChooser.tvAppsTables.Nodes.Add(tableName)
                tableNode.Tag = dbid
            End If

        Next

        pb.Visible = False
        frmTableChooser.tvAppsTables.EndUpdate()
        pb.Value = 0
        btnImport.Visible = True
        lblDestinationTable.Visible = True
        frmTableChooser.Show()
        Me.Cursor = Cursors.Default
    End Sub
    Sub listFields(sourceDBID As String, destinationDBID As String)
        sourceLabelToFieldType = New Dictionary(Of String, String)
        destinationLabelsToFids = New Dictionary(Of String, String)
        sourceLabelsToFids = New Dictionary(Of String, String)
        Dim destinationFIDtoLabel As New Dictionary(Of String, String)
        destinationFieldNodes = New Dictionary(Of String, fieldStruct)
        sourceFieldNodes = New Dictionary(Of String, fieldStruct)

        Dim connectionString As String = getConnectionString(False, True)
        Try
            Using connection As OdbcConnection = getquNectConn(connectionString)
                If connection Is Nothing Then Exit Sub
                Dim strSQL As String = "SELECT label, fid, field_type, parentFieldID, ""unique"", required, ""key"", base_type, decimal_places  FROM """ & destinationDBID & "~fields"" WHERE (mode = '' and role = '') or fid = '3'"

                Using quNectCmd As OdbcCommand = New OdbcCommand(strSQL, connection)
                    Dim dr As OdbcDataReader
                    Try
                        dr = quNectCmd.ExecuteReader()
                    Catch excpt As Exception
                        quNectCmd.Dispose()
                        Exit Sub
                    End Try
                    If Not dr.HasRows Then
                        Exit Sub
                    End If


                    'Loop through all of the fields in the schema.
                    DirectCast(dgMapping.Columns(mapping.destination), System.Windows.Forms.DataGridViewComboBoxColumn).Items.Clear()

                    DirectCast(dgMapping.Columns(mapping.destination), System.Windows.Forms.DataGridViewComboBoxColumn).Items.Add("")
                    While (dr.Read())
                        Dim field As New fieldStruct
                        field.label = dr.GetString(0)
                        field.fid = dr.GetString(1)
                        field.type = dr.GetString(2)
                        field.parentFieldID = dr.GetString(3)
                        field.unique = dr.GetBoolean(4)
                        field.required = dr.GetBoolean(5)
                        destinationFIDtoLabel.Add(field.fid, field.label)
                        If field.parentFieldID <> "" Then
                            field.label = destinationFIDtoLabel(field.parentFieldID) & ": " & field.label
                        End If
                        If dr.GetBoolean(6) Then
                            keyfid = field.fid
                        End If
                        field.base_type = dr.GetString(7)
                        field.decimal_places = 0
                        If Not IsDBNull(dr(8)) Then
                            field.decimal_places = dr.GetInt32(8)
                        End If
                        destinationFieldNodes.Add(field.fid, field)
                        destinationLabelsToFids.Add(field.label, field.fid)
                        DirectCast(dgMapping.Columns(mapping.destination), System.Windows.Forms.DataGridViewComboBoxColumn).Items.Add(field.label)
                    End While
                    dr.Close()
                    quNectCmd.Dispose()
                End Using
                'here we need to open the source and get the field names
                Dim sourceFIDtoLabel As New Dictionary(Of String, String)
                sourceFieldNames.Clear()



                strSQL = "SELECT label, fid, field_type, parentFieldID, ""unique"", required, ""key"", base_type, decimal_places  FROM """ & sourceDBID & "~fields"""

                Using quNectCmd As OdbcCommand = New OdbcCommand(strSQL, connection)
                    Dim dr As OdbcDataReader
                    Try
                        dr = quNectCmd.ExecuteReader()
                    Catch excpt As Exception
                        quNectCmd.Dispose()
                        Exit Sub
                    End Try
                    If Not dr.HasRows Then
                        Exit Sub
                    End If

                    dgMapping.Rows.Clear()
                    Dim i As Integer = 0
                    While (dr.Read())
                        Dim field As New fieldStruct
                        field.label = dr.GetString(0)
                        field.fid = dr.GetString(1)
                        field.type = dr.GetString(2)
                        field.parentFieldID = dr.GetString(3)
                        field.unique = dr.GetBoolean(4)
                        field.required = dr.GetBoolean(5)
                        sourceFIDtoLabel.Add(field.fid, field.label)
                        If field.parentFieldID <> "" Then
                            field.label = sourceFIDtoLabel(field.parentFieldID) & ": " & field.label
                        End If
                        If dr.GetBoolean(6) Then
                            keyfid = field.fid
                        End If
                        field.base_type = dr.GetString(7)
                        field.decimal_places = 0
                        If Not IsDBNull(dr(8)) Then
                            field.decimal_places = dr.GetInt32(8)
                        End If
                        sourceFieldNodes.Add(field.fid, field)
                        sourceLabelsToFids.Add(field.label, field.fid)
                        sourceLabelToFieldType.Add(field.label, field.type)
                        dgMapping.Rows.Add(New String() {field.label})
                        sourceFieldNames.Add(field.label, i)
                        i += 1
                    End While
                    dr.Close()
                End Using
            End Using

            For Each row In dgMapping.Rows
                guessDestination(row.Cells(mapping.source).Value, row.Index)
            Next
            btnImport.Visible = True
            btnPreview.Visible = True
            dgMapping.Visible = True

            Me.Cursor = Cursors.Default
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
    Sub guessDestination(sourceFieldName As String, sourceFieldOrdinal As Integer)

        For Each field As KeyValuePair(Of String, fieldStruct) In destinationFieldNodes
            If field.Value.label = sourceFieldName AndAlso field.Value.type <> "address" Then
                DirectCast(dgMapping.Rows(sourceFieldOrdinal).Cells(mapping.destination), System.Windows.Forms.DataGridViewComboBoxCell).Value = sourceFieldName
                Exit For
            End If
        Next

    End Sub

    Private Sub btnPreview_Click(sender As Object, e As EventArgs) Handles btnPreview.Click
        copyTable(True, True)
    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        copyTable(False, False)
    End Sub

    Private Sub btnListFields_Click(sender As Object, e As EventArgs) Handles btnListFields.Click
        If lblSourceTable.Text = "" Then
            MsgBox("Please choose a file to import.", MsgBoxStyle.OkOnly, AppName)
            Me.Cursor = Cursors.Default
            Exit Sub
        ElseIf lblDestinationTable.Text = "" Then
            MsgBox("Please choose a table to import.", MsgBoxStyle.OkOnly, AppName)
            Me.Cursor = Cursors.Default
            Exit Sub
        End If
        Me.Cursor = Cursors.Default
        listFields(lblSourceTable.Text, lblDestinationTable.Text)
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub dgMapping_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgMapping.CellMouseClick
        If e.RowIndex > 0 AndAlso e.ColumnIndex = mapping.source Then
            If dgMapping.Rows(e.RowIndex).Cells(mapping.destination).Value = "" Then
                guessDestination(dgMapping.Rows(e.RowIndex).Cells(mapping.source).Value, e.RowIndex)
            Else
                dgMapping.Rows(e.RowIndex).Cells(mapping.destination).Value = ""
            End If

        End If
    End Sub

    Private Sub btnCatalog_Click(sender As Object, e As EventArgs) Handles btnCatalog.Click
        sourceOrDestination = tableType.sourceCatalog
        listCatalogs()
    End Sub


End Class


