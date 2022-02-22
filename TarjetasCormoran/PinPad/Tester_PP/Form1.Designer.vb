<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.ListView2 = New System.Windows.Forms.ListView()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.Button8 = New System.Windows.Forms.Button()
        Me.Button11 = New System.Windows.Forms.Button()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.ListView3 = New System.Windows.Forms.ListView()
        Me.Importe = New System.Windows.Forms.TextBox()
        Me.Cashback = New System.Windows.Forms.TextBox()
        Me.PlanCuotas = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.rdbCompra = New System.Windows.Forms.RadioButton()
        Me.rdbDevolucion = New System.Windows.Forms.RadioButton()
        Me.rdbAnulacionCompra = New System.Windows.Forms.RadioButton()
        Me.rdbAnulacionDevolucion = New System.Windows.Forms.RadioButton()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ticket = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cajera = New System.Windows.Forms.TextBox()
        Me.rdbReverso = New System.Windows.Forms.RadioButton()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TKTOriginal = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.FechaTRXOriginal = New System.Windows.Forms.TextBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.Button9 = New System.Windows.Forms.Button()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.rdbConsulta = New System.Windows.Forms.RadioButton()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.txtnrodocumento = New System.Windows.Forms.TextBox()
        Me.Button10 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(11, 636)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(161, 23)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "Tarjeta M/B/C"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'ListView1
        '
        Me.ListView1.HideSelection = False
        Me.ListView1.Location = New System.Drawing.Point(187, 23)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(813, 471)
        Me.ListView1.TabIndex = 3
        Me.ListView1.UseCompatibleStateImageBehavior = False
        '
        'ListView2
        '
        Me.ListView2.HideSelection = False
        Me.ListView2.Location = New System.Drawing.Point(187, 499)
        Me.ListView2.Name = "ListView2"
        Me.ListView2.Size = New System.Drawing.Size(813, 276)
        Me.ListView2.TabIndex = 5
        Me.ListView2.UseCompatibleStateImageBehavior = False
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(12, 540)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(161, 23)
        Me.Button4.TabIndex = 6
        Me.Button4.Text = "Inicializar"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(11, 665)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(161, 23)
        Me.Button5.TabIndex = 7
        Me.Button5.Text = "Cancelar"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'Button8
        '
        Me.Button8.Location = New System.Drawing.Point(12, 511)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(161, 23)
        Me.Button8.TabIndex = 10
        Me.Button8.Text = "Sincronizar"
        Me.Button8.UseVisualStyleBackColor = True
        '
        'Button11
        '
        Me.Button11.Location = New System.Drawing.Point(11, 607)
        Me.Button11.Name = "Button11"
        Me.Button11.Size = New System.Drawing.Size(161, 23)
        Me.Button11.TabIndex = 13
        Me.Button11.Text = "Tarjeta CL"
        Me.Button11.UseVisualStyleBackColor = True
        '
        'Timer1
        '
        Me.Timer1.Interval = 500
        '
        'ListView3
        '
        Me.ListView3.HideSelection = False
        Me.ListView3.Location = New System.Drawing.Point(1006, 22)
        Me.ListView3.Name = "ListView3"
        Me.ListView3.Size = New System.Drawing.Size(450, 753)
        Me.ListView3.TabIndex = 14
        Me.ListView3.UseCompatibleStateImageBehavior = False
        '
        'Importe
        '
        Me.Importe.Location = New System.Drawing.Point(13, 197)
        Me.Importe.Name = "Importe"
        Me.Importe.Size = New System.Drawing.Size(161, 20)
        Me.Importe.TabIndex = 15
        Me.Importe.Text = "10,00"
        Me.Importe.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Cashback
        '
        Me.Cashback.Location = New System.Drawing.Point(13, 236)
        Me.Cashback.Name = "Cashback"
        Me.Cashback.Size = New System.Drawing.Size(161, 20)
        Me.Cashback.TabIndex = 16
        Me.Cashback.Text = "0,00"
        Me.Cashback.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'PlanCuotas
        '
        Me.PlanCuotas.Location = New System.Drawing.Point(13, 275)
        Me.PlanCuotas.Name = "PlanCuotas"
        Me.PlanCuotas.Size = New System.Drawing.Size(161, 20)
        Me.PlanCuotas.TabIndex = 17
        Me.PlanCuotas.Text = "1"
        Me.PlanCuotas.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(14, 181)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(45, 13)
        Me.Label1.TabIndex = 19
        Me.Label1.Text = "Importe:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(14, 220)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(96, 13)
        Me.Label2.TabIndex = 20
        Me.Label2.Text = "Importe Cashback:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(14, 259)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(69, 13)
        Me.Label3.TabIndex = 21
        Me.Label3.Text = "Plan/Cuotas:"
        '
        'rdbCompra
        '
        Me.rdbCompra.Appearance = System.Windows.Forms.Appearance.Button
        Me.rdbCompra.Checked = True
        Me.rdbCompra.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.rdbCompra.FlatAppearance.BorderSize = 2
        Me.rdbCompra.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
        Me.rdbCompra.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.rdbCompra.Location = New System.Drawing.Point(13, 9)
        Me.rdbCompra.Margin = New System.Windows.Forms.Padding(0)
        Me.rdbCompra.Name = "rdbCompra"
        Me.rdbCompra.Size = New System.Drawing.Size(161, 27)
        Me.rdbCompra.TabIndex = 22
        Me.rdbCompra.TabStop = True
        Me.rdbCompra.Text = "Compra"
        Me.rdbCompra.UseVisualStyleBackColor = True
        '
        'rdbDevolucion
        '
        Me.rdbDevolucion.Appearance = System.Windows.Forms.Appearance.Button
        Me.rdbDevolucion.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.rdbDevolucion.FlatAppearance.BorderSize = 2
        Me.rdbDevolucion.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
        Me.rdbDevolucion.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.rdbDevolucion.Location = New System.Drawing.Point(13, 37)
        Me.rdbDevolucion.Margin = New System.Windows.Forms.Padding(0)
        Me.rdbDevolucion.Name = "rdbDevolucion"
        Me.rdbDevolucion.Size = New System.Drawing.Size(161, 27)
        Me.rdbDevolucion.TabIndex = 23
        Me.rdbDevolucion.Text = "Devolución"
        Me.rdbDevolucion.UseVisualStyleBackColor = True
        '
        'rdbAnulacionCompra
        '
        Me.rdbAnulacionCompra.Appearance = System.Windows.Forms.Appearance.Button
        Me.rdbAnulacionCompra.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.rdbAnulacionCompra.FlatAppearance.BorderSize = 2
        Me.rdbAnulacionCompra.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
        Me.rdbAnulacionCompra.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.rdbAnulacionCompra.Location = New System.Drawing.Point(13, 93)
        Me.rdbAnulacionCompra.Margin = New System.Windows.Forms.Padding(0)
        Me.rdbAnulacionCompra.Name = "rdbAnulacionCompra"
        Me.rdbAnulacionCompra.Size = New System.Drawing.Size(161, 27)
        Me.rdbAnulacionCompra.TabIndex = 24
        Me.rdbAnulacionCompra.Text = "Anulacion-Compra"
        Me.rdbAnulacionCompra.UseVisualStyleBackColor = True
        '
        'rdbAnulacionDevolucion
        '
        Me.rdbAnulacionDevolucion.Appearance = System.Windows.Forms.Appearance.Button
        Me.rdbAnulacionDevolucion.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.rdbAnulacionDevolucion.FlatAppearance.BorderSize = 2
        Me.rdbAnulacionDevolucion.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
        Me.rdbAnulacionDevolucion.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.rdbAnulacionDevolucion.Location = New System.Drawing.Point(13, 65)
        Me.rdbAnulacionDevolucion.Margin = New System.Windows.Forms.Padding(0)
        Me.rdbAnulacionDevolucion.Name = "rdbAnulacionDevolucion"
        Me.rdbAnulacionDevolucion.Size = New System.Drawing.Size(161, 27)
        Me.rdbAnulacionDevolucion.TabIndex = 25
        Me.rdbAnulacionDevolucion.Text = "Anulacion-Devolucion"
        Me.rdbAnulacionDevolucion.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(14, 298)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(64, 13)
        Me.Label4.TabIndex = 27
        Me.Label4.Text = "Ticket Caja:"
        '
        'ticket
        '
        Me.ticket.Location = New System.Drawing.Point(13, 314)
        Me.ticket.Name = "ticket"
        Me.ticket.Size = New System.Drawing.Size(161, 20)
        Me.ticket.TabIndex = 26
        Me.ticket.Text = "1"
        Me.ticket.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(14, 338)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(40, 13)
        Me.Label5.TabIndex = 29
        Me.Label5.Text = "Cajera:"
        '
        'cajera
        '
        Me.cajera.Location = New System.Drawing.Point(13, 354)
        Me.cajera.Name = "cajera"
        Me.cajera.Size = New System.Drawing.Size(161, 20)
        Me.cajera.TabIndex = 28
        Me.cajera.Text = "9898"
        Me.cajera.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'rdbReverso
        '
        Me.rdbReverso.Appearance = System.Windows.Forms.Appearance.Button
        Me.rdbReverso.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.rdbReverso.FlatAppearance.BorderSize = 2
        Me.rdbReverso.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
        Me.rdbReverso.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.rdbReverso.Location = New System.Drawing.Point(13, 121)
        Me.rdbReverso.Margin = New System.Windows.Forms.Padding(0)
        Me.rdbReverso.Name = "rdbReverso"
        Me.rdbReverso.Size = New System.Drawing.Size(161, 27)
        Me.rdbReverso.TabIndex = 30
        Me.rdbReverso.Text = "Reversos"
        Me.rdbReverso.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(14, 382)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(96, 13)
        Me.Label6.TabIndex = 32
        Me.Label6.Text = "Ticket Trx Original:"
        '
        'TKTOriginal
        '
        Me.TKTOriginal.Location = New System.Drawing.Point(13, 398)
        Me.TKTOriginal.Name = "TKTOriginal"
        Me.TKTOriginal.Size = New System.Drawing.Size(161, 20)
        Me.TKTOriginal.TabIndex = 31
        Me.TKTOriginal.Text = "1"
        Me.TKTOriginal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(14, 427)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(153, 13)
        Me.Label7.TabIndex = 34
        Me.Label7.Text = "Fecha Trx Original (DDMMYY):"
        '
        'FechaTRXOriginal
        '
        Me.FechaTRXOriginal.Location = New System.Drawing.Point(13, 443)
        Me.FechaTRXOriginal.Name = "FechaTRXOriginal"
        Me.FechaTRXOriginal.Size = New System.Drawing.Size(161, 20)
        Me.FechaTRXOriginal.TabIndex = 33
        Me.FechaTRXOriginal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(11, 694)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(161, 23)
        Me.Button2.TabIndex = 35
        Me.Button2.Text = "Reversar"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(12, 723)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(161, 23)
        Me.Button3.TabIndex = 36
        Me.Button3.Text = "PEI"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button6
        '
        Me.Button6.Location = New System.Drawing.Point(11, 578)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(161, 23)
        Me.Button6.TabIndex = 37
        Me.Button6.Text = "Tarjeta MM"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'Button7
        '
        Me.Button7.Location = New System.Drawing.Point(12, 752)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(161, 23)
        Me.Button7.TabIndex = 38
        Me.Button7.Text = "QR"
        Me.Button7.UseVisualStyleBackColor = True
        '
        'Button9
        '
        Me.Button9.Location = New System.Drawing.Point(11, 781)
        Me.Button9.Name = "Button9"
        Me.Button9.Size = New System.Drawing.Size(162, 23)
        Me.Button9.TabIndex = 39
        Me.Button9.Text = "Actualizar"
        Me.Button9.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Label8.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(196, 789)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(13, 19)
        Me.Label8.TabIndex = 41
        Me.Label8.Text = "."
        '
        'BackgroundWorker1
        '
        '
        'rdbConsulta
        '
        Me.rdbConsulta.Appearance = System.Windows.Forms.Appearance.Button
        Me.rdbConsulta.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.rdbConsulta.FlatAppearance.BorderSize = 2
        Me.rdbConsulta.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
        Me.rdbConsulta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.rdbConsulta.Location = New System.Drawing.Point(12, 149)
        Me.rdbConsulta.Margin = New System.Windows.Forms.Padding(0)
        Me.rdbConsulta.Name = "rdbConsulta"
        Me.rdbConsulta.Size = New System.Drawing.Size(161, 27)
        Me.rdbConsulta.TabIndex = 42
        Me.rdbConsulta.Text = "Consulta (PEI)"
        Me.rdbConsulta.UseVisualStyleBackColor = True
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(14, 469)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(65, 13)
        Me.Label9.TabIndex = 44
        Me.Label9.Text = "Documento:"
        '
        'txtnrodocumento
        '
        Me.txtnrodocumento.Location = New System.Drawing.Point(13, 485)
        Me.txtnrodocumento.Name = "txtnrodocumento"
        Me.txtnrodocumento.Size = New System.Drawing.Size(161, 20)
        Me.txtnrodocumento.TabIndex = 43
        Me.txtnrodocumento.Text = "20111222"
        Me.txtnrodocumento.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Button10
        '
        Me.Button10.Location = New System.Drawing.Point(1332, 785)
        Me.Button10.Name = "Button10"
        Me.Button10.Size = New System.Drawing.Size(124, 23)
        Me.Button10.TabIndex = 45
        Me.Button10.Text = "Homologacion QR"
        Me.Button10.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1468, 817)
        Me.Controls.Add(Me.Button10)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.txtnrodocumento)
        Me.Controls.Add(Me.rdbConsulta)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Button9)
        Me.Controls.Add(Me.Button7)
        Me.Controls.Add(Me.Button6)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.FechaTRXOriginal)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.TKTOriginal)
        Me.Controls.Add(Me.rdbReverso)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.cajera)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.ticket)
        Me.Controls.Add(Me.rdbAnulacionDevolucion)
        Me.Controls.Add(Me.rdbAnulacionCompra)
        Me.Controls.Add(Me.rdbDevolucion)
        Me.Controls.Add(Me.rdbCompra)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.PlanCuotas)
        Me.Controls.Add(Me.Cashback)
        Me.Controls.Add(Me.Importe)
        Me.Controls.Add(Me.ListView3)
        Me.Controls.Add(Me.Button11)
        Me.Controls.Add(Me.Button8)
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.ListView2)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button1 As Button
    Friend WithEvents ListView1 As ListView
    Friend WithEvents ListView2 As ListView
    Friend WithEvents Button4 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents Button8 As Button
    Friend WithEvents Button11 As Button
    Friend WithEvents Timer1 As Timer
    Friend WithEvents ListView3 As ListView
    Friend WithEvents Importe As TextBox
    Friend WithEvents Cashback As TextBox
    Friend WithEvents PlanCuotas As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents rdbCompra As RadioButton
    Friend WithEvents rdbDevolucion As RadioButton
    Friend WithEvents rdbAnulacionCompra As RadioButton
    Friend WithEvents rdbAnulacionDevolucion As RadioButton
    Friend WithEvents Label4 As Label
    Friend WithEvents ticket As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents cajera As TextBox
    Friend WithEvents rdbReverso As RadioButton
    Friend WithEvents Label6 As Label
    Friend WithEvents TKTOriginal As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents FechaTRXOriginal As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Button6 As Button
    Friend WithEvents Button7 As Button
    Friend WithEvents Button9 As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents Label8 As Label
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents rdbConsulta As RadioButton
    Friend WithEvents Label9 As Label
    Friend WithEvents txtnrodocumento As TextBox
    Friend WithEvents Button10 As Button
End Class
