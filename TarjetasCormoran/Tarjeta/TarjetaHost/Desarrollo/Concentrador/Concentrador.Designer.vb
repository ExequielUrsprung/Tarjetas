<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmConcentrador
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
        Me.btnArrancarSrv = New System.Windows.Forms.Button()
        Me.btnDetenerSrv = New System.Windows.Forms.Button()
        Me.CIERRE = New System.Windows.Forms.Button()
        Me.btnSalir = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtPosnet = New System.Windows.Forms.Label()
        Me.txtVisa = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.lblChequeando = New System.Windows.Forms.Label()
        Me.txtPinguino = New System.Windows.Forms.Label()
        Me.btnActualizar = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtValidacion = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.lblDirectorio = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblVersionTJCOMUN = New System.Windows.Forms.Label()
        Me.lblVersionISOLIB = New System.Windows.Forms.Label()
        Me.lblVersionTJSERVER = New System.Windows.Forms.Label()
        Me.lblVersionAPLICACION = New System.Windows.Forms.Label()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.lblTipo = New System.Windows.Forms.Label()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.ListView1 = New Concentrador.ListViewDoubleBuffered()
        Me.lblVersionTRANSMISOR = New System.Windows.Forms.Label()
        Me.lblVersionLIBPEI = New System.Windows.Forms.Label()
        Me.lblPEIhabilitado = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnArrancarSrv
        '
        Me.btnArrancarSrv.Location = New System.Drawing.Point(12, 38)
        Me.btnArrancarSrv.Name = "btnArrancarSrv"
        Me.btnArrancarSrv.Size = New System.Drawing.Size(145, 23)
        Me.btnArrancarSrv.TabIndex = 1
        Me.btnArrancarSrv.Text = "Arrancar Srv"
        Me.btnArrancarSrv.UseVisualStyleBackColor = True
        '
        'btnDetenerSrv
        '
        Me.btnDetenerSrv.Location = New System.Drawing.Point(12, 67)
        Me.btnDetenerSrv.Name = "btnDetenerSrv"
        Me.btnDetenerSrv.Size = New System.Drawing.Size(145, 23)
        Me.btnDetenerSrv.TabIndex = 2
        Me.btnDetenerSrv.Text = "Detener Srv"
        Me.btnDetenerSrv.UseVisualStyleBackColor = True
        '
        'CIERRE
        '
        Me.CIERRE.Location = New System.Drawing.Point(12, 230)
        Me.CIERRE.Name = "CIERRE"
        Me.CIERRE.Size = New System.Drawing.Size(145, 61)
        Me.CIERRE.TabIndex = 5
        Me.CIERRE.Text = "CIERRE"
        Me.CIERRE.UseVisualStyleBackColor = True
        '
        'btnSalir
        '
        Me.btnSalir.Location = New System.Drawing.Point(12, 653)
        Me.btnSalir.Name = "btnSalir"
        Me.btnSalir.Size = New System.Drawing.Size(145, 23)
        Me.btnSalir.TabIndex = 6
        Me.btnSalir.Text = "Salir"
        Me.btnSalir.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtPosnet)
        Me.GroupBox1.Controls.Add(Me.txtVisa)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 309)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(145, 96)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Estado Hosts"
        '
        'txtPosnet
        '
        Me.txtPosnet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtPosnet.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPosnet.Location = New System.Drawing.Point(12, 60)
        Me.txtPosnet.Name = "txtPosnet"
        Me.txtPosnet.Size = New System.Drawing.Size(125, 23)
        Me.txtPosnet.TabIndex = 1
        Me.txtPosnet.Text = "MASTER - MPLS"
        Me.txtPosnet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtVisa
        '
        Me.txtVisa.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtVisa.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtVisa.Location = New System.Drawing.Point(12, 26)
        Me.txtVisa.Name = "txtVisa"
        Me.txtVisa.Size = New System.Drawing.Size(125, 23)
        Me.txtVisa.TabIndex = 0
        Me.txtVisa.Text = "VISA - MPLS"
        Me.txtVisa.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Timer1
        '
        Me.Timer1.Interval = 200
        '
        'lblChequeando
        '
        Me.lblChequeando.AutoSize = True
        Me.lblChequeando.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChequeando.Location = New System.Drawing.Point(160, 9)
        Me.lblChequeando.Name = "lblChequeando"
        Me.lblChequeando.Size = New System.Drawing.Size(84, 16)
        Me.lblChequeando.TabIndex = 10
        Me.lblChequeando.Text = "Buscando ... "
        '
        'txtPinguino
        '
        Me.txtPinguino.AutoSize = True
        Me.txtPinguino.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold)
        Me.txtPinguino.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtPinguino.Location = New System.Drawing.Point(286, 9)
        Me.txtPinguino.Name = "txtPinguino"
        Me.txtPinguino.Size = New System.Drawing.Size(0, 16)
        Me.txtPinguino.TabIndex = 11
        Me.txtPinguino.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnActualizar
        '
        Me.btnActualizar.Location = New System.Drawing.Point(12, 96)
        Me.btnActualizar.Name = "btnActualizar"
        Me.btnActualizar.Size = New System.Drawing.Size(145, 23)
        Me.btnActualizar.TabIndex = 13
        Me.btnActualizar.Text = "Actualizar DB"
        Me.btnActualizar.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.txtValidacion)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 411)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(145, 111)
        Me.GroupBox2.TabIndex = 14
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Validación"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(25, 33)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(87, 13)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "presione ENTER"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(17, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(108, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Ingrese clave y luego"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.Red
        Me.Label2.Location = New System.Drawing.Point(36, 88)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(0, 13)
        Me.Label2.TabIndex = 2
        '
        'txtValidacion
        '
        Me.txtValidacion.Location = New System.Drawing.Point(25, 53)
        Me.txtValidacion.Name = "txtValidacion"
        Me.txtValidacion.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtValidacion.Size = New System.Drawing.Size(100, 20)
        Me.txtValidacion.TabIndex = 1
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(12, 125)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(145, 23)
        Me.Button1.TabIndex = 15
        Me.Button1.Text = "Importar Movimientos"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'lblDirectorio
        '
        Me.lblDirectorio.AutoSize = True
        Me.lblDirectorio.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblDirectorio.Location = New System.Drawing.Point(426, 9)
        Me.lblDirectorio.Name = "lblDirectorio"
        Me.lblDirectorio.Size = New System.Drawing.Size(0, 16)
        Me.lblDirectorio.TabIndex = 16
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(351, 9)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(69, 16)
        Me.Label3.TabIndex = 17
        Me.Label3.Text = "Directorio:"
        '
        'lblVersionTJCOMUN
        '
        Me.lblVersionTJCOMUN.AutoSize = True
        Me.lblVersionTJCOMUN.Location = New System.Drawing.Point(12, 525)
        Me.lblVersionTJCOMUN.Name = "lblVersionTJCOMUN"
        Me.lblVersionTJCOMUN.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionTJCOMUN.TabIndex = 18
        Me.lblVersionTJCOMUN.Text = "Label1"
        '
        'lblVersionISOLIB
        '
        Me.lblVersionISOLIB.AutoSize = True
        Me.lblVersionISOLIB.Location = New System.Drawing.Point(12, 540)
        Me.lblVersionISOLIB.Name = "lblVersionISOLIB"
        Me.lblVersionISOLIB.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionISOLIB.TabIndex = 19
        Me.lblVersionISOLIB.Text = "Label4"
        '
        'lblVersionTJSERVER
        '
        Me.lblVersionTJSERVER.AutoSize = True
        Me.lblVersionTJSERVER.Location = New System.Drawing.Point(12, 555)
        Me.lblVersionTJSERVER.Name = "lblVersionTJSERVER"
        Me.lblVersionTJSERVER.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionTJSERVER.TabIndex = 20
        Me.lblVersionTJSERVER.Text = "Label5"
        '
        'lblVersionAPLICACION
        '
        Me.lblVersionAPLICACION.AutoSize = True
        Me.lblVersionAPLICACION.Location = New System.Drawing.Point(12, 570)
        Me.lblVersionAPLICACION.Name = "lblVersionAPLICACION"
        Me.lblVersionAPLICACION.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionAPLICACION.TabIndex = 21
        Me.lblVersionAPLICACION.Text = "Label6"
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(12, 183)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(145, 23)
        Me.Button2.TabIndex = 22
        Me.Button2.Text = "Echotest"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'lblTipo
        '
        Me.lblTipo.AutoSize = True
        Me.lblTipo.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTipo.ForeColor = System.Drawing.Color.Red
        Me.lblTipo.Location = New System.Drawing.Point(937, 9)
        Me.lblTipo.Name = "lblTipo"
        Me.lblTipo.Size = New System.Drawing.Size(0, 16)
        Me.lblTipo.TabIndex = 23
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'ListView1
        '
        Me.ListView1.HideSelection = False
        Me.ListView1.Location = New System.Drawing.Point(163, 38)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(908, 638)
        Me.ListView1.TabIndex = 12
        Me.ListView1.UseCompatibleStateImageBehavior = False
        '
        'lblVersionTRANSMISOR
        '
        Me.lblVersionTRANSMISOR.AutoSize = True
        Me.lblVersionTRANSMISOR.Location = New System.Drawing.Point(12, 585)
        Me.lblVersionTRANSMISOR.Name = "lblVersionTRANSMISOR"
        Me.lblVersionTRANSMISOR.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionTRANSMISOR.TabIndex = 24
        Me.lblVersionTRANSMISOR.Text = "Label6"
        '
        'lblVersionLIBPEI
        '
        Me.lblVersionLIBPEI.AutoSize = True
        Me.lblVersionLIBPEI.Location = New System.Drawing.Point(12, 600)
        Me.lblVersionLIBPEI.Name = "lblVersionLIBPEI"
        Me.lblVersionLIBPEI.Size = New System.Drawing.Size(39, 13)
        Me.lblVersionLIBPEI.TabIndex = 25
        Me.lblVersionLIBPEI.Text = "Label6"
        '
        'lblPEIhabilitado
        '
        Me.lblPEIhabilitado.AutoSize = True
        Me.lblPEIhabilitado.Location = New System.Drawing.Point(12, 622)
        Me.lblPEIhabilitado.Name = "lblPEIhabilitado"
        Me.lblPEIhabilitado.Size = New System.Drawing.Size(39, 13)
        Me.lblPEIhabilitado.TabIndex = 26
        Me.lblPEIhabilitado.Text = "Label6"
        '
        'FrmConcentrador
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1083, 688)
        Me.ControlBox = False
        Me.Controls.Add(Me.lblPEIhabilitado)
        Me.Controls.Add(Me.lblVersionLIBPEI)
        Me.Controls.Add(Me.lblVersionTRANSMISOR)
        Me.Controls.Add(Me.lblTipo)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.lblVersionAPLICACION)
        Me.Controls.Add(Me.lblVersionTJSERVER)
        Me.Controls.Add(Me.lblVersionISOLIB)
        Me.Controls.Add(Me.lblVersionTJCOMUN)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.lblDirectorio)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.btnActualizar)
        Me.Controls.Add(Me.txtPinguino)
        Me.Controls.Add(Me.lblChequeando)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSalir)
        Me.Controls.Add(Me.CIERRE)
        Me.Controls.Add(Me.btnDetenerSrv)
        Me.Controls.Add(Me.btnArrancarSrv)
        Me.Controls.Add(Me.ListView1)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "FrmConcentrador"
        Me.Text = "Concentrador"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnArrancarSrv As System.Windows.Forms.Button
    Friend WithEvents btnDetenerSrv As System.Windows.Forms.Button
    Friend WithEvents CIERRE As System.Windows.Forms.Button
    Friend WithEvents btnSalir As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtPosnet As System.Windows.Forms.Label
    Friend WithEvents txtVisa As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents lblChequeando As System.Windows.Forms.Label
    Friend WithEvents txtPinguino As System.Windows.Forms.Label
    Friend WithEvents ListView1 As Concentrador.ListViewDoubleBuffered
    Friend WithEvents btnActualizar As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtValidacion As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents lblDirectorio As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents lblVersionTJCOMUN As Label
    Friend WithEvents lblVersionISOLIB As Label
    Friend WithEvents lblVersionTJSERVER As Label
    Friend WithEvents lblVersionAPLICACION As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents lblTipo As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents lblVersionTRANSMISOR As Label
    Friend WithEvents lblVersionLIBPEI As Label
    Friend WithEvents lblPEIhabilitado As Label
End Class
