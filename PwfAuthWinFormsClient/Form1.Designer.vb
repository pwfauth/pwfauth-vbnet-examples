<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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

    'NOTE: The following procedure is required by the Windows Form Designer.
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblSubtitle = New System.Windows.Forms.Label()
        Me.lblSecret = New System.Windows.Forms.Label()
        Me.txtSecret = New System.Windows.Forms.TextBox()
        Me.lblBaseUrl = New System.Windows.Forms.Label()
        Me.txtBaseUrl = New System.Windows.Forms.TextBox()
        Me.lblHwid = New System.Windows.Forms.Label()
        Me.tabs = New System.Windows.Forms.TabControl()
        Me.tabActivation = New System.Windows.Forms.TabPage()
        Me.lblKey = New System.Windows.Forms.Label()
        Me.txtKey = New System.Windows.Forms.TextBox()
        Me.btnCheck = New System.Windows.Forms.Button()
        Me.btnLogin = New System.Windows.Forms.Button()
        Me.btnHeartbeat = New System.Windows.Forms.Button()
        Me.btnLogout = New System.Windows.Forms.Button()
        Me.lblSession = New System.Windows.Forms.Label()
        Me.lblActNote = New System.Windows.Forms.Label()
        Me.tabTrial = New System.Windows.Forms.TabPage()
        Me.lblTrialNote = New System.Windows.Forms.Label()
        Me.btnTrial = New System.Windows.Forms.Button()
        Me.lblResetNote = New System.Windows.Forms.Label()
        Me.lblResetKey = New System.Windows.Forms.Label()
        Me.txtResetKey = New System.Windows.Forms.TextBox()
        Me.lblReason = New System.Windows.Forms.Label()
        Me.txtResetReason = New System.Windows.Forms.TextBox()
        Me.btnReset = New System.Windows.Forms.Button()
        Me.tabAccounts = New System.Windows.Forms.TabPage()
        Me.lblAccUser = New System.Windows.Forms.Label()
        Me.txtAccUser = New System.Windows.Forms.TextBox()
        Me.lblAccPass = New System.Windows.Forms.Label()
        Me.txtAccPass = New System.Windows.Forms.TextBox()
        Me.lblAccEmail = New System.Windows.Forms.Label()
        Me.txtAccEmail = New System.Windows.Forms.TextBox()
        Me.btnRegister = New System.Windows.Forms.Button()
        Me.btnAccLogin = New System.Windows.Forms.Button()
        Me.lblChangeNote = New System.Windows.Forms.Label()
        Me.txtCurPass = New System.Windows.Forms.TextBox()
        Me.txtNewPass = New System.Windows.Forms.TextBox()
        Me.btnChangePass = New System.Windows.Forms.Button()
        Me.lblAccTip = New System.Windows.Forms.Label()
        Me.lblAccKey = New System.Windows.Forms.Label()
        Me.txtAccKey = New System.Windows.Forms.TextBox()
        Me.btnRegisterKey = New System.Windows.Forms.Button()
        Me.btnRedeem = New System.Windows.Forms.Button()
        Me.lblRedeemNote = New System.Windows.Forms.Label()
        Me.tabAppInfo = New System.Windows.Forms.TabPage()
        Me.lblAppInfoNote = New System.Windows.Forms.Label()
        Me.btnAppInfo = New System.Windows.Forms.Button()
        Me.btnUpdate = New System.Windows.Forms.Button()
        Me.lblAppInfo = New System.Windows.Forms.Label()
        Me.lblLog = New System.Windows.Forms.Label()
        Me.txtLog = New System.Windows.Forms.TextBox()
        Me.tabs.SuspendLayout()
        Me.tabActivation.SuspendLayout()
        Me.tabTrial.SuspendLayout()
        Me.tabAccounts.SuspendLayout()
        Me.tabAppInfo.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 15.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(37, Byte), Integer), CType(CType(41, Byte), Integer))
        Me.lblTitle.Location = New System.Drawing.Point(18, 14)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(298, 28)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "PWF Auth — API Explorer"
        '
        'lblSubtitle
        '
        Me.lblSubtitle.AutoSize = True
        Me.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblSubtitle.Location = New System.Drawing.Point(20, 46)
        Me.lblSubtitle.Name = "lblSubtitle"
        Me.lblSubtitle.Size = New System.Drawing.Size(345, 15)
        Me.lblSubtitle.TabIndex = 1
        Me.lblSubtitle.Text = "Every client feature in one window, built on the PWFAuth NuGet package. Watch the Activity log below."
        '
        'lblSecret
        '
        Me.lblSecret.AutoSize = True
        Me.lblSecret.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblSecret.Location = New System.Drawing.Point(18, 74)
        Me.lblSecret.Name = "lblSecret"
        Me.lblSecret.Size = New System.Drawing.Size(63, 15)
        Me.lblSecret.TabIndex = 2
        Me.lblSecret.Text = "App Secret"
        '
        'txtSecret
        '
        Me.txtSecret.Location = New System.Drawing.Point(18, 94)
        Me.txtSecret.Name = "txtSecret"
        Me.txtSecret.Size = New System.Drawing.Size(644, 23)
        Me.txtSecret.TabIndex = 3
        Me.txtSecret.UseSystemPasswordChar = True
        '
        'lblBaseUrl
        '
        Me.lblBaseUrl.AutoSize = True
        Me.lblBaseUrl.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblBaseUrl.Location = New System.Drawing.Point(18, 124)
        Me.lblBaseUrl.Name = "lblBaseUrl"
        Me.lblBaseUrl.Size = New System.Drawing.Size(52, 15)
        Me.lblBaseUrl.TabIndex = 4
        Me.lblBaseUrl.Text = "Base URL"
        '
        'txtBaseUrl
        '
        Me.txtBaseUrl.Location = New System.Drawing.Point(18, 144)
        Me.txtBaseUrl.Name = "txtBaseUrl"
        Me.txtBaseUrl.Size = New System.Drawing.Size(352, 23)
        Me.txtBaseUrl.TabIndex = 5
        '
        'lblHwid
        '
        Me.lblHwid.AutoSize = True
        Me.lblHwid.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblHwid.Location = New System.Drawing.Point(380, 148)
        Me.lblHwid.Name = "lblHwid"
        Me.lblHwid.Size = New System.Drawing.Size(37, 15)
        Me.lblHwid.TabIndex = 6
        Me.lblHwid.Text = "HWID"
        '
        'tabs
        '
        Me.tabs.Controls.Add(Me.tabActivation)
        Me.tabs.Controls.Add(Me.tabTrial)
        Me.tabs.Controls.Add(Me.tabAccounts)
        Me.tabs.Controls.Add(Me.tabAppInfo)
        Me.tabs.Font = New System.Drawing.Font("Segoe UI", 9.5!)
        Me.tabs.Location = New System.Drawing.Point(18, 180)
        Me.tabs.Name = "tabs"
        Me.tabs.SelectedIndex = 0
        Me.tabs.Size = New System.Drawing.Size(644, 340)
        Me.tabs.TabIndex = 7
        '
        'tabActivation
        '
        Me.tabActivation.Controls.Add(Me.lblKey)
        Me.tabActivation.Controls.Add(Me.txtKey)
        Me.tabActivation.Controls.Add(Me.btnCheck)
        Me.tabActivation.Controls.Add(Me.btnLogin)
        Me.tabActivation.Controls.Add(Me.btnHeartbeat)
        Me.tabActivation.Controls.Add(Me.btnLogout)
        Me.tabActivation.Controls.Add(Me.lblSession)
        Me.tabActivation.Controls.Add(Me.lblActNote)
        Me.tabActivation.Location = New System.Drawing.Point(4, 26)
        Me.tabActivation.Name = "tabActivation"
        Me.tabActivation.Padding = New System.Windows.Forms.Padding(3)
        Me.tabActivation.Size = New System.Drawing.Size(636, 310)
        Me.tabActivation.TabIndex = 0
        Me.tabActivation.Text = "Activation"
        Me.tabActivation.UseVisualStyleBackColor = True
        '
        'lblKey
        '
        Me.lblKey.AutoSize = True
        Me.lblKey.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblKey.Location = New System.Drawing.Point(16, 14)
        Me.lblKey.Name = "lblKey"
        Me.lblKey.Size = New System.Drawing.Size(62, 15)
        Me.lblKey.TabIndex = 0
        Me.lblKey.Text = "License key"
        '
        'txtKey
        '
        Me.txtKey.Font = New System.Drawing.Font("Consolas", 10.5!)
        Me.txtKey.Location = New System.Drawing.Point(16, 34)
        Me.txtKey.Name = "txtKey"
        Me.txtKey.PlaceholderText = "PWF-XXXX-XXXX-XXXX"
        Me.txtKey.Size = New System.Drawing.Size(400, 25)
        Me.txtKey.TabIndex = 1
        '
        'btnCheck
        '
        Me.btnCheck.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnCheck.FlatAppearance.BorderSize = 0
        Me.btnCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCheck.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnCheck.ForeColor = System.Drawing.Color.White
        Me.btnCheck.Location = New System.Drawing.Point(424, 32)
        Me.btnCheck.Name = "btnCheck"
        Me.btnCheck.Size = New System.Drawing.Size(190, 30)
        Me.btnCheck.TabIndex = 2
        Me.btnCheck.Text = "Check key"
        Me.btnCheck.UseVisualStyleBackColor = False
        '
        'btnLogin
        '
        Me.btnLogin.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnLogin.FlatAppearance.BorderSize = 0
        Me.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnLogin.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnLogin.ForeColor = System.Drawing.Color.White
        Me.btnLogin.Location = New System.Drawing.Point(16, 78)
        Me.btnLogin.Name = "btnLogin"
        Me.btnLogin.Size = New System.Drawing.Size(190, 32)
        Me.btnLogin.TabIndex = 3
        Me.btnLogin.Text = "Login (bind HWID)"
        Me.btnLogin.UseVisualStyleBackColor = False
        '
        'btnHeartbeat
        '
        Me.btnHeartbeat.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnHeartbeat.Location = New System.Drawing.Point(214, 78)
        Me.btnHeartbeat.Name = "btnHeartbeat"
        Me.btnHeartbeat.Size = New System.Drawing.Size(190, 32)
        Me.btnHeartbeat.TabIndex = 4
        Me.btnHeartbeat.Text = "Send one heartbeat"
        Me.btnHeartbeat.UseVisualStyleBackColor = True
        '
        'btnLogout
        '
        Me.btnLogout.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnLogout.Location = New System.Drawing.Point(412, 78)
        Me.btnLogout.Name = "btnLogout"
        Me.btnLogout.Size = New System.Drawing.Size(202, 32)
        Me.btnLogout.TabIndex = 5
        Me.btnLogout.Text = "Logout"
        Me.btnLogout.UseVisualStyleBackColor = True
        '
        'lblSession
        '
        Me.lblSession.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblSession.Location = New System.Drawing.Point(16, 128)
        Me.lblSession.Name = "lblSession"
        Me.lblSession.Size = New System.Drawing.Size(600, 90)
        Me.lblSession.TabIndex = 6
        Me.lblSession.Text = "No session yet — Check a key, then Login to open one."
        '
        'lblActNote
        '
        Me.lblActNote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblActNote.Location = New System.Drawing.Point(16, 228)
        Me.lblActNote.Name = "lblActNote"
        Me.lblActNote.Size = New System.Drawing.Size(600, 66)
        Me.lblActNote.TabIndex = 7
        Me.lblActNote.Text = "Login starts the background heartbeat (StartHeartbeat). Ban or pause the key in your dashboard " &
            "and this window is signed out within one beat. Check key reads the status without using a device seat."
        '
        'tabTrial
        '
        Me.tabTrial.Controls.Add(Me.lblTrialNote)
        Me.tabTrial.Controls.Add(Me.btnTrial)
        Me.tabTrial.Controls.Add(Me.lblResetNote)
        Me.tabTrial.Controls.Add(Me.lblResetKey)
        Me.tabTrial.Controls.Add(Me.txtResetKey)
        Me.tabTrial.Controls.Add(Me.lblReason)
        Me.tabTrial.Controls.Add(Me.txtResetReason)
        Me.tabTrial.Controls.Add(Me.btnReset)
        Me.tabTrial.Location = New System.Drawing.Point(4, 26)
        Me.tabTrial.Name = "tabTrial"
        Me.tabTrial.Padding = New System.Windows.Forms.Padding(3)
        Me.tabTrial.Size = New System.Drawing.Size(636, 310)
        Me.tabTrial.TabIndex = 1
        Me.tabTrial.Text = "Trial & Reset"
        Me.tabTrial.UseVisualStyleBackColor = True
        '
        'lblTrialNote
        '
        Me.lblTrialNote.AutoSize = True
        Me.lblTrialNote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblTrialNote.Location = New System.Drawing.Point(16, 14)
        Me.lblTrialNote.Name = "lblTrialNote"
        Me.lblTrialNote.Size = New System.Drawing.Size(430, 15)
        Me.lblTrialNote.TabIndex = 0
        Me.lblTrialNote.Text = "Free trial — mints a trial key bound to this device (once per app/device)."
        '
        'btnTrial
        '
        Me.btnTrial.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnTrial.FlatAppearance.BorderSize = 0
        Me.btnTrial.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnTrial.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnTrial.ForeColor = System.Drawing.Color.White
        Me.btnTrial.Location = New System.Drawing.Point(16, 36)
        Me.btnTrial.Name = "btnTrial"
        Me.btnTrial.Size = New System.Drawing.Size(240, 32)
        Me.btnTrial.TabIndex = 1
        Me.btnTrial.Text = "Start free trial"
        Me.btnTrial.UseVisualStyleBackColor = False
        '
        'lblResetNote
        '
        Me.lblResetNote.AutoSize = True
        Me.lblResetNote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblResetNote.Location = New System.Drawing.Point(16, 96)
        Me.lblResetNote.Name = "lblResetNote"
        Me.lblResetNote.Size = New System.Drawing.Size(392, 15)
        Me.lblResetNote.TabIndex = 2
        Me.lblResetNote.Text = "Move a license to this PC — unbinds the key from its old computer (the app's cooldown applies)."
        '
        'lblResetKey
        '
        Me.lblResetKey.AutoSize = True
        Me.lblResetKey.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblResetKey.Location = New System.Drawing.Point(16, 122)
        Me.lblResetKey.Name = "lblResetKey"
        Me.lblResetKey.Size = New System.Drawing.Size(62, 15)
        Me.lblResetKey.TabIndex = 3
        Me.lblResetKey.Text = "License key"
        '
        'txtResetKey
        '
        Me.txtResetKey.Font = New System.Drawing.Font("Consolas", 10.5!)
        Me.txtResetKey.Location = New System.Drawing.Point(16, 142)
        Me.txtResetKey.Name = "txtResetKey"
        Me.txtResetKey.Size = New System.Drawing.Size(400, 25)
        Me.txtResetKey.TabIndex = 4
        '
        'lblReason
        '
        Me.lblReason.AutoSize = True
        Me.lblReason.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblReason.Location = New System.Drawing.Point(16, 178)
        Me.lblReason.Name = "lblReason"
        Me.lblReason.Size = New System.Drawing.Size(46, 15)
        Me.lblReason.TabIndex = 5
        Me.lblReason.Text = "Reason"
        '
        'txtResetReason
        '
        Me.txtResetReason.Location = New System.Drawing.Point(16, 198)
        Me.txtResetReason.Name = "txtResetReason"
        Me.txtResetReason.Size = New System.Drawing.Size(400, 23)
        Me.txtResetReason.TabIndex = 6
        Me.txtResetReason.Text = "New computer"
        '
        'btnReset
        '
        Me.btnReset.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnReset.FlatAppearance.BorderSize = 0
        Me.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnReset.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnReset.ForeColor = System.Drawing.Color.White
        Me.btnReset.Location = New System.Drawing.Point(16, 236)
        Me.btnReset.Name = "btnReset"
        Me.btnReset.Size = New System.Drawing.Size(240, 32)
        Me.btnReset.TabIndex = 7
        Me.btnReset.Text = "Move license to this PC"
        Me.btnReset.UseVisualStyleBackColor = False
        '
        'tabAccounts
        '
        Me.tabAccounts.Controls.Add(Me.lblAccUser)
        Me.tabAccounts.Controls.Add(Me.txtAccUser)
        Me.tabAccounts.Controls.Add(Me.lblAccPass)
        Me.tabAccounts.Controls.Add(Me.txtAccPass)
        Me.tabAccounts.Controls.Add(Me.lblAccEmail)
        Me.tabAccounts.Controls.Add(Me.txtAccEmail)
        Me.tabAccounts.Controls.Add(Me.lblAccKey)
        Me.tabAccounts.Controls.Add(Me.txtAccKey)
        Me.tabAccounts.Controls.Add(Me.btnRegister)
        Me.tabAccounts.Controls.Add(Me.btnRegisterKey)
        Me.tabAccounts.Controls.Add(Me.btnAccLogin)
        Me.tabAccounts.Controls.Add(Me.btnRedeem)
        Me.tabAccounts.Controls.Add(Me.lblRedeemNote)
        Me.tabAccounts.Controls.Add(Me.lblChangeNote)
        Me.tabAccounts.Controls.Add(Me.txtCurPass)
        Me.tabAccounts.Controls.Add(Me.txtNewPass)
        Me.tabAccounts.Controls.Add(Me.btnChangePass)
        Me.tabAccounts.Controls.Add(Me.lblAccTip)
        Me.tabAccounts.Location = New System.Drawing.Point(4, 26)
        Me.tabAccounts.Name = "tabAccounts"
        Me.tabAccounts.Padding = New System.Windows.Forms.Padding(3)
        Me.tabAccounts.Size = New System.Drawing.Size(636, 310)
        Me.tabAccounts.TabIndex = 2
        Me.tabAccounts.Text = "Accounts"
        Me.tabAccounts.UseVisualStyleBackColor = True
        '
        'lblAccUser
        '
        Me.lblAccUser.AutoSize = True
        Me.lblAccUser.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblAccUser.Location = New System.Drawing.Point(16, 10)
        Me.lblAccUser.Name = "lblAccUser"
        Me.lblAccUser.Size = New System.Drawing.Size(60, 15)
        Me.lblAccUser.TabIndex = 0
        Me.lblAccUser.Text = "Username"
        '
        'txtAccUser
        '
        Me.txtAccUser.Location = New System.Drawing.Point(16, 28)
        Me.txtAccUser.Name = "txtAccUser"
        Me.txtAccUser.Size = New System.Drawing.Size(290, 23)
        Me.txtAccUser.TabIndex = 1
        '
        'lblAccPass
        '
        Me.lblAccPass.AutoSize = True
        Me.lblAccPass.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblAccPass.Location = New System.Drawing.Point(320, 10)
        Me.lblAccPass.Name = "lblAccPass"
        Me.lblAccPass.Size = New System.Drawing.Size(57, 15)
        Me.lblAccPass.TabIndex = 2
        Me.lblAccPass.Text = "Password"
        '
        'txtAccPass
        '
        Me.txtAccPass.Location = New System.Drawing.Point(320, 28)
        Me.txtAccPass.Name = "txtAccPass"
        Me.txtAccPass.Size = New System.Drawing.Size(290, 23)
        Me.txtAccPass.TabIndex = 3
        Me.txtAccPass.UseSystemPasswordChar = True
        '
        'lblAccEmail
        '
        Me.lblAccEmail.AutoSize = True
        Me.lblAccEmail.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblAccEmail.Location = New System.Drawing.Point(16, 58)
        Me.lblAccEmail.Name = "lblAccEmail"
        Me.lblAccEmail.Size = New System.Drawing.Size(36, 15)
        Me.lblAccEmail.TabIndex = 4
        Me.lblAccEmail.Text = "Email"
        '
        'txtAccEmail
        '
        Me.txtAccEmail.Location = New System.Drawing.Point(16, 76)
        Me.txtAccEmail.Name = "txtAccEmail"
        Me.txtAccEmail.Size = New System.Drawing.Size(290, 23)
        Me.txtAccEmail.TabIndex = 5
        '
        'lblAccKey
        '
        Me.lblAccKey.AutoSize = True
        Me.lblAccKey.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblAccKey.Location = New System.Drawing.Point(320, 58)
        Me.lblAccKey.Name = "lblAccKey"
        Me.lblAccKey.Size = New System.Drawing.Size(230, 15)
        Me.lblAccKey.TabIndex = 6
        Me.lblAccKey.Text = "Unused license key (sign up with it, or redeem it)"
        '
        'txtAccKey
        '
        Me.txtAccKey.Font = New System.Drawing.Font("Consolas", 10.0!)
        Me.txtAccKey.Location = New System.Drawing.Point(320, 76)
        Me.txtAccKey.Name = "txtAccKey"
        Me.txtAccKey.PlaceholderText = "PWF-XXXX-XXXX-XXXX"
        Me.txtAccKey.Size = New System.Drawing.Size(290, 23)
        Me.txtAccKey.TabIndex = 7
        '
        'btnRegister
        '
        Me.btnRegister.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnRegister.FlatAppearance.BorderSize = 0
        Me.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRegister.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnRegister.ForeColor = System.Drawing.Color.White
        Me.btnRegister.Location = New System.Drawing.Point(16, 112)
        Me.btnRegister.Name = "btnRegister"
        Me.btnRegister.Size = New System.Drawing.Size(190, 30)
        Me.btnRegister.TabIndex = 8
        Me.btnRegister.Text = "Register account"
        Me.btnRegister.UseVisualStyleBackColor = False
        '
        'btnRegisterKey
        '
        Me.btnRegisterKey.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnRegisterKey.FlatAppearance.BorderSize = 0
        Me.btnRegisterKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRegisterKey.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnRegisterKey.ForeColor = System.Drawing.Color.White
        Me.btnRegisterKey.Location = New System.Drawing.Point(214, 112)
        Me.btnRegisterKey.Name = "btnRegisterKey"
        Me.btnRegisterKey.Size = New System.Drawing.Size(190, 30)
        Me.btnRegisterKey.TabIndex = 9
        Me.btnRegisterKey.Text = "Register with the key"
        Me.btnRegisterKey.UseVisualStyleBackColor = False
        '
        'btnAccLogin
        '
        Me.btnAccLogin.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnAccLogin.Location = New System.Drawing.Point(412, 112)
        Me.btnAccLogin.Name = "btnAccLogin"
        Me.btnAccLogin.Size = New System.Drawing.Size(198, 30)
        Me.btnAccLogin.TabIndex = 10
        Me.btnAccLogin.Text = "Account login"
        Me.btnAccLogin.UseVisualStyleBackColor = True
        '
        'btnRedeem
        '
        Me.btnRedeem.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnRedeem.Location = New System.Drawing.Point(16, 150)
        Me.btnRedeem.Name = "btnRedeem"
        Me.btnRedeem.Size = New System.Drawing.Size(190, 30)
        Me.btnRedeem.TabIndex = 11
        Me.btnRedeem.Text = "Redeem the key"
        Me.btnRedeem.UseVisualStyleBackColor = True
        '
        'lblRedeemNote
        '
        Me.lblRedeemNote.AutoSize = True
        Me.lblRedeemNote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblRedeemNote.Location = New System.Drawing.Point(214, 158)
        Me.lblRedeemNote.Name = "lblRedeemNote"
        Me.lblRedeemNote.Size = New System.Drawing.Size(330, 15)
        Me.lblRedeemNote.TabIndex = 12
        Me.lblRedeemNote.Text = "Adds the key's time to the account. The key is used up."
        '
        'lblChangeNote
        '
        Me.lblChangeNote.AutoSize = True
        Me.lblChangeNote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblChangeNote.Location = New System.Drawing.Point(16, 194)
        Me.lblChangeNote.Name = "lblChangeNote"
        Me.lblChangeNote.Size = New System.Drawing.Size(212, 15)
        Me.lblChangeNote.TabIndex = 13
        Me.lblChangeNote.Text = "Change password — current / new (signs the account out on every device)"
        '
        'txtCurPass
        '
        Me.txtCurPass.Location = New System.Drawing.Point(16, 214)
        Me.txtCurPass.Name = "txtCurPass"
        Me.txtCurPass.PlaceholderText = "current password"
        Me.txtCurPass.Size = New System.Drawing.Size(290, 23)
        Me.txtCurPass.TabIndex = 14
        Me.txtCurPass.UseSystemPasswordChar = True
        '
        'txtNewPass
        '
        Me.txtNewPass.Location = New System.Drawing.Point(320, 214)
        Me.txtNewPass.Name = "txtNewPass"
        Me.txtNewPass.PlaceholderText = "new password"
        Me.txtNewPass.Size = New System.Drawing.Size(290, 23)
        Me.txtNewPass.TabIndex = 15
        Me.txtNewPass.UseSystemPasswordChar = True
        '
        'btnChangePass
        '
        Me.btnChangePass.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnChangePass.FlatAppearance.BorderSize = 0
        Me.btnChangePass.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChangePass.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnChangePass.ForeColor = System.Drawing.Color.White
        Me.btnChangePass.Location = New System.Drawing.Point(16, 248)
        Me.btnChangePass.Name = "btnChangePass"
        Me.btnChangePass.Size = New System.Drawing.Size(290, 30)
        Me.btnChangePass.TabIndex = 16
        Me.btnChangePass.Text = "Change password"
        Me.btnChangePass.UseVisualStyleBackColor = False
        '
        'lblAccTip
        '
        Me.lblAccTip.AutoSize = True
        Me.lblAccTip.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblAccTip.Location = New System.Drawing.Point(320, 255)
        Me.lblAccTip.Name = "lblAccTip"
        Me.lblAccTip.Size = New System.Drawing.Size(290, 15)
        Me.lblAccTip.TabIndex = 17
        Me.lblAccTip.Text = "Tip: leave Username blank for a demo account."
        '
        'tabAppInfo
        '
        Me.tabAppInfo.Controls.Add(Me.lblAppInfoNote)
        Me.tabAppInfo.Controls.Add(Me.btnAppInfo)
        Me.tabAppInfo.Controls.Add(Me.btnUpdate)
        Me.tabAppInfo.Controls.Add(Me.lblAppInfo)
        Me.tabAppInfo.Location = New System.Drawing.Point(4, 26)
        Me.tabAppInfo.Name = "tabAppInfo"
        Me.tabAppInfo.Padding = New System.Windows.Forms.Padding(3)
        Me.tabAppInfo.Size = New System.Drawing.Size(636, 310)
        Me.tabAppInfo.TabIndex = 3
        Me.tabAppInfo.Text = "App info"
        Me.tabAppInfo.UseVisualStyleBackColor = True
        '
        'lblAppInfoNote
        '
        Me.lblAppInfoNote.AutoSize = True
        Me.lblAppInfoNote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblAppInfoNote.Location = New System.Drawing.Point(16, 14)
        Me.lblAppInfoNote.Name = "lblAppInfoNote"
        Me.lblAppInfoNote.Size = New System.Drawing.Size(470, 15)
        Me.lblAppInfoNote.TabIndex = 0
        Me.lblAppInfoNote.Text = "Public branding + current version + download URL, and the update check your app runs at start."
        '
        'btnAppInfo
        '
        Me.btnAppInfo.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.btnAppInfo.FlatAppearance.BorderSize = 0
        Me.btnAppInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAppInfo.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnAppInfo.ForeColor = System.Drawing.Color.White
        Me.btnAppInfo.Location = New System.Drawing.Point(16, 38)
        Me.btnAppInfo.Name = "btnAppInfo"
        Me.btnAppInfo.Size = New System.Drawing.Size(240, 32)
        Me.btnAppInfo.TabIndex = 1
        Me.btnAppInfo.Text = "Load app info"
        Me.btnAppInfo.UseVisualStyleBackColor = False
        '
        'btnUpdate
        '
        Me.btnUpdate.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnUpdate.Location = New System.Drawing.Point(264, 38)
        Me.btnUpdate.Name = "btnUpdate"
        Me.btnUpdate.Size = New System.Drawing.Size(240, 32)
        Me.btnUpdate.TabIndex = 3
        Me.btnUpdate.Text = "Check for an update"
        Me.btnUpdate.UseVisualStyleBackColor = True
        '
        'lblAppInfo
        '
        Me.lblAppInfo.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.lblAppInfo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(37, Byte), Integer), CType(CType(41, Byte), Integer))
        Me.lblAppInfo.Location = New System.Drawing.Point(16, 90)
        Me.lblAppInfo.Name = "lblAppInfo"
        Me.lblAppInfo.Size = New System.Drawing.Size(600, 200)
        Me.lblAppInfo.TabIndex = 2
        Me.lblAppInfo.Text = "Click ""Load app info"" to fetch the app's public metadata."
        '
        'lblLog
        '
        Me.lblLog.AutoSize = True
        Me.lblLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.lblLog.Location = New System.Drawing.Point(18, 528)
        Me.lblLog.Name = "lblLog"
        Me.lblLog.Size = New System.Drawing.Size(70, 15)
        Me.lblLog.TabIndex = 8
        Me.lblLog.Text = "Activity log"
        '
        'txtLog
        '
        Me.txtLog.BackColor = System.Drawing.Color.FromArgb(CType(CType(248, Byte), Integer), CType(CType(249, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.txtLog.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(37, Byte), Integer), CType(CType(41, Byte), Integer))
        Me.txtLog.Location = New System.Drawing.Point(18, 548)
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ReadOnly = True
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLog.Size = New System.Drawing.Size(644, 158)
        Me.txtLog.TabIndex = 9
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(680, 720)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.lblSubtitle)
        Me.Controls.Add(Me.lblSecret)
        Me.Controls.Add(Me.txtSecret)
        Me.Controls.Add(Me.lblBaseUrl)
        Me.Controls.Add(Me.txtBaseUrl)
        Me.Controls.Add(Me.lblHwid)
        Me.Controls.Add(Me.tabs)
        Me.Controls.Add(Me.lblLog)
        Me.Controls.Add(Me.txtLog)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.ForeColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(37, Byte), Integer), CType(CType(41, Byte), Integer))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "PWF Auth — API Explorer"
        Me.tabs.ResumeLayout(False)
        Me.tabActivation.ResumeLayout(False)
        Me.tabActivation.PerformLayout()
        Me.tabTrial.ResumeLayout(False)
        Me.tabTrial.PerformLayout()
        Me.tabAccounts.ResumeLayout(False)
        Me.tabAccounts.PerformLayout()
        Me.tabAppInfo.ResumeLayout(False)
        Me.tabAppInfo.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblSubtitle As System.Windows.Forms.Label
    Friend WithEvents lblSecret As System.Windows.Forms.Label
    Friend WithEvents txtSecret As System.Windows.Forms.TextBox
    Friend WithEvents lblBaseUrl As System.Windows.Forms.Label
    Friend WithEvents txtBaseUrl As System.Windows.Forms.TextBox
    Friend WithEvents lblHwid As System.Windows.Forms.Label
    Friend WithEvents tabs As System.Windows.Forms.TabControl
    Friend WithEvents tabActivation As System.Windows.Forms.TabPage
    Friend WithEvents lblKey As System.Windows.Forms.Label
    Friend WithEvents txtKey As System.Windows.Forms.TextBox
    Friend WithEvents btnCheck As System.Windows.Forms.Button
    Friend WithEvents btnLogin As System.Windows.Forms.Button
    Friend WithEvents btnHeartbeat As System.Windows.Forms.Button
    Friend WithEvents btnLogout As System.Windows.Forms.Button
    Friend WithEvents lblSession As System.Windows.Forms.Label
    Friend WithEvents lblActNote As System.Windows.Forms.Label
    Friend WithEvents tabTrial As System.Windows.Forms.TabPage
    Friend WithEvents lblTrialNote As System.Windows.Forms.Label
    Friend WithEvents btnTrial As System.Windows.Forms.Button
    Friend WithEvents lblResetNote As System.Windows.Forms.Label
    Friend WithEvents lblResetKey As System.Windows.Forms.Label
    Friend WithEvents txtResetKey As System.Windows.Forms.TextBox
    Friend WithEvents lblReason As System.Windows.Forms.Label
    Friend WithEvents txtResetReason As System.Windows.Forms.TextBox
    Friend WithEvents btnReset As System.Windows.Forms.Button
    Friend WithEvents tabAccounts As System.Windows.Forms.TabPage
    Friend WithEvents lblAccUser As System.Windows.Forms.Label
    Friend WithEvents txtAccUser As System.Windows.Forms.TextBox
    Friend WithEvents lblAccPass As System.Windows.Forms.Label
    Friend WithEvents txtAccPass As System.Windows.Forms.TextBox
    Friend WithEvents lblAccEmail As System.Windows.Forms.Label
    Friend WithEvents txtAccEmail As System.Windows.Forms.TextBox
    Friend WithEvents btnRegister As System.Windows.Forms.Button
    Friend WithEvents btnAccLogin As System.Windows.Forms.Button
    Friend WithEvents lblChangeNote As System.Windows.Forms.Label
    Friend WithEvents txtCurPass As System.Windows.Forms.TextBox
    Friend WithEvents txtNewPass As System.Windows.Forms.TextBox
    Friend WithEvents btnChangePass As System.Windows.Forms.Button
    Friend WithEvents lblAccTip As System.Windows.Forms.Label
    Friend WithEvents lblAccKey As System.Windows.Forms.Label
    Friend WithEvents txtAccKey As System.Windows.Forms.TextBox
    Friend WithEvents btnRegisterKey As System.Windows.Forms.Button
    Friend WithEvents btnRedeem As System.Windows.Forms.Button
    Friend WithEvents lblRedeemNote As System.Windows.Forms.Label
    Friend WithEvents tabAppInfo As System.Windows.Forms.TabPage
    Friend WithEvents lblAppInfoNote As System.Windows.Forms.Label
    Friend WithEvents btnAppInfo As System.Windows.Forms.Button
    Friend WithEvents btnUpdate As System.Windows.Forms.Button
    Friend WithEvents lblAppInfo As System.Windows.Forms.Label
    Friend WithEvents lblLog As System.Windows.Forms.Label
    Friend WithEvents txtLog As System.Windows.Forms.TextBox
End Class
