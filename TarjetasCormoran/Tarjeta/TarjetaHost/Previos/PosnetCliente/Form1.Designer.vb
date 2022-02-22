<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.components = New System.ComponentModel.Container
        Me.Label5 = New System.Windows.Forms.Label
        Me.ListBox1 = New System.Windows.Forms.ListBox
        Me.Button3 = New System.Windows.Forms.Button
        Me.CmdEnviar = New System.Windows.Forms.Button
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Button2 = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.CmbTerm = New System.Windows.Forms.ComboBox
        Me.EdtVto = New System.Windows.Forms.DateTimePicker
        Me.EdtFectrans = New System.Windows.Forms.DateTimePicker
        Me.ListBox2 = New System.Windows.Forms.ListBox
        Me.CmbPlastico = New System.Windows.Forms.ComboBox
        Me.CmbIP = New System.Windows.Forms.ComboBox
        Me.CmbTransaccion = New System.Windows.Forms.ComboBox
        Me.EdtFecNegocio = New System.Windows.Forms.DateTimePicker
        Me.CheckBox1 = New System.Windows.Forms.CheckBox
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.EdtRespuesta = New System.Windows.Forms.TextBox
        Me.TxtTrack2 = New System.Windows.Forms.TextBox
        Me.Label7 = New System.Windows.Forms.Label
        Me.Label6 = New System.Windows.Forms.Label
        Me.CmbPlan = New System.Windows.Forms.ComboBox
        Me.CmbCuotas = New System.Windows.Forms.ComboBox
        Me.CMBMONTO = New System.Windows.Forms.ComboBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.Label9 = New System.Windows.Forms.Label
        Me.CmbComercio = New System.Windows.Forms.ComboBox
        Me.Label10 = New System.Windows.Forms.Label
        Me.Label11 = New System.Windows.Forms.Label
        Me.txtVisor = New System.Windows.Forms.TextBox
        Me.Label12 = New System.Windows.Forms.Label
        Me.TxtCvc = New System.Windows.Forms.TextBox
        Me.Label13 = New System.Windows.Forms.Label
        Me.CmbModoIngreso = New System.Windows.Forms.ComboBox
        Me.Label14 = New System.Windows.Forms.Label
        Me.Button4 = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(16, 172)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(42, 13)
        Me.Label5.TabIndex = 58
        Me.Label5.Text = "Fechas"
        '
        'ListBox1
        '
        Me.ListBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListBox1.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.HorizontalScrollbar = True
        Me.ListBox1.ItemHeight = 11
        Me.ListBox1.Location = New System.Drawing.Point(18, 284)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(695, 158)
        Me.ListBox1.TabIndex = 50
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(456, 244)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(80, 26)
        Me.Button3.TabIndex = 49
        Me.Button3.Text = "Mandar 0800"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'CmdEnviar
        '
        Me.CmdEnviar.Location = New System.Drawing.Point(316, 244)
        Me.CmdEnviar.Name = "CmdEnviar"
        Me.CmdEnviar.Size = New System.Drawing.Size(80, 28)
        Me.CmdEnviar.TabIndex = 48
        Me.CmdEnviar.Text = "Enviar"
        Me.CmdEnviar.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 114)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(44, 13)
        Me.Label4.TabIndex = 47
        Me.Label4.Text = "Plastico"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 86)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(66, 13)
        Me.Label3.TabIndex = 46
        Me.Label3.Text = "Transaccion"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 58)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(47, 13)
        Me.Label2.TabIndex = 45
        Me.Label2.Text = "Terminal"
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(300, 10)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(84, 24)
        Me.Button2.TabIndex = 44
        Me.Button2.Text = "DesConectar"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(214, 10)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(70, 24)
        Me.Button1.TabIndex = 43
        Me.Button1.Text = "Conectar"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(29, 13)
        Me.Label1.TabIndex = 42
        Me.Label1.Text = "Host"
        '
        'CmbTerm
        '
        Me.CmbTerm.FormattingEnabled = True
        Me.CmbTerm.Items.AddRange(New Object() {"NEXO1234", "SSS12345", "70580101"})
        Me.CmbTerm.Location = New System.Drawing.Point(84, 52)
        Me.CmbTerm.Name = "CmbTerm"
        Me.CmbTerm.Size = New System.Drawing.Size(128, 21)
        Me.CmbTerm.TabIndex = 41
        Me.CmbTerm.Text = "NEXO1234"
        '
        'EdtVto
        '
        Me.EdtVto.CustomFormat = "MM/yy"
        Me.EdtVto.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.EdtVto.Location = New System.Drawing.Point(236, 112)
        Me.EdtVto.Name = "EdtVto"
        Me.EdtVto.Size = New System.Drawing.Size(56, 20)
        Me.EdtVto.TabIndex = 40
        Me.EdtVto.Value = New Date(2008, 12, 31, 0, 0, 0, 0)
        '
        'EdtFectrans
        '
        Me.EdtFectrans.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.EdtFectrans.Location = New System.Drawing.Point(84, 192)
        Me.EdtFectrans.Name = "EdtFectrans"
        Me.EdtFectrans.Size = New System.Drawing.Size(112, 20)
        Me.EdtFectrans.TabIndex = 39
        '
        'ListBox2
        '
        Me.ListBox2.FormattingEnabled = True
        Me.ListBox2.Location = New System.Drawing.Point(460, 72)
        Me.ListBox2.Name = "ListBox2"
        Me.ListBox2.Size = New System.Drawing.Size(256, 134)
        Me.ListBox2.TabIndex = 35
        '
        'CmbPlastico
        '
        Me.CmbPlastico.FormattingEnabled = True
        Me.CmbPlastico.Items.AddRange(New Object() {"6281610038744002"})
        Me.CmbPlastico.Location = New System.Drawing.Point(84, 112)
        Me.CmbPlastico.Name = "CmbPlastico"
        Me.CmbPlastico.Size = New System.Drawing.Size(146, 21)
        Me.CmbPlastico.TabIndex = 34
        Me.CmbPlastico.Text = "6281610038744002"
        '
        'CmbIP
        '
        Me.CmbIP.FormattingEnabled = True
        Me.CmbIP.Items.AddRange(New Object() {"127.0.0.1", "172.100.1.20"})
        Me.CmbIP.Location = New System.Drawing.Point(84, 12)
        Me.CmbIP.Name = "CmbIP"
        Me.CmbIP.Size = New System.Drawing.Size(112, 21)
        Me.CmbIP.TabIndex = 33
        '
        'CmbTransaccion
        '
        Me.CmbTransaccion.FormattingEnabled = True
        Me.CmbTransaccion.Location = New System.Drawing.Point(84, 82)
        Me.CmbTransaccion.Name = "CmbTransaccion"
        Me.CmbTransaccion.Size = New System.Drawing.Size(126, 21)
        Me.CmbTransaccion.TabIndex = 32
        '
        'EdtFecNegocio
        '
        Me.EdtFecNegocio.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.EdtFecNegocio.Location = New System.Drawing.Point(84, 168)
        Me.EdtFecNegocio.Name = "EdtFecNegocio"
        Me.EdtFecNegocio.Size = New System.Drawing.Size(112, 20)
        Me.EdtFecNegocio.TabIndex = 38
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(84, 36)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(71, 17)
        Me.CheckBox1.TabIndex = 37
        Me.CheckBox1.Text = "Escuchar"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'Timer1
        '
        '
        'EdtRespuesta
        '
        Me.EdtRespuesta.Location = New System.Drawing.Point(460, 32)
        Me.EdtRespuesta.Name = "EdtRespuesta"
        Me.EdtRespuesta.Size = New System.Drawing.Size(248, 20)
        Me.EdtRespuesta.TabIndex = 36
        '
        'TxtTrack2
        '
        Me.TxtTrack2.Location = New System.Drawing.Point(84, 140)
        Me.TxtTrack2.Name = "TxtTrack2"
        Me.TxtTrack2.Size = New System.Drawing.Size(300, 20)
        Me.TxtTrack2.TabIndex = 60
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(16, 144)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(40, 13)
        Me.Label7.TabIndex = 61
        Me.Label7.Text = "Tracks"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(20, 220)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(28, 13)
        Me.Label6.TabIndex = 62
        Me.Label6.Text = "Plan"
        '
        'CmbPlan
        '
        Me.CmbPlan.FormattingEnabled = True
        Me.CmbPlan.Items.AddRange(New Object() {"1", "2", "3", "7", "D"})
        Me.CmbPlan.Location = New System.Drawing.Point(84, 220)
        Me.CmbPlan.Name = "CmbPlan"
        Me.CmbPlan.Size = New System.Drawing.Size(60, 21)
        Me.CmbPlan.TabIndex = 63
        Me.CmbPlan.Text = "1"
        '
        'CmbCuotas
        '
        Me.CmbCuotas.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.CmbCuotas.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.CmbCuotas.FormattingEnabled = True
        Me.CmbCuotas.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "18", "24", "36", "99"})
        Me.CmbCuotas.Location = New System.Drawing.Point(208, 220)
        Me.CmbCuotas.Name = "CmbCuotas"
        Me.CmbCuotas.Size = New System.Drawing.Size(69, 21)
        Me.CmbCuotas.TabIndex = 64
        Me.CmbCuotas.Text = "1"
        '
        'CMBMONTO
        '
        Me.CMBMONTO.FormattingEnabled = True
        Me.CMBMONTO.Items.AddRange(New Object() {"0.01", "0.12", "1.2", "1.23", "9.99", "100", "400", "10000", "222222"})
        Me.CMBMONTO.Location = New System.Drawing.Point(84, 252)
        Me.CMBMONTO.Name = "CMBMONTO"
        Me.CMBMONTO.Size = New System.Drawing.Size(121, 21)
        Me.CMBMONTO.TabIndex = 65
        Me.CMBMONTO.Text = "10"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(24, 252)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(37, 13)
        Me.Label8.TabIndex = 66
        Me.Label8.Text = "Monto"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(164, 220)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(40, 13)
        Me.Label9.TabIndex = 67
        Me.Label9.Text = "Cuotas"
        '
        'CmbComercio
        '
        Me.CmbComercio.FormattingEnabled = True
        Me.CmbComercio.Items.AddRange(New Object() {"2371", "1", "2"})
        Me.CmbComercio.Location = New System.Drawing.Point(304, 52)
        Me.CmbComercio.Name = "CmbComercio"
        Me.CmbComercio.Size = New System.Drawing.Size(101, 21)
        Me.CmbComercio.TabIndex = 68
        Me.CmbComercio.Text = "2371"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(224, 56)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(51, 13)
        Me.Label10.TabIndex = 69
        Me.Label10.Text = "Comercio"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(460, 56)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(37, 13)
        Me.Label11.TabIndex = 70
        Me.Label11.Text = "Ticket"
        '
        'txtVisor
        '
        Me.txtVisor.Location = New System.Drawing.Point(460, 8)
        Me.txtVisor.Name = "txtVisor"
        Me.txtVisor.Size = New System.Drawing.Size(248, 20)
        Me.txtVisor.TabIndex = 71
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(396, 12)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(30, 13)
        Me.Label12.TabIndex = 72
        Me.Label12.Text = "Visor"
        '
        'TxtCvc
        '
        Me.TxtCvc.Location = New System.Drawing.Point(336, 112)
        Me.TxtCvc.MaxLength = 4
        Me.TxtCvc.Name = "TxtCvc"
        Me.TxtCvc.Size = New System.Drawing.Size(48, 20)
        Me.TxtCvc.TabIndex = 73
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(296, 116)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(34, 13)
        Me.Label13.TabIndex = 74
        Me.Label13.Text = "CVC2"
        '
        'CmbModoIngreso
        '
        Me.CmbModoIngreso.FormattingEnabled = True
        Me.CmbModoIngreso.Location = New System.Drawing.Point(304, 80)
        Me.CmbModoIngreso.Name = "CmbModoIngreso"
        Me.CmbModoIngreso.Size = New System.Drawing.Size(101, 21)
        Me.CmbModoIngreso.TabIndex = 75
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(224, 84)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(72, 13)
        Me.Label14.TabIndex = 76
        Me.Label14.Text = "Modo Ingreso"
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(544, 244)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(80, 24)
        Me.Button4.TabIndex = 77
        Me.Button4.Text = "Ejecutar XLS"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(747, 451)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Label14)
        Me.Controls.Add(Me.CmbModoIngreso)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.TxtCvc)
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.txtVisor)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.CmbComercio)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.CMBMONTO)
        Me.Controls.Add(Me.CmbCuotas)
        Me.Controls.Add(Me.CmbPlan)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.TxtTrack2)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.ListBox1)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.CmdEnviar)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.CmbTerm)
        Me.Controls.Add(Me.EdtVto)
        Me.Controls.Add(Me.EdtFectrans)
        Me.Controls.Add(Me.ListBox2)
        Me.Controls.Add(Me.CmbPlastico)
        Me.Controls.Add(Me.CmbIP)
        Me.Controls.Add(Me.CmbTransaccion)
        Me.Controls.Add(Me.EdtFecNegocio)
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.EdtRespuesta)
        Me.Name = "Form1"
        Me.Text = "Cliente Posnet 1.1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents CmdEnviar As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CmbTerm As System.Windows.Forms.ComboBox
    Friend WithEvents EdtVto As System.Windows.Forms.DateTimePicker
    Friend WithEvents EdtFectrans As System.Windows.Forms.DateTimePicker
    Friend WithEvents ListBox2 As System.Windows.Forms.ListBox
    Friend WithEvents CmbPlastico As System.Windows.Forms.ComboBox
    Friend WithEvents CmbIP As System.Windows.Forms.ComboBox
    Friend WithEvents CmbTransaccion As System.Windows.Forms.ComboBox
    Friend WithEvents EdtFecNegocio As System.Windows.Forms.DateTimePicker
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents EdtRespuesta As System.Windows.Forms.TextBox
    Friend WithEvents TxtTrack2 As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents CmbPlan As System.Windows.Forms.ComboBox
    Friend WithEvents CmbCuotas As System.Windows.Forms.ComboBox
    Friend WithEvents CMBMONTO As System.Windows.Forms.ComboBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents CmbComercio As System.Windows.Forms.ComboBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents txtVisor As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents TxtCvc As System.Windows.Forms.TextBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents CmbModoIngreso As System.Windows.Forms.ComboBox
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Button4 As System.Windows.Forms.Button

End Class
