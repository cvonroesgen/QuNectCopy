Public Class frmTableChooser

    Private Sub tvAppsTables_DoubleClick(sender As Object, e As EventArgs) Handles tvAppsTables.DoubleClick
        btnDone_Click(sender, e)
    End Sub
    Private Sub tvAppsTables_Click(sender As Object, e As EventArgs) Handles tvAppsTables.Click
        If tvAppsTables.SelectedNode Is Nothing Then
            Exit Sub
        End If
        If tvAppsTables.SelectedNode.Level <> 1 Then
            Exit Sub
        End If
        frmCopy.lblDestinationTable.Text = tvAppsTables.SelectedNode.FullPath()
        hideButtons()
    End Sub

    Private Sub btnDone_Click(sender As Object, e As EventArgs) Handles btnDone.Click
        If tvAppsTables.SelectedNode Is Nothing Then
            Me.Hide()
            Exit Sub
        End If
        Dim tableLabel As Label
        If frmCopy.sourceOrDestination = frmCopy.tableType.source Then
            tableLabel = frmCopy.lblSourceTable
        ElseIf frmCopy.sourceOrDestination = frmCopy.tableType.sourceCatalog Then
            frmCopy.lblCatalog.Tag = tvAppsTables.SelectedNode.Tag
            frmCopy.lblCatalog.Text = tvAppsTables.SelectedNode.FullPath()
            hideButtons()
            Me.Hide()
            Return
        Else
            tableLabel = frmCopy.lblDestinationTable
        End If

        If tvAppsTables.SelectedNode.Level <> 1 And frmCopy.sourceOrDestination <> frmCopy.tableType.source Then
            tableLabel.Text = ""
        Else
            tableLabel.Text = tvAppsTables.SelectedNode.FullPath()
        End If
        hideButtons()
        Me.Hide()
    End Sub
    Private Sub hideButtons()
        frmCopy.btnPreview.Visible = False
        frmCopy.btnImport.Visible = False
        frmCopy.dgMapping.Visible = False
    End Sub
End Class