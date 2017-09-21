<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCopy
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCopy))
        Me.dgMapping = New System.Windows.Forms.DataGridView()
        Me.Source = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Destination = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.btnSource = New System.Windows.Forms.Button()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.btnDestination = New System.Windows.Forms.Button()
        Me.pb = New System.Windows.Forms.ProgressBar()
        Me.lblAppToken = New System.Windows.Forms.Label()
        Me.txtAppToken = New System.Windows.Forms.TextBox()
        Me.lblServer = New System.Windows.Forms.Label()
        Me.txtServer = New System.Windows.Forms.TextBox()
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.lblUsername = New System.Windows.Forms.Label()
        Me.txtUsername = New System.Windows.Forms.TextBox()
        Me.lblDestinationTable = New System.Windows.Forms.Label()
        Me.lblSourceTable = New System.Windows.Forms.Label()
        Me.ckbDetectProxy = New System.Windows.Forms.CheckBox()
        Me.btnListFields = New System.Windows.Forms.Button()
        Me.btnPreview = New System.Windows.Forms.Button()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.lblMode = New System.Windows.Forms.Label()
        Me.btnCatalog = New System.Windows.Forms.Button()
        Me.lblCatalog = New System.Windows.Forms.Label()
        CType(Me.dgMapping, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgMapping
        '
        Me.dgMapping.AllowUserToAddRows = False
        Me.dgMapping.AllowUserToDeleteRows = False
        Me.dgMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgMapping.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Source, Me.Destination})
        Me.dgMapping.Location = New System.Drawing.Point(18, 241)
        Me.dgMapping.Name = "dgMapping"
        Me.dgMapping.Size = New System.Drawing.Size(505, 682)
        Me.dgMapping.TabIndex = 0
        '
        'Source
        '
        Me.Source.HeaderText = "Source"
        Me.Source.Name = "Source"
        Me.Source.ReadOnly = True
        Me.Source.Width = 200
        '
        'Destination
        '
        Me.Destination.HeaderText = "Destination"
        Me.Destination.Name = "Destination"
        Me.Destination.Width = 200
        '
        'btnSource
        '
        Me.btnSource.Location = New System.Drawing.Point(11, 145)
        Me.btnSource.Name = "btnSource"
        Me.btnSource.Size = New System.Drawing.Size(168, 27)
        Me.btnSource.TabIndex = 1
        Me.btnSource.Text = "Choose Source Table..."
        Me.btnSource.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        Me.btnImport.Location = New System.Drawing.Point(338, 208)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(185, 27)
        Me.btnImport.TabIndex = 3
        Me.btnImport.Text = "Import from Source to Destination"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'btnDestination
        '
        Me.btnDestination.Location = New System.Drawing.Point(12, 178)
        Me.btnDestination.Name = "btnDestination"
        Me.btnDestination.Size = New System.Drawing.Size(167, 23)
        Me.btnDestination.TabIndex = 4
        Me.btnDestination.Text = "Choose Destination Table..."
        Me.btnDestination.UseVisualStyleBackColor = True
        '
        'pb
        '
        Me.pb.Location = New System.Drawing.Point(286, 83)
        Me.pb.Maximum = 1000
        Me.pb.Name = "pb"
        Me.pb.Size = New System.Drawing.Size(237, 23)
        Me.pb.TabIndex = 33
        Me.pb.Visible = False
        '
        'lblAppToken
        '
        Me.lblAppToken.AutoSize = True
        Me.lblAppToken.Location = New System.Drawing.Point(15, 64)
        Me.lblAppToken.Name = "lblAppToken"
        Me.lblAppToken.Size = New System.Drawing.Size(148, 13)
        Me.lblAppToken.TabIndex = 30
        Me.lblAppToken.Text = "QuickBase Application Token"
        '
        'txtAppToken
        '
        Me.txtAppToken.Location = New System.Drawing.Point(12, 83)
        Me.txtAppToken.Name = "txtAppToken"
        Me.txtAppToken.Size = New System.Drawing.Size(258, 20)
        Me.txtAppToken.TabIndex = 29
        '
        'lblServer
        '
        Me.lblServer.AutoSize = True
        Me.lblServer.Location = New System.Drawing.Point(289, 11)
        Me.lblServer.Name = "lblServer"
        Me.lblServer.Size = New System.Drawing.Size(93, 13)
        Me.lblServer.TabIndex = 28
        Me.lblServer.Text = "QuickBase Server"
        '
        'txtServer
        '
        Me.txtServer.Location = New System.Drawing.Point(286, 30)
        Me.txtServer.Name = "txtServer"
        Me.txtServer.Size = New System.Drawing.Size(237, 20)
        Me.txtServer.TabIndex = 27
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New System.Drawing.Point(153, 11)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(108, 13)
        Me.lblPassword.TabIndex = 26
        Me.lblPassword.Text = "QuickBase Password"
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(150, 30)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(120, 20)
        Me.txtPassword.TabIndex = 25
        '
        'lblUsername
        '
        Me.lblUsername.AutoSize = True
        Me.lblUsername.Location = New System.Drawing.Point(15, 11)
        Me.lblUsername.Name = "lblUsername"
        Me.lblUsername.Size = New System.Drawing.Size(110, 13)
        Me.lblUsername.TabIndex = 24
        Me.lblUsername.Text = "QuickBase Username"
        '
        'txtUsername
        '
        Me.txtUsername.Location = New System.Drawing.Point(12, 30)
        Me.txtUsername.Name = "txtUsername"
        Me.txtUsername.Size = New System.Drawing.Size(120, 20)
        Me.txtUsername.TabIndex = 23
        '
        'lblDestinationTable
        '
        Me.lblDestinationTable.AutoSize = True
        Me.lblDestinationTable.Location = New System.Drawing.Point(184, 183)
        Me.lblDestinationTable.Name = "lblDestinationTable"
        Me.lblDestinationTable.Size = New System.Drawing.Size(0, 13)
        Me.lblDestinationTable.TabIndex = 34
        '
        'lblSourceTable
        '
        Me.lblSourceTable.AutoSize = True
        Me.lblSourceTable.Location = New System.Drawing.Point(185, 154)
        Me.lblSourceTable.Name = "lblSourceTable"
        Me.lblSourceTable.Size = New System.Drawing.Size(0, 13)
        Me.lblSourceTable.TabIndex = 35
        '
        'ckbDetectProxy
        '
        Me.ckbDetectProxy.AutoSize = True
        Me.ckbDetectProxy.Location = New System.Drawing.Point(286, 56)
        Me.ckbDetectProxy.Name = "ckbDetectProxy"
        Me.ckbDetectProxy.Size = New System.Drawing.Size(188, 17)
        Me.ckbDetectProxy.TabIndex = 36
        Me.ckbDetectProxy.Text = "Automatically detect proxy settings"
        Me.ckbDetectProxy.UseVisualStyleBackColor = True
        '
        'btnListFields
        '
        Me.btnListFields.Location = New System.Drawing.Point(12, 208)
        Me.btnListFields.Name = "btnListFields"
        Me.btnListFields.Size = New System.Drawing.Size(82, 27)
        Me.btnListFields.TabIndex = 37
        Me.btnListFields.Text = "List Fields"
        Me.btnListFields.UseVisualStyleBackColor = True
        '
        'btnPreview
        '
        Me.btnPreview.Location = New System.Drawing.Point(100, 208)
        Me.btnPreview.Name = "btnPreview"
        Me.btnPreview.Size = New System.Drawing.Size(221, 27)
        Me.btnPreview.TabIndex = 41
        Me.btnPreview.Text = "Preview which rows will be inported"
        Me.btnPreview.UseVisualStyleBackColor = True
        Me.btnPreview.Visible = False
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(587, 116)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(0, 13)
        Me.lblProgress.TabIndex = 42
        '
        'lblMode
        '
        Me.lblMode.AutoSize = True
        Me.lblMode.Location = New System.Drawing.Point(588, 152)
        Me.lblMode.Name = "lblMode"
        Me.lblMode.Size = New System.Drawing.Size(0, 13)
        Me.lblMode.TabIndex = 43
        '
        'btnCatalog
        '
        Me.btnCatalog.Location = New System.Drawing.Point(12, 112)
        Me.btnCatalog.Name = "btnCatalog"
        Me.btnCatalog.Size = New System.Drawing.Size(167, 27)
        Me.btnCatalog.TabIndex = 44
        Me.btnCatalog.Text = "Choose Source Application..."
        Me.btnCatalog.UseVisualStyleBackColor = True
        '
        'lblCatalog
        '
        Me.lblCatalog.AutoSize = True
        Me.lblCatalog.Location = New System.Drawing.Point(185, 119)
        Me.lblCatalog.Name = "lblCatalog"
        Me.lblCatalog.Size = New System.Drawing.Size(0, 13)
        Me.lblCatalog.TabIndex = 45
        '
        'frmCopy
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(543, 947)
        Me.Controls.Add(Me.lblCatalog)
        Me.Controls.Add(Me.btnCatalog)
        Me.Controls.Add(Me.lblMode)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnPreview)
        Me.Controls.Add(Me.btnListFields)
        Me.Controls.Add(Me.ckbDetectProxy)
        Me.Controls.Add(Me.lblSourceTable)
        Me.Controls.Add(Me.lblDestinationTable)
        Me.Controls.Add(Me.pb)
        Me.Controls.Add(Me.lblAppToken)
        Me.Controls.Add(Me.txtAppToken)
        Me.Controls.Add(Me.lblServer)
        Me.Controls.Add(Me.txtServer)
        Me.Controls.Add(Me.lblPassword)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.lblUsername)
        Me.Controls.Add(Me.txtUsername)
        Me.Controls.Add(Me.btnDestination)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.btnSource)
        Me.Controls.Add(Me.dgMapping)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmCopy"
        Me.Text = "QuNect Copy"
        CType(Me.dgMapping, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents dgMapping As System.Windows.Forms.DataGridView
    Friend WithEvents btnSource As System.Windows.Forms.Button
    Friend WithEvents btnImport As System.Windows.Forms.Button
    Friend WithEvents btnDestination As System.Windows.Forms.Button
    Friend WithEvents pb As System.Windows.Forms.ProgressBar
    Friend WithEvents lblAppToken As System.Windows.Forms.Label
    Friend WithEvents txtAppToken As System.Windows.Forms.TextBox
    Friend WithEvents lblServer As System.Windows.Forms.Label
    Friend WithEvents txtServer As System.Windows.Forms.TextBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents lblUsername As System.Windows.Forms.Label
    Friend WithEvents txtUsername As System.Windows.Forms.TextBox
    Friend WithEvents lblDestinationTable As System.Windows.Forms.Label
    Friend WithEvents lblSourceTable As System.Windows.Forms.Label
    Friend WithEvents ckbDetectProxy As System.Windows.Forms.CheckBox
    Friend WithEvents btnListFields As System.Windows.Forms.Button
    Friend WithEvents Source As DataGridViewTextBoxColumn
    Friend WithEvents Destination As DataGridViewComboBoxColumn
    Friend WithEvents btnPreview As Button
    Friend WithEvents lblProgress As Label
    Friend WithEvents lblMode As Label
    Friend WithEvents btnCatalog As Button
    Friend WithEvents lblCatalog As Label
End Class
