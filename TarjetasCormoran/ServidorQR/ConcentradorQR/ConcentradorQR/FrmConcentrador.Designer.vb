<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmConcentrador
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
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

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.lblChequeando = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.CIERRE = New System.Windows.Forms.Button()
        Me.lblTipo = New System.Windows.Forms.Label()
        Me.lblVersionAPLICACION = New System.Windows.Forms.Label()
        Me.lblVersionTJSERVER = New System.Windows.Forms.Label()
        Me.lblVersionTJCOMUN = New System.Windows.Forms.Label()
        Me.lblVersionLibQR = New System.Windows.Forms.Label()
        Me.btnSalir = New System.Windows.Forms.Button()
        Me.Importar = New System.Windows.Forms.Button()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.SuspendLayout()
        '
        'ListView1
        '
        Me.ListView1.HideSelection = False
        Me.ListView1.Location = New System.Drawing.Point(252, 33)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(664, 405)
        Me.ListView1.TabIndex = 0
        Me.ListView1.UseCompatibleStateImageBehavior = False
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(24, 46)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(75, 23)
        Me.Button3.TabIndex = 1
        Me.Button3.Text = "Button1"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'lblChequeando
        '
        Me.lblChequeando.AutoSize = True
        Me.lblChequeando.Location = New System.Drawing.Point(24, 13)
        Me.lblChequeando.Name = "lblChequeando"
        Me.lblChequeando.Size = New System.Drawing.Size(39, 13)
        Me.lblChequeando.TabIndex = 2
        Me.lblChequeando.Text = "Label1"
        '
        'Timer1
        '
        '
        'CIERRE
        '
        Me.CIERRE.Location = New System.Drawing.Point(24, 104)
        Me.CIERRE.Name = "CIERRE"
        Me.CIERRE.Size = New System.Drawing.Size(75, 23)
        Me.CIERRE.TabIndex = 3
        Me.CIERRE.Text = "Cerrar"
        Me.CIERRE.UseVisualStyleBackColor = True
        '
        'lblTipo
        '
        Me.lblTipo.AutoSize = True
        Me.lblTipo.Location = New System.Drawing.Point(656, 6)
        Me.lblTipo.Name = "lblTipo"
        Me.lblTipo.Size = New System.Drawing.Size(39, 13)
        Me.lblTipo.TabIndex = 4
        Me.lblTipo.Text = "Label1"
        '
        'lblVersionAPLICACION
        '
        Me.lblVersionAPLICACION.AutoSize = True
        Me.lblVersionAPLICACION.Location = New System.Drawing.Point(12, 193)
        Me.lblVersionAPLICACION.Name = "lblVersionAPLICACION"
        Me.lblVersionAPLICACION.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionAPLICACION.TabIndex = 5
        Me.lblVersionAPLICACION.Text = "Label1"
        '
        'lblVersionTJSERVER
        '
        Me.lblVersionTJSERVER.AutoSize = True
        Me.lblVersionTJSERVER.Location = New System.Drawing.Point(12, 206)
        Me.lblVersionTJSERVER.Name = "lblVersionTJSERVER"
        Me.lblVersionTJSERVER.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionTJSERVER.TabIndex = 6
        Me.lblVersionTJSERVER.Text = "Label1"
        '
        'lblVersionTJCOMUN
        '
        Me.lblVersionTJCOMUN.AutoSize = True
        Me.lblVersionTJCOMUN.Location = New System.Drawing.Point(12, 219)
        Me.lblVersionTJCOMUN.Name = "lblVersionTJCOMUN"
        Me.lblVersionTJCOMUN.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionTJCOMUN.TabIndex = 7
        Me.lblVersionTJCOMUN.Text = "Label1"
        '
        'lblVersionLibQR
        '
        Me.lblVersionLibQR.AutoSize = True
        Me.lblVersionLibQR.Location = New System.Drawing.Point(12, 232)
        Me.lblVersionLibQR.Name = "lblVersionLibQR"
        Me.lblVersionLibQR.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionLibQR.TabIndex = 8
        Me.lblVersionLibQR.Text = "Label1"
        '
        'btnSalir
        '
        Me.btnSalir.Location = New System.Drawing.Point(24, 133)
        Me.btnSalir.Name = "btnSalir"
        Me.btnSalir.Size = New System.Drawing.Size(75, 23)
        Me.btnSalir.TabIndex = 9
        Me.btnSalir.Text = "Salir"
        Me.btnSalir.UseVisualStyleBackColor = True
        '
        'Importar
        '
        Me.Importar.Location = New System.Drawing.Point(24, 75)
        Me.Importar.Name = "Importar"
        Me.Importar.Size = New System.Drawing.Size(75, 23)
        Me.Importar.TabIndex = 10
        Me.Importar.Text = "Importar"
        Me.Importar.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'FrmConcentrador
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(928, 450)
        Me.ControlBox = False
        Me.Controls.Add(Me.Importar)
        Me.Controls.Add(Me.btnSalir)
        Me.Controls.Add(Me.lblVersionLibQR)
        Me.Controls.Add(Me.lblVersionTJCOMUN)
        Me.Controls.Add(Me.lblVersionTJSERVER)
        Me.Controls.Add(Me.lblVersionAPLICACION)
        Me.Controls.Add(Me.lblTipo)
        Me.Controls.Add(Me.CIERRE)
        Me.Controls.Add(Me.lblChequeando)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.ListView1)
        Me.Name = "FrmConcentrador"
        Me.Text = "FrmConcentrador"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListView1 As ListView
    Friend WithEvents Button3 As Button
    Friend WithEvents lblChequeando As Label
    Friend WithEvents Timer1 As Timer
    Friend WithEvents CIERRE As Button
    Friend WithEvents lblTipo As Label
    Friend WithEvents lblVersionAPLICACION As Label
    Friend WithEvents lblVersionTJSERVER As Label
    Friend WithEvents lblVersionTJCOMUN As Label
    Friend WithEvents lblVersionLibQR As Label
    Friend WithEvents btnSalir As Button
    Friend WithEvents Importar As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
End Class
