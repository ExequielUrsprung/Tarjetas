<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormQR
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
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.Button8 = New System.Windows.Forms.Button()
        Me.Button9 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(12, 14)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(101, 23)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Crear Sucursal"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(12, 43)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(101, 23)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "Eliminar Sucursal"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(12, 72)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(101, 23)
        Me.Button3.TabIndex = 2
        Me.Button3.Text = "Modificar Sucursal"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(12, 178)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(101, 23)
        Me.Button4.TabIndex = 5
        Me.Button4.Text = "Modificar Sucursal"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(12, 149)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(101, 23)
        Me.Button5.TabIndex = 4
        Me.Button5.Text = "Eliminar Caja"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'Button6
        '
        Me.Button6.Location = New System.Drawing.Point(12, 120)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(101, 23)
        Me.Button6.TabIndex = 3
        Me.Button6.Text = "Crear Caja"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'Button7
        '
        Me.Button7.Location = New System.Drawing.Point(12, 224)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(101, 23)
        Me.Button7.TabIndex = 6
        Me.Button7.Text = "Generar Pago"
        Me.Button7.UseVisualStyleBackColor = True
        '
        'Button8
        '
        Me.Button8.Location = New System.Drawing.Point(12, 253)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(101, 23)
        Me.Button8.TabIndex = 7
        Me.Button8.Text = "Cancelar Pago"
        Me.Button8.UseVisualStyleBackColor = True
        '
        'Button9
        '
        Me.Button9.Location = New System.Drawing.Point(12, 282)
        Me.Button9.Name = "Button9"
        Me.Button9.Size = New System.Drawing.Size(101, 23)
        Me.Button9.TabIndex = 8
        Me.Button9.Text = "Obtener Pago"
        Me.Button9.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(136, 229)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(0, 13)
        Me.Label1.TabIndex = 9
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        '
        'FormQR
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button9)
        Me.Controls.Add(Me.Button8)
        Me.Controls.Add(Me.Button7)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.Button6)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Name = "FormQR"
        Me.Text = "FormQR"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents Button6 As Button
    Friend WithEvents Button7 As Button
    Friend WithEvents Button8 As Button
    Friend WithEvents Button9 As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents Timer1 As Timer
End Class
