Imports System.ComponentModel
Imports System.Data.Odbc
Imports System.Text.RegularExpressions


Public Class frmCopy
    Private Class qdbField
        Public Sub New(_fid As String, _label As String, _type As String, _parentFieldID As String, _unique As Boolean, _required As Boolean, _base_type As String, _decimal_places As Integer)
            fid = _fid
            label = _label
            parentFieldID = _parentFieldID
            unique = _unique
            required = _required
            base_type = _base_type
            decimal_places = _decimal_places
        End Sub
        Public fid As String
        Public label As String
        Public type As String
        Public parentFieldID As String
        Public unique As Boolean
        Public required As Boolean
        Public base_type As String
        Public decimal_places As Integer
        Overrides Function toString() As String
            Return label
        End Function
    End Class
    Private Const AppName = "QuNectCopy"
    Private appVersion As String
    Private destinationFieldNodes As Dictionary(Of String, qdbField)
    Private sourceFieldNodes As Dictionary(Of String, qdbField)
    Private qdbConnections As New Dictionary(Of String, OdbcConnection)
    Private rids As HashSet(Of String)
    Private keyfid As String
    Private uniqueExistingFieldValues As Dictionary(Of String, HashSet(Of String))
    Private uniqueNewFieldValues As Dictionary(Of String, HashSet(Of String))
    Private sourceLabelToFieldType As Dictionary(Of String, String)
    Private sourceFieldNames As New Dictionary(Of String, Integer)
    Private destinationLabelsToFids As Dictionary(Of String, String)
    Private sourceLabelsToFids As Dictionary(Of String, String)
    Private isBooleanTrue As Regex = New Regex("y|tr|c|[1-9]", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
    Private copyFinished As Boolean = False
    Dim copyCount As Integer
    Dim existingCount As Integer
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
    Enum tableType
        source
        destination
        sourceCatalog
    End Enum
    Public sourceOrDestination As tableType
    Private Sub restore_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim myBuildInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)
        appVersion = myBuildInfo.ProductVersion
        Text = AppName & " " & appVersion
        txtUsername.Text = GetSetting(AppName, "Credentials", "username")
        cmbPassword.SelectedIndex = CInt(GetSetting(AppName, "Credentials", "passwordOrToken", "0"))
        txtPassword.Text = GetSetting(AppName, "Credentials", "password")
        txtServer.Text = GetSetting(AppName, "Credentials", "server", "")
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

        showHideControls()
    End Sub
    Private Sub lblCatalog_TextChanged(sender As Object, e As EventArgs) Handles lblCatalog.TextChanged
        SaveSetting(AppName, "config", "sourcecatalog", lblCatalog.Text)
        If Not lblCatalog.Tag Is Nothing Then
            SaveSetting(AppName, "config", "sourcecatalogdbid", lblCatalog.Tag)
        End If
        showHideControls()
    End Sub
    Sub showHideControls()
        cmbPassword.Visible = txtUsername.Text.Length > 0
        txtPassword.Visible = cmbPassword.Visible And cmbPassword.SelectedIndex <> 0
        txtServer.Visible = txtPassword.Visible And txtPassword.Text.Length > 0
        lblServer.Visible = txtServer.Visible
        lblAppToken.Visible = cmbPassword.Visible And cmbPassword.SelectedIndex = 1
        btnAppToken.Visible = lblAppToken.Visible
        txtAppToken.Visible = lblAppToken.Visible
        btnUserToken.Visible = cmbPassword.Visible And cmbPassword.SelectedIndex = 2
        btnListFields.Visible = False
        btnSource.Visible = False
        btnDestination.Visible = False
        btnImport.Visible = False
        dgMapping.Visible = False

        If lblCatalog.Text <> "" Then
            btnSource.Visible = True
        Else
            Exit Sub
        End If
        If lblSourceTable.Text <> "" Then
            btnDestination.Visible = True
        Else
            Exit Sub
        End If
        If lblDestinationTable.Text <> "" Then
            btnListFields.Visible = True
        Else
            Exit Sub
        End If
        If lblDestinationTable.Text <> "" Then
            btnListFields.Visible = True
        Else
            Exit Sub
        End If
        If dgMapping.RowCount > 0 Then
            dgMapping.Visible = True
            btnImport.Visible = True
        End If
    End Sub
    Private Sub txtServer_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtServer.TextChanged
        SaveSetting(AppName, "Credentials", "server", txtServer.Text)
        showHideControls()
    End Sub
    Private Sub ckbDetectProxy_CheckStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ckbDetectProxy.CheckStateChanged
        If ckbDetectProxy.Checked Then
            SaveSetting(AppName, "Credentials", "detectproxysettings", "1")
        Else
            SaveSetting(AppName, "Credentials", "detectproxysettings", "0")
        End If
        showHideControls()
    End Sub
    Private Sub txtAppToken_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAppToken.TextChanged
        SaveSetting(AppName, "Credentials", "apptoken", txtAppToken.Text)
        showHideControls()
    End Sub
    Private Sub lblDestinationTable_TextChanged(sender As Object, e As EventArgs) Handles lblDestinationTable.TextChanged
        SaveSetting(AppName, "config", "destinationtable", lblDestinationTable.Text)
        showHideControls()
    End Sub

    Private Sub lblSourceTable_TextChanged(sender As Object, e As EventArgs) Handles lblSourceTable.TextChanged
        SaveSetting(AppName, "config", "sourcetable", lblSourceTable.Text)
        showHideControls()
    End Sub
    Private Sub txtUsername_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUsername.TextChanged
        SaveSetting(AppName, "Credentials", "username", txtUsername.Text)
        showHideControls()
        showHideControls()
    End Sub

    Private Sub txtPassword_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPassword.TextChanged
        SaveSetting(AppName, "Credentials", "password", txtPassword.Text)
        showHideControls()
        showHideControls()
    End Sub

    Private Function getConnectionString(usefids As Boolean, useAppDBID As Boolean) As String

        getConnectionString = "Driver={QuNect ODBC For QuickBase};FIELDNAMECHARACTERS=all;ALLREVISIONS=ALL;uid=" & txtUsername.Text & ";pwd=" & txtPassword.Text & ";QUICKBASESERVER=" & txtServer.Text & ";APPTOKEN=" & txtAppToken.Text
        If usefids Then
            getConnectionString &= ";USEFIDS=1"
        End If
        If useAppDBID Then
            getConnectionString &= ";APPID=" & lblCatalog.Tag
        End If
        If cmbPassword.SelectedIndex = 0 Then
            cmbPassword.Focus()
            Throw New System.Exception("Please indicate whether you are Using a password Or a user token.")
            Return ""
        ElseIf cmbPassword.SelectedIndex = 1 Then
            getConnectionString &= ";PWDISPASSWORD=1"
        Else
            getConnectionString &= ";PWDISPASSWORD=0"
        End If
    End Function
    Function countRecords(sql As String, connection As OdbcConnection) As Integer
        Using command As OdbcCommand = New OdbcCommand(sql, connection)
            Dim dr As OdbcDataReader
            Try
                dr = command.ExecuteReader()
            Catch excpt As Exception
                Throw New System.Exception("Could Not count records " & excpt.Message)
            End Try
            If Not dr.HasRows Then
                Throw New System.Exception("Counting records failed. " & sql)
            Else
                Return dr(0)
            End If
        End Using
    End Function
    Private Function copyTable() As Boolean

        Dim destinationFields As New ArrayList
        Dim sourceFields As New ArrayList
        Dim fidsForImport As New HashSet(Of String)
        Dim strSQL As String = "INSERT INTO """ & lblDestinationTable.Text & """ (fid"
        For i As Integer = 0 To dgMapping.Rows.Count - 1
            Dim destComboBoxCell As DataGridViewComboBoxCell = DirectCast(dgMapping.Rows(i).Cells(mapping.destination), System.Windows.Forms.DataGridViewComboBoxCell)
            If destComboBoxCell.Value Is Nothing Then Continue For
            Dim destDDIndex = destComboBoxCell.Items.IndexOf(destComboBoxCell.Value)
            If destDDIndex = 0 Then Continue For

            Dim fieldNode As qdbField
            fieldNode = sourceFieldNodes(sourceLabelsToFids(dgMapping.Rows(i).Cells(mapping.source).Value))
            sourceFields.Add(fieldNode.fid)
            fieldNode = destinationFieldNodes(destinationLabelsToFids(destComboBoxCell.Value))
            If keyfid = "3" And fieldNode.fid = "3" Then
                Dim copyAnyway As MsgBoxResult = MsgBox("Copying into the key field " & fieldNode.label & " will update existing records without creating New records. Do you want To Continue?", MsgBoxStyle.YesNo)
                If copyAnyway = MsgBoxResult.No Then
                    VolatileWrite(copyFinished, True)
                    Return False
                End If
            End If
            If fidsForImport.Contains(fieldNode.fid) Then
                MsgBox("You cannot import two different columns into the same field: " & destComboBoxCell.Value, MsgBoxStyle.OkOnly, AppName)
                VolatileWrite(copyFinished, True)
                Return False
            End If
            fidsForImport.Add(fieldNode.fid)
            destinationFields.Add(fieldNode.fid)
        Next
        If fidsForImport.Count = 0 Then
            MsgBox("You must map at least one field from the source table to the destination table.", MsgBoxStyle.OkOnly, AppName)
            VolatileWrite(copyFinished, True)
            Return False
        End If
        Try
            Dim quNectConn As OdbcConnection = getquNectConn(getConnectionString(True, False))
            copyCount = countRecords("SELECT count(1) FROM """ & lblSourceTable.Text & """", quNectConn)
            If copyCount = 0 Then
                MsgBox("There are no records to copy.")
                VolatileWrite(copyFinished, True)
                Return False
            End If
            Dim tooMany As MsgBoxResult = MsgBox("You will be copying " & copyCount & " records from """ & lblSourceTable.Text & """ to """ & lblDestinationTable.Text & """. Do you want to continue?", MsgBoxStyle.YesNo)
            If tooMany = MsgBoxResult.No Then
                VolatileWrite(copyFinished, True)
                Return False
            End If
            existingCount = countRecords("SELECT count(1) FROM """ & lblDestinationTable.Text & """ WHERE fid4 = '" & txtUsername.Text & "'", quNectConn)

            Dim progressThread As System.Threading.Thread = New Threading.Thread(AddressOf showProgress)
            progressThread.Start()
            strSQL &= String.Join(", fid", destinationFields.ToArray) & ") Select fid" & String.Join(", fid", sourceFields.ToArray) & " FROM """ & lblSourceTable.Text & """"
            Using command As OdbcCommand = New OdbcCommand(strSQL, quNectConn)
                Dim i As Integer = command.ExecuteNonQuery()
                VolatileWrite(copyFinished, True)
                MsgBox("Copy complete. " & i & " records copied.")
            End Using
        Catch ex As Exception
            MsgBox("Could Not copy because " & ex.Message)
        End Try
        VolatileWrite(copyFinished, True)
        Return True

    End Function
    Private Function getquNectConn(connectionString As String) As OdbcConnection
        If qdbConnections.ContainsKey(connectionString) Then
            Return qdbConnections(connectionString)
        End If
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
        Me.Text = AppName & " " & appVersion & " with QuNect ODBC for QuickBase " & ver
        Dim m As Match = Regex.Match(ver, "\d+\.(\d+)\.(\d+)\.(\d+)")
        qdbVer.year = CInt(m.Groups(1).Value)
        qdbVer.major = CInt(m.Groups(2).Value)
        qdbVer.minor = CInt(m.Groups(3).Value)
        If (qdbVer.major < 7) Or (qdbVer.major = 7 And qdbVer.minor < 37) Then
            MsgBox("You are running the " & ver & " version of QuNect ODBC for QuickBase. Please install the latest version from http://qunect.com/download/QuNect.exe")
            quNectConn.Close()
            Me.Cursor = Cursors.Default
            Return Nothing
            Exit Function
        End If
        qdbConnections.Add(connectionString, quNectConn)
        Return quNectConn
    End Function

    Private Sub btnSource_Click(sender As Object, e As EventArgs) Handles btnSource.Click
        sourceOrDestination = tableType.source
        listTables()
    End Sub

    Private Sub btnDestination_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDestination.Click
        sourceOrDestination = tableType.destination
        listTables()
    End Sub
    Private Sub listTables()
        Me.Cursor = Cursors.WaitCursor
        Try
            Dim connectionString As String = getConnectionString(False, False)
            If sourceOrDestination = tableType.source Then
                connectionString = getConnectionString(False, True)
            End If
            Dim quNectConn As OdbcConnection = getquNectConn(connectionString)
            Dim tables As DataTable = quNectConn.GetSchema("Tables")
            Dim views As DataTable = quNectConn.GetSchema("Views")
            tables.Merge(views)
            listTablesFromGetSchema(tables)

        Catch ex As Exception
            MsgBox("Could not list tables because " & ex.Message)
        End Try
    End Sub
    Sub listCatalogs()
        Try
            Dim connectionString As String = getConnectionString(False, False)
            Dim quNectConn As OdbcConnection = getquNectConn(connectionString)
            Using quNectCmd = New OdbcCommand("SELECT * FROM CATALOGS", quNectConn)
                Dim dr As OdbcDataReader = quNectCmd.ExecuteReader()
                frmTableChooser.tvAppsTables.BeginUpdate()
                frmTableChooser.tvAppsTables.Nodes.Clear()
                While (dr.Read())
                    Dim applicationName As String = dr.GetString(0)
                    Dim appDBID As String = dr.GetString(2)
                    Dim appNode As TreeNode = frmTableChooser.tvAppsTables.Nodes.Add(applicationName)
                    appNode.Tag = appDBID
                End While
                frmTableChooser.tvAppsTables.EndUpdate()
            End Using
        Catch ex As Exception
            MsgBox("Could not list catalogs because " & ex.Message)
            Exit Sub
        End Try
        frmTableChooser.Show()
    End Sub
    Sub listTablesFromGetSchema(tables As DataTable)
        frmTableChooser.tvAppsTables.BeginUpdate()
        frmTableChooser.tvAppsTables.Nodes.Clear()
        frmTableChooser.tvAppsTables.ShowNodeToolTips = True
        Dim dbName As String
        Dim applicationName As String = ""
        Dim prevAppName As String = ""
        Dim dbid As String
        pb.Minimum = 0
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
        destinationFieldNodes = New Dictionary(Of String, qdbField)
        sourceFieldNodes = New Dictionary(Of String, qdbField)

        Try
            Dim connectionString As String = getConnectionString(False, True)
            Dim connection As OdbcConnection = getquNectConn(connectionString)
            If connection Is Nothing Then Exit Sub
            Dim strSQL As String = "SELECT label, fid, field_type, parentFieldID, ""isunique"", required, ""iskey"", base_type, decimal_places  FROM """ & destinationDBID & "~fields"" WHERE (mode = '' and role = '') or fid = '3'"

            Using quNectCmd As OdbcCommand = New OdbcCommand(strSQL, connection)
                Dim dr As OdbcDataReader
                Try
                    dr = quNectCmd.ExecuteReader()
                Catch excpt As Exception
                    Throw New System.Exception("Could not get field information for " & destinationDBID & vbCrLf & excpt.Message)
                End Try
                If Not dr.HasRows Then
                    Exit Sub
                End If


                'Loop through all of the fields in the schema.
                DirectCast(dgMapping.Columns(mapping.destination), System.Windows.Forms.DataGridViewComboBoxColumn).Items.Clear()

                DirectCast(dgMapping.Columns(mapping.destination), System.Windows.Forms.DataGridViewComboBoxColumn).Items.Add("")
                While (dr.Read())
                    Dim field As New qdbField(dr.GetString(1), dr.GetString(0), dr.GetString(2), dr.GetString(3), dr.GetBoolean(4), dr.GetBoolean(5), dr.GetString(7), 0)
                    destinationFIDtoLabel.Add(field.fid, field.label)
                    If field.parentFieldID <> "" Then
                        field.label = destinationFIDtoLabel(field.parentFieldID) & ": " & field.label
                    End If
                    If dr.GetBoolean(6) Then
                        keyfid = field.fid
                    End If
                    If Not IsDBNull(dr(8)) Then
                        field.decimal_places = dr.GetInt32(8)
                    End If
                    destinationFieldNodes.Add(field.fid, field)
                    destinationLabelsToFids.Add(field.label, field.fid)
                    DirectCast(dgMapping.Columns(mapping.destination), System.Windows.Forms.DataGridViewComboBoxColumn).Items.Add(field.label)
                End While
                dr.Close()
            End Using
            'here we need to open the source and get the field names
            Dim sourceFIDtoLabel As New Dictionary(Of String, String)
            sourceFieldNames.Clear()



            strSQL = "SELECT label, fid, field_type, parentFieldID, ""isunique"", required, ""iskey"", base_type, decimal_places  FROM """ & sourceDBID & "~fields"""

            Using quNectCmd As OdbcCommand = New OdbcCommand(strSQL, connection)
                Dim dr As OdbcDataReader
                Try
                    dr = quNectCmd.ExecuteReader()
                Catch excpt As Exception
                    Throw New System.Exception("Could not get field information for " & sourceDBID & vbCrLf & excpt.Message)
                End Try
                If Not dr.HasRows Then
                    Throw New System.Exception("Could not get field information for " & sourceDBID)
                End If

                dgMapping.Rows.Clear()
                Dim i As Integer = 0
                While (dr.Read())
                    Dim field As New qdbField(dr.GetString(1), dr.GetString(0), dr.GetString(2), dr.GetString(3), dr.GetBoolean(4), dr.GetBoolean(5), dr.GetString(7), 0)
                    sourceFIDtoLabel.Add(field.fid, field.label)
                    If field.parentFieldID <> "" Then
                        field.label = sourceFIDtoLabel(field.parentFieldID) & ": " & field.label
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


            For Each row In dgMapping.Rows
                guessDestination(row.Cells(mapping.source).Value, row.Index)
            Next
            btnImport.Visible = True

            dgMapping.Visible = True

            Me.Cursor = Cursors.Default
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
    Sub guessDestination(sourceFieldName As String, sourceFieldOrdinal As Integer)

        For Each field As KeyValuePair(Of String, qdbField) In destinationFieldNodes
            If field.Value.label = sourceFieldName AndAlso field.Value.type <> "address" AndAlso field.Value.fid <> "3" Then
                DirectCast(dgMapping.Rows(sourceFieldOrdinal).Cells(mapping.destination), System.Windows.Forms.DataGridViewComboBoxCell).Value = sourceFieldName
                Exit For
            End If
        Next

    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        VolatileWrite(copyFinished, False)
        pb.Visible = True
        Dim copyThread As System.Threading.Thread = New Threading.Thread(AddressOf copyTable)
        copyThread.Start()

    End Sub

    Private Sub btnListFields_Click(sender As Object, e As EventArgs) Handles btnListFields.Click
        If lblSourceTable.Text = "" Then
            MsgBox("Please choose a table to copy from.", MsgBoxStyle.OkOnly, AppName)
            Me.Cursor = Cursors.Default
            Exit Sub
        ElseIf lblDestinationTable.Text = "" Then
            MsgBox("Please choose a table to copy into.", MsgBoxStyle.OkOnly, AppName)
            Me.Cursor = Cursors.Default
            Exit Sub
        End If
        Me.Cursor = Cursors.WaitCursor
        Try
            listFields(lblSourceTable.Text, lblDestinationTable.Text)
        Catch excpt As Exception
            MsgBox(excpt.Message)
        End Try
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
        Me.Cursor = Cursors.WaitCursor
        sourceOrDestination = tableType.sourceCatalog
        listCatalogs()
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub showProgress()
        Try
            Dim quNectConn As OdbcConnection = New OdbcConnection(getConnectionString(True, False))
            quNectConn.Open()
            Dim pbValue As Integer
            While Not VolatileRead(copyFinished)
                Dim currentCount = countRecords("SELECT count(1) FROM """ & lblDestinationTable.Text & """ WHERE fid4 = '" & txtUsername.Text & "'", quNectConn)
                Debug.WriteLine(Now & currentCount.ToString)
                If currentCount > copyCount + existingCount Then
                    pbValue = copyCount + existingCount
                ElseIf currentCount < existingCount Then
                    pbValue = existingCount
                Else
                    pbValue = currentCount
                End If
                SetLabelProgress("Copying record " & (pbValue - existingCount) & " of " & copyCount)
                SetPBProgress(pbValue)
                Threading.Thread.Sleep(2000)
            End While
            SetLabelProgress("")
            SetPBProgress(copyCount + existingCount)
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub
    Delegate Sub LabelDelegate(progressMessage As String)
    Private Sub SetLabelProgress(ByVal progressMessage As String)

        ' InvokeRequired required compares the thread ID of the  
        ' calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If Me.lblProgress.InvokeRequired Or Me.pb.InvokeRequired Then
            Dim d As New LabelDelegate(AddressOf SetLabelProgress)
            Me.Invoke(d, New Object() {progressMessage})
        Else
            Me.lblProgress.Text = progressMessage
        End If
    End Sub
    Delegate Sub PBDelegate(numRecords As Integer)
    Private Sub SetPBProgress(ByVal numRecords As Integer)

        ' InvokeRequired required compares the thread ID of the  
        ' calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If Me.pb.InvokeRequired Then
            Dim d As New PBDelegate(AddressOf SetPBProgress)
            Me.Invoke(d, New Object() {numRecords})
        Else
            Me.pb.Maximum = existingCount + copyCount
            Me.pb.Minimum = existingCount
            Me.pb.Value = numRecords
        End If
    End Sub


    Private Sub frmCopy_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        VolatileWrite(copyFinished, True)
    End Sub

    Function VolatileRead(Of T)(ByRef Address As T) As T
        VolatileRead = Address
        Threading.Thread.MemoryBarrier()
    End Function

    Sub VolatileWrite(Of T)(ByRef Address As T, ByVal Value As T)
        Threading.Thread.MemoryBarrier()
        Address = Value
    End Sub

    Private Sub cmbPassword_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPassword.SelectedIndexChanged
        SaveSetting(AppName, "Credentials", "passwordOrToken", cmbPassword.SelectedIndex)
        showHideControls()
    End Sub
    Private Sub btnAppToken_Click(sender As Object, e As EventArgs) Handles btnAppToken.Click
        Process.Start("https://qunect.com/flash/AppToken.html")
    End Sub

    Private Sub btnUserToken_Click(sender As Object, e As EventArgs) Handles btnUserToken.Click
        Process.Start("https://qunect.com/flash/UserToken.html")
    End Sub
End Class


