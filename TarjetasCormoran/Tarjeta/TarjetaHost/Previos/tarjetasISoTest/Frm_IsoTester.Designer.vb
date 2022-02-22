<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Frm_IsoTester
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
        Me.Button1 = New System.Windows.Forms.Button
        Me.ListBox1 = New System.Windows.Forms.ListBox
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.Label15 = New System.Windows.Forms.Label
        Me.ComboBox3 = New System.Windows.Forms.ComboBox
        Me.Label12 = New System.Windows.Forms.Label
        Me.edtREverso = New System.Windows.Forms.ComboBox
        Me.Button8 = New System.Windows.Forms.Button
        Me.Label11 = New System.Windows.Forms.Label
        Me.Label10 = New System.Windows.Forms.Label
        Me.CmbTransaccion = New System.Windows.Forms.ComboBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.CmbTerm = New System.Windows.Forms.ComboBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.Label6 = New System.Windows.Forms.Label
        Me.edtvto = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.EdtnroCuotas = New System.Windows.Forms.NumericUpDown
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.edtFecTrans = New System.Windows.Forms.DateTimePicker
        Me.edtfecnegocio = New System.Windows.Forms.DateTimePicker
        Me.EdtMonto = New System.Windows.Forms.ComboBox
        Me.CmbPlastico = New System.Windows.Forms.ComboBox
        Me.Button6 = New System.Windows.Forms.Button
        Me.ComboBox2 = New System.Windows.Forms.ComboBox
        Me.Button5 = New System.Windows.Forms.Button
        Me.Button2 = New System.Windows.Forms.Button
        Me.Label7 = New System.Windows.Forms.Label
        Me.EdtRespuesta = New System.Windows.Forms.TextBox
        Me.Button3 = New System.Windows.Forms.Button
        Me.Button4 = New System.Windows.Forms.Button
        Me.ComboBox1 = New System.Windows.Forms.ComboBox
        Me.Button7 = New System.Windows.Forms.Button
        Me.CmbIP = New System.Windows.Forms.ComboBox
        Me.ListBox2 = New System.Windows.Forms.ListBox
        Me.Label13 = New System.Windows.Forms.Label
        Me.Label14 = New System.Windows.Forms.Label
        Me.CheckBox1 = New System.Windows.Forms.CheckBox
        Me.GroupBox1.SuspendLayout()
        CType(Me.EdtnroCuotas, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(18, 6)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(84, 22)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Conectar"
        Me.Button1.UseVisualStyleBackColor = True
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
        Me.ListBox1.Location = New System.Drawing.Point(404, 14)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(384, 268)
        Me.ListBox1.TabIndex = 1
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 200
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label15)
        Me.GroupBox1.Controls.Add(Me.ComboBox3)
        Me.GroupBox1.Controls.Add(Me.Label12)
        Me.GroupBox1.Controls.Add(Me.edtREverso)
        Me.GroupBox1.Controls.Add(Me.Button8)
        Me.GroupBox1.Controls.Add(Me.Label11)
        Me.GroupBox1.Controls.Add(Me.Label10)
        Me.GroupBox1.Controls.Add(Me.CmbTransaccion)
        Me.GroupBox1.Controls.Add(Me.Label9)
        Me.GroupBox1.Controls.Add(Me.CmbTerm)
        Me.GroupBox1.Controls.Add(Me.Label8)
        Me.GroupBox1.Controls.Add(Me.Label6)
        Me.GroupBox1.Controls.Add(Me.edtvto)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.EdtnroCuotas)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.edtFecTrans)
        Me.GroupBox1.Controls.Add(Me.edtfecnegocio)
        Me.GroupBox1.Controls.Add(Me.EdtMonto)
        Me.GroupBox1.Controls.Add(Me.CmbPlastico)
        Me.GroupBox1.Controls.Add(Me.Button6)
        Me.GroupBox1.Controls.Add(Me.ComboBox2)
        Me.GroupBox1.Controls.Add(Me.Button5)
        Me.GroupBox1.Controls.Add(Me.Button2)
        Me.GroupBox1.Enabled = False
        Me.GroupBox1.Location = New System.Drawing.Point(10, 64)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(266, 346)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Mensajes"
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(18, 236)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(67, 13)
        Me.Label15.TabIndex = 33
        Me.Label15.Text = "Codigo 0800"
        '
        'ComboBox3
        '
        Me.ComboBox3.FormattingEnabled = True
        Me.ComboBox3.Items.AddRange(New Object() {"301", "1", "0"})
        Me.ComboBox3.Location = New System.Drawing.Point(130, 234)
        Me.ComboBox3.Name = "ComboBox3"
        Me.ComboBox3.Size = New System.Drawing.Size(119, 21)
        Me.ComboBox3.TabIndex = 32
        Me.ComboBox3.Text = "301"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(14, 210)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(92, 13)
        Me.Label12.TabIndex = 31
        Me.Label12.Text = "Monto Reversado"
        '
        'edtREverso
        '
        Me.edtREverso.AllowDrop = True
        Me.edtREverso.AutoCompleteCustomSource.AddRange(New String() {"628161", "6281610038744002"})
        Me.edtREverso.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.edtREverso.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.edtREverso.FormatString = "N2"
        Me.edtREverso.FormattingEnabled = True
        Me.edtREverso.Location = New System.Drawing.Point(128, 206)
        Me.edtREverso.MaxLength = 7
        Me.edtREverso.Name = "edtREverso"
        Me.edtREverso.Size = New System.Drawing.Size(124, 21)
        Me.edtREverso.TabIndex = 30
        Me.edtREverso.Text = "123,45"
        '
        'Button8
        '
        Me.Button8.Location = New System.Drawing.Point(25, 285)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(70, 22)
        Me.Button8.TabIndex = 29
        Me.Button8.Text = "0420"
        Me.Button8.UseVisualStyleBackColor = True
        '
        'Label11
        '
        Me.Label11.Location = New System.Drawing.Point(13, 266)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(121, 16)
        Me.Label11.TabIndex = 28
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(15, 23)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(66, 13)
        Me.Label10.TabIndex = 27
        Me.Label10.Text = "Transaccion"
        '
        'CmbTransaccion
        '
        Me.CmbTransaccion.FormattingEnabled = True
        Me.CmbTransaccion.Location = New System.Drawing.Point(104, 20)
        Me.CmbTransaccion.Name = "CmbTransaccion"
        Me.CmbTransaccion.Size = New System.Drawing.Size(151, 21)
        Me.CmbTransaccion.TabIndex = 26
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(14, 184)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(47, 13)
        Me.Label9.TabIndex = 25
        Me.Label9.Text = "Terminal"
        '
        'CmbTerm
        '
        Me.CmbTerm.AllowDrop = True
        Me.CmbTerm.AutoCompleteCustomSource.AddRange(New String() {"628161", "6281610038744002"})
        Me.CmbTerm.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.CmbTerm.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.CmbTerm.FormatString = "N2"
        Me.CmbTerm.FormattingEnabled = True
        Me.CmbTerm.Items.AddRange(New Object() {"75801234", "S1234566"})
        Me.CmbTerm.Location = New System.Drawing.Point(152, 180)
        Me.CmbTerm.MaxLength = 7
        Me.CmbTerm.Name = "CmbTerm"
        Me.CmbTerm.Size = New System.Drawing.Size(100, 21)
        Me.CmbTerm.TabIndex = 24
        Me.CmbTerm.Text = "S1234567"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(176, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(76, 13)
        Me.Label8.TabIndex = 7
        Me.Label8.Text = "No Conectado"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(174, 160)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(23, 13)
        Me.Label6.TabIndex = 21
        Me.Label6.Text = "Vto"
        '
        'edtvto
        '
        Me.edtvto.Location = New System.Drawing.Point(204, 156)
        Me.edtvto.MaxLength = 4
        Me.edtvto.Name = "edtvto"
        Me.edtvto.Size = New System.Drawing.Size(47, 20)
        Me.edtvto.TabIndex = 20
        Me.edtvto.Text = "1107"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(13, 160)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(40, 13)
        Me.Label5.TabIndex = 19
        Me.Label5.Text = "Cuotas"
        '
        'EdtnroCuotas
        '
        Me.EdtnroCuotas.Location = New System.Drawing.Point(98, 156)
        Me.EdtnroCuotas.Name = "EdtnroCuotas"
        Me.EdtnroCuotas.Size = New System.Drawing.Size(66, 20)
        Me.EdtnroCuotas.TabIndex = 18
        Me.EdtnroCuotas.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(11, 132)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(67, 13)
        Me.Label4.TabIndex = 17
        Me.Label4.Text = "Fecha Trans"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(11, 106)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(80, 13)
        Me.Label3.TabIndex = 16
        Me.Label3.Text = "Fecha Negocio"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 80)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(37, 13)
        Me.Label2.TabIndex = 15
        Me.Label2.Text = "Monto"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(11, 56)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(44, 13)
        Me.Label1.TabIndex = 14
        Me.Label1.Text = "Plastico"
        '
        'edtFecTrans
        '
        Me.edtFecTrans.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.edtFecTrans.Location = New System.Drawing.Point(127, 132)
        Me.edtFecTrans.Name = "edtFecTrans"
        Me.edtFecTrans.Size = New System.Drawing.Size(126, 20)
        Me.edtFecTrans.TabIndex = 13
        '
        'edtfecnegocio
        '
        Me.edtfecnegocio.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.edtfecnegocio.Location = New System.Drawing.Point(127, 104)
        Me.edtfecnegocio.Name = "edtfecnegocio"
        Me.edtfecnegocio.Size = New System.Drawing.Size(126, 20)
        Me.edtfecnegocio.TabIndex = 12
        '
        'EdtMonto
        '
        Me.EdtMonto.AllowDrop = True
        Me.EdtMonto.AutoCompleteCustomSource.AddRange(New String() {"628161", "6281610038744002"})
        Me.EdtMonto.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.EdtMonto.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.EdtMonto.FormatString = "N2"
        Me.EdtMonto.FormattingEnabled = True
        Me.EdtMonto.Location = New System.Drawing.Point(129, 74)
        Me.EdtMonto.MaxLength = 7
        Me.EdtMonto.Name = "EdtMonto"
        Me.EdtMonto.Size = New System.Drawing.Size(124, 21)
        Me.EdtMonto.TabIndex = 11
        Me.EdtMonto.Text = "123,45"
        '
        'CmbPlastico
        '
        Me.CmbPlastico.AllowDrop = True
        Me.CmbPlastico.AutoCompleteCustomSource.AddRange(New String() {"628161", "6281610038744002"})
        Me.CmbPlastico.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.CmbPlastico.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.CmbPlastico.FormatString = "N0"
        Me.CmbPlastico.FormattingEnabled = True
        Me.CmbPlastico.Items.AddRange(New Object() {"6281610015496022", "6281610033766018", "6281610033770028", "6281610034079015", "6281610034429012", "6281610039549012", "6281610041964019", "6281610058537013"})
        Me.CmbPlastico.Location = New System.Drawing.Point(129, 48)
        Me.CmbPlastico.MaxLength = 18
        Me.CmbPlastico.Name = "CmbPlastico"
        Me.CmbPlastico.Size = New System.Drawing.Size(124, 21)
        Me.CmbPlastico.TabIndex = 10
        Me.CmbPlastico.Text = "6281610038744002"
        '
        'Button6
        '
        Me.Button6.Location = New System.Drawing.Point(201, 310)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(52, 24)
        Me.Button6.TabIndex = 9
        Me.Button6.Text = "Mandar"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'ComboBox2
        '
        Me.ComboBox2.FormattingEnabled = True
        Me.ComboBox2.Items.AddRange(New Object() {"Banelco 0800", "Banelco 0810", "Banelco 0200", "Banelco 0210", "Banelco 0420", "Banelco 0430"})
        Me.ComboBox2.Location = New System.Drawing.Point(53, 313)
        Me.ComboBox2.Name = "ComboBox2"
        Me.ComboBox2.Size = New System.Drawing.Size(130, 21)
        Me.ComboBox2.TabIndex = 8
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(107, 285)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(70, 22)
        Me.Button5.TabIndex = 2
        Me.Button5.Text = "0200"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(183, 285)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(70, 22)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "0800"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(406, 290)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(80, 13)
        Me.Label7.TabIndex = 23
        Me.Label7.Text = "Cod.Respuesta"
        '
        'EdtRespuesta
        '
        Me.EdtRespuesta.Location = New System.Drawing.Point(492, 286)
        Me.EdtRespuesta.Name = "EdtRespuesta"
        Me.EdtRespuesta.Size = New System.Drawing.Size(178, 20)
        Me.EdtRespuesta.TabIndex = 22
        '
        'Button3
        '
        Me.Button3.Enabled = False
        Me.Button3.Location = New System.Drawing.Point(16, 34)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(84, 22)
        Me.Button3.TabIndex = 3
        Me.Button3.Text = "DesConectar"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(692, 288)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(92, 24)
        Me.Button4.TabIndex = 4
        Me.Button4.Text = "Borrar"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'ComboBox1
        '
        Me.ComboBox1.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Items.AddRange(New Object() {"Link", "Banelco"})
        Me.ComboBox1.Location = New System.Drawing.Point(124, 36)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(114, 21)
        Me.ComboBox1.TabIndex = 5
        Me.ComboBox1.Text = "Banelco"
        '
        'Button7
        '
        Me.Button7.Location = New System.Drawing.Point(690, 318)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(92, 42)
        Me.Button7.TabIndex = 6
        Me.Button7.Text = "Borrar Autorizaciones"
        Me.Button7.UseVisualStyleBackColor = True
        '
        'CmbIP
        '
        Me.CmbIP.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.CmbIP.FormattingEnabled = True
        Me.CmbIP.Items.AddRange(New Object() {"10.2.0.94", "127.0.0.1", "172.100.1.28"})
        Me.CmbIP.Location = New System.Drawing.Point(124, 8)
        Me.CmbIP.Name = "CmbIP"
        Me.CmbIP.Size = New System.Drawing.Size(114, 21)
        Me.CmbIP.TabIndex = 7
        Me.CmbIP.Text = "127.0.0.1"
        '
        'ListBox2
        '
        Me.ListBox2.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListBox2.FormattingEnabled = True
        Me.ListBox2.ItemHeight = 11
        Me.ListBox2.Location = New System.Drawing.Point(406, 318)
        Me.ListBox2.Name = "ListBox2"
        Me.ListBox2.Size = New System.Drawing.Size(264, 147)
        Me.ListBox2.TabIndex = 8
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(462, 8)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(61, 13)
        Me.Label13.TabIndex = 9
        Me.Label13.Text = "Log Interno"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(556, 312)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(108, 13)
        Me.Label14.TabIndex = 10
        Me.Label14.Text = "Respuesta Extendida"
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(264, 14)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(71, 17)
        Me.CheckBox1.TabIndex = 24
        Me.CheckBox1.Text = "Escuchar"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'Frm_IsoTester
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(794, 526)
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.Label14)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.ListBox2)
        Me.Controls.Add(Me.CmbIP)
        Me.Controls.Add(Me.Button7)
        Me.Controls.Add(Me.ComboBox1)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.EdtRespuesta)
        Me.Controls.Add(Me.ListBox1)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Frm_IsoTester"
        Me.Text = "Torturador ISO"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.EdtnroCuotas, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents Button5 As System.Windows.Forms.Button
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents Button6 As System.Windows.Forms.Button
    Friend WithEvents ComboBox2 As System.Windows.Forms.ComboBox
    Friend WithEvents CmbPlastico As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents edtFecTrans As System.Windows.Forms.DateTimePicker
    Friend WithEvents edtfecnegocio As System.Windows.Forms.DateTimePicker
    Friend WithEvents EdtMonto As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents EdtnroCuotas As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents edtvto As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents EdtRespuesta As System.Windows.Forms.TextBox
    Friend WithEvents Button7 As System.Windows.Forms.Button
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents CmbIP As System.Windows.Forms.ComboBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents CmbTerm As System.Windows.Forms.ComboBox
    Friend WithEvents ListBox2 As System.Windows.Forms.ListBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents CmbTransaccion As System.Windows.Forms.ComboBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Button8 As System.Windows.Forms.Button
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents edtREverso As System.Windows.Forms.ComboBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents ComboBox3 As System.Windows.Forms.ComboBox
End Class
