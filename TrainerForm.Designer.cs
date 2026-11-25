using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

partial class TrainerForm
{
    private IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new Container();

        layout = new TableLayoutPanel();
        header = new Panel();
        titleLabel = new Label();
        subtitleLabel = new Label();
        languageLabel = new Label();

        body = new TableLayoutPanel();
        leftPanel = new Panel();
        coverArt = new PictureBox();
        lblProcess = new Label();
        lblPid = new Label();
        lblProcessStatus = new Label();
        lblCredit = new Label();

        features = new TableLayoutPanel();

        hotkeyHeader = new Label();
        cheatHeader = new Label();
        valueHeader = new Label();

        hotkey1 = new Label();
        hotkey2 = new Label();
        hotkey3 = new Label();
        hotkey4 = new Label();
        hotkey5 = new Label();
        hotkey6 = new Label();
        hotkey7 = new Label();
        hotkey8 = new Label();
        hotkey9 = new Label();
        hotkey10 = new Label();
        hotkey11 = new Label();
        hotkey12 = new Label();
        hotkey13 = new Label();
        hotkey14 = new Label();
        hotkey15 = new Label();
        hotkey16 = new Label();
        hotkeyHome = new Label();

        btnRefillHealth = new TrainerFeatureButton();
        btnRefillBattery = new TrainerFeatureButton();
        btnUnlimitedHealth = new TrainerFeatureButton();
        btnUnlimitedBattery = new TrainerFeatureButton();
        btnCarryCapacity = new TrainerFeatureButton();
        btnWalkingSpeed = new TrainerFeatureButton();
        btnSwimmingSpeed = new TrainerFeatureButton();
        btnJumpVelocity = new TrainerFeatureButton();
        btnLowGravity = new TrainerFeatureButton();
        btnTimeSpeed = new TrainerFeatureButton();
        btnCameraFov = new TrainerFeatureButton();
        btnFreeCrafting = new TrainerFeatureButton();

        btnClearSkies = new TrainerFeatureButton();
        btnDaytime = new TrainerFeatureButton();
        btnCalmOcean = new TrainerFeatureButton();
        btnNoLightning = new TrainerFeatureButton();

        btnReset = new TrainerFeatureButton();

        trkWalk = new TrackBar();
        lblWalkValue = new Label();
        trkSwim = new TrackBar();
        lblSwimValue = new Label();
        trkJump = new TrackBar();
        lblJumpValue = new Label();
        trkGravity = new TrackBar();
        lblGravityValue = new Label();
        trkGameSpeed = new TrackBar();
        lblGameSpeedValue = new Label();
        trkFov = new TrackBar();
        lblFovValue = new Label();

        footer = new Panel();
        advancedMenu = new ContextMenuStrip(components);
        menuAttach = new ToolStripMenuItem();
        menuAddresses = new ToolStripMenuItem();
        state = new Label();

        bar = new FlowLayoutPanel();
        chkAlwaysOnTop = new CheckBox();
        chkSound = new CheckBox();
        footerCredit = new Label();

        btnResolve = new Button();
        btnAddresses = new Button();

        diagnosticsLabel = new Label();
        log = new TextBox();
        diagnosticsPanel = new Panel();

        fallback = new Timer(components);
        timer1 = new Timer(components);

        layout.SuspendLayout();
        header.SuspendLayout();
        body.SuspendLayout();
        leftPanel.SuspendLayout();

        ((ISupportInitialize)coverArt).BeginInit();

        features.SuspendLayout();

        ((ISupportInitialize)trkWalk).BeginInit();
        ((ISupportInitialize)trkSwim).BeginInit();
        ((ISupportInitialize)trkJump).BeginInit();
        ((ISupportInitialize)trkGravity).BeginInit();
        ((ISupportInitialize)trkGameSpeed).BeginInit();
        ((ISupportInitialize)trkFov).BeginInit();

        footer.SuspendLayout();
        advancedMenu.SuspendLayout();
        bar.SuspendLayout();
        diagnosticsPanel.SuspendLayout();

        SuspendLayout();

        //
        // layout
        //
        layout.BackColor = Color.FromArgb(46, 46, 46);
        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(body, 0, 1);
        layout.Controls.Add(footer, 0, 2);

        layout.Dock = DockStyle.Fill;
        layout.Location = new Point(0, 0);
        layout.Margin = new Padding(4);
        layout.Name = "layout";
        layout.Padding = new Padding(1);

        layout.RowCount = 3;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

        layout.Size = new Size(1000, 812);
        layout.TabIndex = 1;

        //
        // header
        //
        header.BackColor = Color.FromArgb(26, 26, 26);

        header.Controls.Add(titleLabel);
        header.Controls.Add(subtitleLabel);
        header.Controls.Add(languageLabel);

        header.Dock = DockStyle.Fill;
        header.Location = new Point(1, 1);
        header.Margin = new Padding(0, 0, 0, 1);
        header.Name = "header";
        header.Size = new Size(998, 67);
        header.TabIndex = 0;

        //
        // titleLabel
        //
        titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        titleLabel.ForeColor = Color.FromArgb(235, 182, 43);
        titleLabel.Location = new Point(15, 4);
        titleLabel.Margin = new Padding(4, 0, 4, 0);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(775, 32);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "THE LAST CARETAKER";
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // subtitleLabel
        //
        subtitleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        subtitleLabel.ForeColor = Color.Gainsboro;
        subtitleLabel.Location = new Point(18, 35);
        subtitleLabel.Margin = new Padding(4, 0, 4, 0);
        subtitleLabel.Name = "subtitleLabel";
        subtitleLabel.Size = new Size(762, 26);
        subtitleLabel.TabIndex = 1;
        subtitleLabel.Text = "v6  •  Plus 16 Trainer";
        subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // languageLabel
        //
        languageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        languageLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        languageLabel.ForeColor = Color.FromArgb(235, 182, 43);
        languageLabel.Location = new Point(885, 9);
        languageLabel.Margin = new Padding(4, 0, 4, 0);
        languageLabel.Name = "languageLabel";
        languageLabel.Size = new Size(92, 50);
        languageLabel.TabIndex = 2;
        languageLabel.Text = "ENG";
        languageLabel.TextAlign = ContentAlignment.MiddleRight;

        //
        // body
        //
        body.BackColor = Color.FromArgb(17, 17, 17);

        body.ColumnCount = 2;
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        body.Controls.Add(leftPanel, 0, 0);
        body.Controls.Add(features, 1, 0);

        body.Dock = DockStyle.Fill;
        body.Location = new Point(1, 69);
        body.Margin = new Padding(0, 0, 0, 1);
        body.Name = "body";

        body.RowCount = 1;
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        body.Size = new Size(998, 691);
        body.TabIndex = 1;

        //
        // leftPanel
        //
        leftPanel.BackColor = Color.FromArgb(26, 26, 26);

        leftPanel.Controls.Add(coverArt);
        leftPanel.Controls.Add(lblProcess);
        leftPanel.Controls.Add(lblPid);
        leftPanel.Controls.Add(lblProcessStatus);
        leftPanel.Controls.Add(lblCredit);

        leftPanel.Dock = DockStyle.Fill;
        leftPanel.Location = new Point(0, 0);
        leftPanel.Margin = new Padding(0, 0, 1, 0);
        leftPanel.Name = "leftPanel";
        leftPanel.Size = new Size(279, 691);
        leftPanel.TabIndex = 0;

        //
        // coverArt
        //
        coverArt.BackColor = Color.FromArgb(17, 17, 17);
        coverArt.BorderStyle = BorderStyle.FixedSingle;

        coverArt.ImageLocation =
            "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1783560/f09cecef778ea213c6ab144675f1ca69f9245c22/library_capsule.jpg";

        coverArt.Location = new Point(6, 6);
        coverArt.Margin = new Padding(4);
        coverArt.Name = "coverArt";
        coverArt.Size = new Size(267, 401);
        coverArt.SizeMode = PictureBoxSizeMode.StretchImage;
        coverArt.TabIndex = 0;
        coverArt.TabStop = false;

        //
        // lblProcess
        //
        lblProcess.Font = new Font("Segoe UI", 8F);
        lblProcess.ForeColor = Color.Gainsboro;
        lblProcess.Location = new Point(10, 415);
        lblProcess.Margin = new Padding(4, 0, 4, 0);
        lblProcess.Name = "lblProcess";
        lblProcess.Size = new Size(264, 42);
        lblProcess.TabIndex = 1;
        lblProcess.Text = "Game Process:\r\nVoyageSteam-Win64-Shipping.exe";

        //
        // lblPid
        //
        lblPid.Font = new Font("Segoe UI", 8F);
        lblPid.ForeColor = Color.FromArgb(170, 170, 170);
        lblPid.Location = new Point(10, 462);
        lblPid.Margin = new Padding(4, 0, 4, 0);
        lblPid.Name = "lblPid";
        lblPid.Size = new Size(264, 22);
        lblPid.TabIndex = 2;
        lblPid.Text = "Process ID: —";

        //
        // lblProcessStatus
        //
        lblProcessStatus.Font = new Font("Segoe UI", 8F);
        lblProcessStatus.ForeColor = Color.FromArgb(235, 182, 43);
        lblProcessStatus.Location = new Point(10, 489);
        lblProcessStatus.Margin = new Padding(4, 0, 4, 0);
        lblProcessStatus.Name = "lblProcessStatus";
        lblProcessStatus.Size = new Size(264, 25);
        lblProcessStatus.TabIndex = 3;
        lblProcessStatus.Text = "Status: Waiting for game";

        //
        // lblCredit
        //
        lblCredit.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblCredit.ForeColor = Color.FromArgb(170, 170, 170);
        lblCredit.Location = new Point(10, 516);
        lblCredit.Margin = new Padding(4, 0, 4, 0);
        lblCredit.Name = "lblCredit";
        lblCredit.Size = new Size(264, 28);
        lblCredit.TabIndex = 4;
        lblCredit.Text = "xDPx";

        //
        // features
        //
        features.BackColor = Color.FromArgb(17, 17, 17);

        features.ColumnCount = 4;
        features.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
        features.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        features.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        features.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66F));

        features.RowCount = 19;

        // Header
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        // Existing 12 options
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 1
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 2
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 3
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 4
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 5
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 6
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 7
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 8
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 9
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 10
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 11
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 12

        // New weather options
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 13
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 14
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 15
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // 16

        // Home and remaining space
        features.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        features.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        features.Controls.Add(hotkeyHeader, 0, 0);
        features.Controls.Add(cheatHeader, 1, 0);
        features.Controls.Add(valueHeader, 3, 0);

        features.Controls.Add(hotkey1, 0, 1);
        features.Controls.Add(btnRefillHealth, 1, 1);

        features.Controls.Add(hotkey2, 0, 2);
        features.Controls.Add(btnRefillBattery, 1, 2);

        features.Controls.Add(hotkey3, 0, 3);
        features.Controls.Add(btnUnlimitedHealth, 1, 3);

        features.Controls.Add(hotkey4, 0, 4);
        features.Controls.Add(btnUnlimitedBattery, 1, 4);

        features.Controls.Add(hotkey5, 0, 5);
        features.Controls.Add(btnCarryCapacity, 1, 5);

        features.Controls.Add(hotkey6, 0, 6);
        features.Controls.Add(btnWalkingSpeed, 1, 6);
        features.Controls.Add(trkWalk, 2, 6);
        features.Controls.Add(lblWalkValue, 3, 6);

        features.Controls.Add(hotkey7, 0, 7);
        features.Controls.Add(btnSwimmingSpeed, 1, 7);
        features.Controls.Add(trkSwim, 2, 7);
        features.Controls.Add(lblSwimValue, 3, 7);

        features.Controls.Add(hotkey8, 0, 8);
        features.Controls.Add(btnJumpVelocity, 1, 8);
        features.Controls.Add(trkJump, 2, 8);
        features.Controls.Add(lblJumpValue, 3, 8);

        features.Controls.Add(hotkey9, 0, 9);
        features.Controls.Add(btnLowGravity, 1, 9);
        features.Controls.Add(trkGravity, 2, 9);
        features.Controls.Add(lblGravityValue, 3, 9);

        features.Controls.Add(hotkey10, 0, 10);
        features.Controls.Add(btnTimeSpeed, 1, 10);
        features.Controls.Add(trkGameSpeed, 2, 10);
        features.Controls.Add(lblGameSpeedValue, 3, 10);

        features.Controls.Add(hotkey11, 0, 11);
        features.Controls.Add(btnCameraFov, 1, 11);
        features.Controls.Add(trkFov, 2, 11);
        features.Controls.Add(lblFovValue, 3, 11);

        features.Controls.Add(hotkey12, 0, 12);
        features.Controls.Add(btnFreeCrafting, 1, 12);

        features.Controls.Add(hotkey13, 0, 13);
        features.Controls.Add(btnClearSkies, 1, 13);

        features.Controls.Add(hotkey14, 0, 14);
        features.Controls.Add(btnDaytime, 1, 14);

        features.Controls.Add(hotkey15, 0, 15);
        features.Controls.Add(btnCalmOcean, 1, 15);

        features.Controls.Add(hotkey16, 0, 16);
        features.Controls.Add(btnNoLightning, 1, 16);

        features.Controls.Add(hotkeyHome, 0, 17);
        features.Controls.Add(btnReset, 1, 17);

        features.Dock = DockStyle.Fill;
        features.Location = new Point(280, 0);
        features.Margin = new Padding(0);
        features.Name = "features";
        features.Padding = new Padding(12, 5, 11, 5);
        features.Size = new Size(718, 691);
        features.TabIndex = 1;

        //
        // hotkeyHeader
        //
        hotkeyHeader.Dock = DockStyle.Fill;
        hotkeyHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        hotkeyHeader.ForeColor = Color.FromArgb(170, 170, 170);
        hotkeyHeader.Margin = new Padding(0);
        hotkeyHeader.Name = "hotkeyHeader";
        hotkeyHeader.Text = "HOTKEYS";
        hotkeyHeader.TextAlign = ContentAlignment.MiddleLeft;
        hotkeyHeader.TabIndex = 0;

        //
        // cheatHeader
        //
        cheatHeader.Dock = DockStyle.Fill;
        cheatHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        cheatHeader.ForeColor = Color.FromArgb(170, 170, 170);
        cheatHeader.Margin = new Padding(0);
        cheatHeader.Name = "cheatHeader";
        cheatHeader.Text = "CHEATS";
        cheatHeader.TextAlign = ContentAlignment.MiddleLeft;
        cheatHeader.TabIndex = 1;

        //
        // valueHeader
        //
        valueHeader.Dock = DockStyle.Fill;
        valueHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        valueHeader.ForeColor = Color.FromArgb(170, 170, 170);
        valueHeader.Margin = new Padding(0);
        valueHeader.Name = "valueHeader";
        valueHeader.Text = "VALUE";
        valueHeader.TextAlign = ContentAlignment.MiddleCenter;
        valueHeader.TabIndex = 2;

        //
        // hotkey1
        //
        hotkey1.Dock = DockStyle.Fill;
        hotkey1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey1.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey1.Margin = new Padding(0);
        hotkey1.Name = "hotkey1";
        hotkey1.Text = "NUM 1";
        hotkey1.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnRefillHealth
        //
        btnRefillHealth.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnRefillHealth, 3);
        btnRefillHealth.Cursor = Cursors.Hand;
        btnRefillHealth.Dock = DockStyle.Fill;
        btnRefillHealth.FlatAppearance.BorderSize = 0;
        btnRefillHealth.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnRefillHealth.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnRefillHealth.FlatStyle = FlatStyle.Flat;
        btnRefillHealth.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnRefillHealth.ForeColor = Color.FromArgb(242, 242, 242);
        btnRefillHealth.Margin = new Padding(0);
        btnRefillHealth.Name = "btnRefillHealth";
        btnRefillHealth.TabIndex = 0;
        btnRefillHealth.Text = "Refill Health";
        btnRefillHealth.TextAlign = ContentAlignment.MiddleLeft;
        btnRefillHealth.UseVisualStyleBackColor = false;
        btnRefillHealth.Click += btnRefillHealth_Click;

        //
        // hotkey2
        //
        hotkey2.Dock = DockStyle.Fill;
        hotkey2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey2.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey2.Margin = new Padding(0);
        hotkey2.Name = "hotkey2";
        hotkey2.Text = "NUM 2";
        hotkey2.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnRefillBattery
        //
        btnRefillBattery.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnRefillBattery, 3);
        btnRefillBattery.Cursor = Cursors.Hand;
        btnRefillBattery.Dock = DockStyle.Fill;
        btnRefillBattery.FlatAppearance.BorderSize = 0;
        btnRefillBattery.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnRefillBattery.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnRefillBattery.FlatStyle = FlatStyle.Flat;
        btnRefillBattery.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnRefillBattery.ForeColor = Color.FromArgb(242, 242, 242);
        btnRefillBattery.Margin = new Padding(0);
        btnRefillBattery.Name = "btnRefillBattery";
        btnRefillBattery.TabIndex = 1;
        btnRefillBattery.Text = "Refill Battery";
        btnRefillBattery.TextAlign = ContentAlignment.MiddleLeft;
        btnRefillBattery.UseVisualStyleBackColor = false;
        btnRefillBattery.Click += btnRefillBattery_Click;

        //
        // hotkey3
        //
        hotkey3.Dock = DockStyle.Fill;
        hotkey3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey3.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey3.Margin = new Padding(0);
        hotkey3.Name = "hotkey3";
        hotkey3.Text = "NUM 3";
        hotkey3.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnUnlimitedHealth
        //
        btnUnlimitedHealth.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnUnlimitedHealth, 3);
        btnUnlimitedHealth.Cursor = Cursors.Hand;
        btnUnlimitedHealth.Dock = DockStyle.Fill;
        btnUnlimitedHealth.FlatAppearance.BorderSize = 0;
        btnUnlimitedHealth.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnUnlimitedHealth.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnUnlimitedHealth.FlatStyle = FlatStyle.Flat;
        btnUnlimitedHealth.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnUnlimitedHealth.ForeColor = Color.FromArgb(242, 242, 242);
        btnUnlimitedHealth.Margin = new Padding(0);
        btnUnlimitedHealth.Name = "btnUnlimitedHealth";
        btnUnlimitedHealth.TabIndex = 2;
        btnUnlimitedHealth.Text = "Unlimited Health   [OFF]";
        btnUnlimitedHealth.TextAlign = ContentAlignment.MiddleLeft;
        btnUnlimitedHealth.UseVisualStyleBackColor = false;
        btnUnlimitedHealth.Click += btnUnlimitedHealth_Click;

        //
        // hotkey4
        //
        hotkey4.Dock = DockStyle.Fill;
        hotkey4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey4.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey4.Margin = new Padding(0);
        hotkey4.Name = "hotkey4";
        hotkey4.Text = "NUM 4";
        hotkey4.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnUnlimitedBattery
        //
        btnUnlimitedBattery.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnUnlimitedBattery, 3);
        btnUnlimitedBattery.Cursor = Cursors.Hand;
        btnUnlimitedBattery.Dock = DockStyle.Fill;
        btnUnlimitedBattery.FlatAppearance.BorderSize = 0;
        btnUnlimitedBattery.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnUnlimitedBattery.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnUnlimitedBattery.FlatStyle = FlatStyle.Flat;
        btnUnlimitedBattery.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnUnlimitedBattery.ForeColor = Color.FromArgb(242, 242, 242);
        btnUnlimitedBattery.Margin = new Padding(0);
        btnUnlimitedBattery.Name = "btnUnlimitedBattery";
        btnUnlimitedBattery.TabIndex = 3;
        btnUnlimitedBattery.Text = "Unlimited Battery   [OFF]";
        btnUnlimitedBattery.TextAlign = ContentAlignment.MiddleLeft;
        btnUnlimitedBattery.UseVisualStyleBackColor = false;
        btnUnlimitedBattery.Click += btnUnlimitedBattery_Click;

        //
        // hotkey5
        //
        hotkey5.Dock = DockStyle.Fill;
        hotkey5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey5.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey5.Margin = new Padding(0);
        hotkey5.Name = "hotkey5";
        hotkey5.Text = "NUM 5";
        hotkey5.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnCarryCapacity
        //
        btnCarryCapacity.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnCarryCapacity, 3);
        btnCarryCapacity.Cursor = Cursors.Hand;
        btnCarryCapacity.Dock = DockStyle.Fill;
        btnCarryCapacity.FlatAppearance.BorderSize = 0;
        btnCarryCapacity.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnCarryCapacity.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnCarryCapacity.FlatStyle = FlatStyle.Flat;
        btnCarryCapacity.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCarryCapacity.ForeColor = Color.FromArgb(242, 242, 242);
        btnCarryCapacity.Margin = new Padding(0);
        btnCarryCapacity.Name = "btnCarryCapacity";
        btnCarryCapacity.TabIndex = 4;
        btnCarryCapacity.Text = "Carry Capacity 999 tonnes   [OFF]";
        btnCarryCapacity.TextAlign = ContentAlignment.MiddleLeft;
        btnCarryCapacity.UseVisualStyleBackColor = false;
        btnCarryCapacity.Click += btnCarryCapacity_Click;

        //
        // hotkey6
        //
        hotkey6.Dock = DockStyle.Fill;
        hotkey6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey6.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey6.Margin = new Padding(0);
        hotkey6.Name = "hotkey6";
        hotkey6.Text = "NUM 6";
        hotkey6.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnWalkingSpeed
        //
        btnWalkingSpeed.BackColor = Color.FromArgb(17, 17, 17);
        btnWalkingSpeed.Cursor = Cursors.Hand;
        btnWalkingSpeed.Dock = DockStyle.Fill;
        btnWalkingSpeed.FlatAppearance.BorderSize = 0;
        btnWalkingSpeed.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnWalkingSpeed.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnWalkingSpeed.FlatStyle = FlatStyle.Flat;
        btnWalkingSpeed.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnWalkingSpeed.ForeColor = Color.FromArgb(242, 242, 242);
        btnWalkingSpeed.Margin = new Padding(0);
        btnWalkingSpeed.Name = "btnWalkingSpeed";
        btnWalkingSpeed.TabIndex = 5;
        btnWalkingSpeed.Text = "Walking Speed   [OFF]";
        btnWalkingSpeed.TextAlign = ContentAlignment.MiddleLeft;
        btnWalkingSpeed.UseVisualStyleBackColor = false;
        btnWalkingSpeed.Click += btnWalkingSpeed_Click;

        //
        // trkWalk
        //
        trkWalk.AutoSize = false;
        trkWalk.BackColor = Color.FromArgb(17, 17, 17);
        trkWalk.Dock = DockStyle.Fill;
        trkWalk.Margin = new Padding(0, 2, 0, 2);
        trkWalk.Maximum = 50;
        trkWalk.Minimum = 5;
        trkWalk.Name = "trkWalk";
        trkWalk.TabIndex = 25;
        trkWalk.TickStyle = TickStyle.None;
        trkWalk.Value = 20;

        //
        // lblWalkValue
        //
        lblWalkValue.Dock = DockStyle.Fill;
        lblWalkValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblWalkValue.ForeColor = Color.FromArgb(235, 182, 43);
        lblWalkValue.Margin = new Padding(0);
        lblWalkValue.Name = "lblWalkValue";
        lblWalkValue.Text = "2.0x";
        lblWalkValue.TextAlign = ContentAlignment.MiddleCenter;

        //
        // hotkey7
        //
        hotkey7.Dock = DockStyle.Fill;
        hotkey7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey7.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey7.Margin = new Padding(0);
        hotkey7.Name = "hotkey7";
        hotkey7.Text = "NUM 7";
        hotkey7.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnSwimmingSpeed
        //
        btnSwimmingSpeed.BackColor = Color.FromArgb(17, 17, 17);
        btnSwimmingSpeed.Cursor = Cursors.Hand;
        btnSwimmingSpeed.Dock = DockStyle.Fill;
        btnSwimmingSpeed.FlatAppearance.BorderSize = 0;
        btnSwimmingSpeed.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnSwimmingSpeed.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnSwimmingSpeed.FlatStyle = FlatStyle.Flat;
        btnSwimmingSpeed.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSwimmingSpeed.ForeColor = Color.FromArgb(242, 242, 242);
        btnSwimmingSpeed.Margin = new Padding(0);
        btnSwimmingSpeed.Name = "btnSwimmingSpeed";
        btnSwimmingSpeed.TabIndex = 6;
        btnSwimmingSpeed.Text = "Swimming Speed   [OFF]";
        btnSwimmingSpeed.TextAlign = ContentAlignment.MiddleLeft;
        btnSwimmingSpeed.UseVisualStyleBackColor = false;
        btnSwimmingSpeed.Click += btnSwimmingSpeed_Click;

        //
        // trkSwim
        //
        trkSwim.AutoSize = false;
        trkSwim.BackColor = Color.FromArgb(17, 17, 17);
        trkSwim.Dock = DockStyle.Fill;
        trkSwim.Margin = new Padding(0, 2, 0, 2);
        trkSwim.Maximum = 50;
        trkSwim.Minimum = 5;
        trkSwim.Name = "trkSwim";
        trkSwim.TabIndex = 26;
        trkSwim.TickStyle = TickStyle.None;
        trkSwim.Value = 20;

        //
        // lblSwimValue
        //
        lblSwimValue.Dock = DockStyle.Fill;
        lblSwimValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSwimValue.ForeColor = Color.FromArgb(235, 182, 43);
        lblSwimValue.Margin = new Padding(0);
        lblSwimValue.Name = "lblSwimValue";
        lblSwimValue.Text = "2.0x";
        lblSwimValue.TextAlign = ContentAlignment.MiddleCenter;

        //
        // hotkey8
        //
        hotkey8.Dock = DockStyle.Fill;
        hotkey8.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey8.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey8.Margin = new Padding(0);
        hotkey8.Name = "hotkey8";
        hotkey8.Text = "NUM 8";
        hotkey8.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnJumpVelocity
        //
        btnJumpVelocity.BackColor = Color.FromArgb(17, 17, 17);
        btnJumpVelocity.Cursor = Cursors.Hand;
        btnJumpVelocity.Dock = DockStyle.Fill;
        btnJumpVelocity.FlatAppearance.BorderSize = 0;
        btnJumpVelocity.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnJumpVelocity.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnJumpVelocity.FlatStyle = FlatStyle.Flat;
        btnJumpVelocity.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnJumpVelocity.ForeColor = Color.FromArgb(242, 242, 242);
        btnJumpVelocity.Margin = new Padding(0);
        btnJumpVelocity.Name = "btnJumpVelocity";
        btnJumpVelocity.TabIndex = 7;
        btnJumpVelocity.Text = "Jump Multiplier   [OFF]";
        btnJumpVelocity.TextAlign = ContentAlignment.MiddleLeft;
        btnJumpVelocity.UseVisualStyleBackColor = false;
        btnJumpVelocity.Click += btnJumpVelocity_Click;

        //
        // trkJump
        //
        trkJump.AutoSize = false;
        trkJump.BackColor = Color.FromArgb(17, 17, 17);
        trkJump.Dock = DockStyle.Fill;
        trkJump.Margin = new Padding(0, 2, 0, 2);
        trkJump.Maximum = 50;
        trkJump.Minimum = 5;
        trkJump.Name = "trkJump";
        trkJump.TabIndex = 27;
        trkJump.TickStyle = TickStyle.None;
        trkJump.Value = 20;

        //
        // lblJumpValue
        //
        lblJumpValue.Dock = DockStyle.Fill;
        lblJumpValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblJumpValue.ForeColor = Color.FromArgb(235, 182, 43);
        lblJumpValue.Margin = new Padding(0);
        lblJumpValue.Name = "lblJumpValue";
        lblJumpValue.Text = "2.0x";
        lblJumpValue.TextAlign = ContentAlignment.MiddleCenter;

        //
        // hotkey9
        //
        hotkey9.Dock = DockStyle.Fill;
        hotkey9.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey9.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey9.Margin = new Padding(0);
        hotkey9.Name = "hotkey9";
        hotkey9.Text = "NUM 9";
        hotkey9.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnLowGravity
        //
        btnLowGravity.BackColor = Color.FromArgb(17, 17, 17);
        btnLowGravity.Cursor = Cursors.Hand;
        btnLowGravity.Dock = DockStyle.Fill;
        btnLowGravity.FlatAppearance.BorderSize = 0;
        btnLowGravity.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnLowGravity.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnLowGravity.FlatStyle = FlatStyle.Flat;
        btnLowGravity.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnLowGravity.ForeColor = Color.FromArgb(242, 242, 242);
        btnLowGravity.Margin = new Padding(0);
        btnLowGravity.Name = "btnLowGravity";
        btnLowGravity.TabIndex = 8;
        btnLowGravity.Text = "Gravity Multiplier   [OFF]";
        btnLowGravity.TextAlign = ContentAlignment.MiddleLeft;
        btnLowGravity.UseVisualStyleBackColor = false;
        btnLowGravity.Click += btnLowGravity_Click;

        //
        // trkGravity
        //
        trkGravity.AutoSize = false;
        trkGravity.BackColor = Color.FromArgb(17, 17, 17);
        trkGravity.Dock = DockStyle.Fill;
        trkGravity.Margin = new Padding(0, 2, 0, 2);
        trkGravity.Maximum = 20;
        trkGravity.Minimum = 1;
        trkGravity.Name = "trkGravity";
        trkGravity.TabIndex = 28;
        trkGravity.TickStyle = TickStyle.None;
        trkGravity.Value = 5;

        //
        // lblGravityValue
        //
        lblGravityValue.Dock = DockStyle.Fill;
        lblGravityValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblGravityValue.ForeColor = Color.FromArgb(235, 182, 43);
        lblGravityValue.Margin = new Padding(0);
        lblGravityValue.Name = "lblGravityValue";
        lblGravityValue.Text = "0.5x";
        lblGravityValue.TextAlign = ContentAlignment.MiddleCenter;

        //
        // hotkey10
        //
        hotkey10.Dock = DockStyle.Fill;
        hotkey10.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey10.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey10.Margin = new Padding(0);
        hotkey10.Name = "hotkey10";
        hotkey10.Text = "NUM 0";
        hotkey10.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnTimeSpeed
        //
        btnTimeSpeed.BackColor = Color.FromArgb(17, 17, 17);
        btnTimeSpeed.Cursor = Cursors.Hand;
        btnTimeSpeed.Dock = DockStyle.Fill;
        btnTimeSpeed.FlatAppearance.BorderSize = 0;
        btnTimeSpeed.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnTimeSpeed.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnTimeSpeed.FlatStyle = FlatStyle.Flat;
        btnTimeSpeed.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnTimeSpeed.ForeColor = Color.FromArgb(242, 242, 242);
        btnTimeSpeed.Margin = new Padding(0);
        btnTimeSpeed.Name = "btnTimeSpeed";
        btnTimeSpeed.TabIndex = 9;
        btnTimeSpeed.Text = "Game Speed   [OFF]";
        btnTimeSpeed.TextAlign = ContentAlignment.MiddleLeft;
        btnTimeSpeed.UseVisualStyleBackColor = false;
        btnTimeSpeed.Click += btnTimeSpeed_Click;

        //
        // trkGameSpeed
        //
        trkGameSpeed.AutoSize = false;
        trkGameSpeed.BackColor = Color.FromArgb(17, 17, 17);
        trkGameSpeed.Dock = DockStyle.Fill;
        trkGameSpeed.Margin = new Padding(0, 2, 0, 2);
        trkGameSpeed.Maximum = 30;
        trkGameSpeed.Minimum = 5;
        trkGameSpeed.Name = "trkGameSpeed";
        trkGameSpeed.TabIndex = 29;
        trkGameSpeed.TickStyle = TickStyle.None;
        trkGameSpeed.Value = 20;

        //
        // lblGameSpeedValue
        //
        lblGameSpeedValue.Dock = DockStyle.Fill;
        lblGameSpeedValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblGameSpeedValue.ForeColor = Color.FromArgb(235, 182, 43);
        lblGameSpeedValue.Margin = new Padding(0);
        lblGameSpeedValue.Name = "lblGameSpeedValue";
        lblGameSpeedValue.Text = "2.0x";
        lblGameSpeedValue.TextAlign = ContentAlignment.MiddleCenter;

        //
        // hotkey11
        //
        hotkey11.Dock = DockStyle.Fill;
        hotkey11.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey11.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey11.Margin = new Padding(0);
        hotkey11.Name = "hotkey11";
        hotkey11.Text = "NUM +";
        hotkey11.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnCameraFov
        //
        btnCameraFov.BackColor = Color.FromArgb(17, 17, 17);
        btnCameraFov.Cursor = Cursors.Hand;
        btnCameraFov.Dock = DockStyle.Fill;
        btnCameraFov.FlatAppearance.BorderSize = 0;
        btnCameraFov.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnCameraFov.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnCameraFov.FlatStyle = FlatStyle.Flat;
        btnCameraFov.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCameraFov.ForeColor = Color.FromArgb(242, 242, 242);
        btnCameraFov.Margin = new Padding(0);
        btnCameraFov.Name = "btnCameraFov";
        btnCameraFov.TabIndex = 10;
        btnCameraFov.Text = "Camera FOV   [OFF]";
        btnCameraFov.TextAlign = ContentAlignment.MiddleLeft;
        btnCameraFov.UseVisualStyleBackColor = false;
        btnCameraFov.Click += btnCameraFov_Click;

        //
        // trkFov
        //
        trkFov.AutoSize = false;
        trkFov.BackColor = Color.FromArgb(17, 17, 17);
        trkFov.Dock = DockStyle.Fill;
        trkFov.Margin = new Padding(0, 2, 0, 2);
        trkFov.Maximum = 130;
        trkFov.Minimum = 60;
        trkFov.Name = "trkFov";
        trkFov.TabIndex = 30;
        trkFov.TickStyle = TickStyle.None;
        trkFov.Value = 100;

        //
        // lblFovValue
        //
        lblFovValue.Dock = DockStyle.Fill;
        lblFovValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblFovValue.ForeColor = Color.FromArgb(235, 182, 43);
        lblFovValue.Margin = new Padding(0);
        lblFovValue.Name = "lblFovValue";
        lblFovValue.Text = "100°";
        lblFovValue.TextAlign = ContentAlignment.MiddleCenter;

        //
        // hotkey12
        //
        hotkey12.Dock = DockStyle.Fill;
        hotkey12.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey12.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey12.Margin = new Padding(0);
        hotkey12.Name = "hotkey12";
        hotkey12.Text = "NUM −";
        hotkey12.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnFreeCrafting
        //
        btnFreeCrafting.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnFreeCrafting, 3);
        btnFreeCrafting.Cursor = Cursors.Hand;
        btnFreeCrafting.Dock = DockStyle.Fill;
        btnFreeCrafting.FlatAppearance.BorderSize = 0;
        btnFreeCrafting.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnFreeCrafting.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnFreeCrafting.FlatStyle = FlatStyle.Flat;
        btnFreeCrafting.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnFreeCrafting.ForeColor = Color.FromArgb(242, 242, 242);
        btnFreeCrafting.Margin = new Padding(0);
        btnFreeCrafting.Name = "btnFreeCrafting";
        btnFreeCrafting.TabIndex = 11;
        btnFreeCrafting.Text = "Free Crafting   [OFF]";
        btnFreeCrafting.TextAlign = ContentAlignment.MiddleLeft;
        btnFreeCrafting.UseVisualStyleBackColor = false;
        btnFreeCrafting.Click += btnFreeCrafting_Click;

        //
        // hotkey13
        //
        hotkey13.Dock = DockStyle.Fill;
        hotkey13.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey13.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey13.Margin = new Padding(0);
        hotkey13.Name = "hotkey13";
        hotkey13.Text = "NUM *";
        hotkey13.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnClearSkies
        //
        btnClearSkies.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnClearSkies, 3);
        btnClearSkies.Cursor = Cursors.Hand;
        btnClearSkies.Dock = DockStyle.Fill;
        btnClearSkies.FlatAppearance.BorderSize = 0;
        btnClearSkies.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnClearSkies.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnClearSkies.FlatStyle = FlatStyle.Flat;
        btnClearSkies.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnClearSkies.ForeColor = Color.FromArgb(242, 242, 242);
        btnClearSkies.Margin = new Padding(0);
        btnClearSkies.Name = "btnClearSkies";
        btnClearSkies.TabIndex = 12;
        btnClearSkies.Text = "Clear Skies   [OFF]";
        btnClearSkies.TextAlign = ContentAlignment.MiddleLeft;
        btnClearSkies.UseVisualStyleBackColor = false;
        btnClearSkies.Click += btnClearSkies_Click;

        //
        // hotkey14
        //
        hotkey14.Dock = DockStyle.Fill;
        hotkey14.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey14.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey14.Margin = new Padding(0);
        hotkey14.Name = "hotkey14";
        hotkey14.Text = "NUM /";
        hotkey14.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnDaytime
        //
        btnDaytime.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnDaytime, 3);
        btnDaytime.Cursor = Cursors.Hand;
        btnDaytime.Dock = DockStyle.Fill;
        btnDaytime.FlatAppearance.BorderSize = 0;
        btnDaytime.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnDaytime.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnDaytime.FlatStyle = FlatStyle.Flat;
        btnDaytime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnDaytime.ForeColor = Color.FromArgb(242, 242, 242);
        btnDaytime.Margin = new Padding(0);
        btnDaytime.Name = "btnDaytime";
        btnDaytime.TabIndex = 13;
        btnDaytime.Text = "Always Daytime   [OFF]";
        btnDaytime.TextAlign = ContentAlignment.MiddleLeft;
        btnDaytime.UseVisualStyleBackColor = false;
        btnDaytime.Click += btnDaytime_Click;

        //
        // hotkey15
        //
        hotkey15.Dock = DockStyle.Fill;
        hotkey15.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey15.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey15.Margin = new Padding(0);
        hotkey15.Name = "hotkey15";
        hotkey15.Text = "CTRL + NUM 1";
        hotkey15.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnCalmOcean
        //
        btnCalmOcean.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnCalmOcean, 3);
        btnCalmOcean.Cursor = Cursors.Hand;
        btnCalmOcean.Dock = DockStyle.Fill;
        btnCalmOcean.FlatAppearance.BorderSize = 0;
        btnCalmOcean.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnCalmOcean.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnCalmOcean.FlatStyle = FlatStyle.Flat;
        btnCalmOcean.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCalmOcean.ForeColor = Color.FromArgb(242, 242, 242);
        btnCalmOcean.Margin = new Padding(0);
        btnCalmOcean.Name = "btnCalmOcean";
        btnCalmOcean.TabIndex = 14;
        btnCalmOcean.Text = "Calm Ocean   [OFF]";
        btnCalmOcean.TextAlign = ContentAlignment.MiddleLeft;
        btnCalmOcean.UseVisualStyleBackColor = false;
        btnCalmOcean.Click += btnCalmOcean_Click;

        //
        // hotkey16
        //
        hotkey16.Dock = DockStyle.Fill;
        hotkey16.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkey16.ForeColor = Color.FromArgb(235, 182, 43);
        hotkey16.Margin = new Padding(0);
        hotkey16.Name = "hotkey16";
        hotkey16.Text = "CTRL + NUM 2";
        hotkey16.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnNoLightning
        //
        btnNoLightning.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnNoLightning, 3);
        btnNoLightning.Cursor = Cursors.Hand;
        btnNoLightning.Dock = DockStyle.Fill;
        btnNoLightning.FlatAppearance.BorderSize = 0;
        btnNoLightning.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnNoLightning.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnNoLightning.FlatStyle = FlatStyle.Flat;
        btnNoLightning.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnNoLightning.ForeColor = Color.FromArgb(242, 242, 242);
        btnNoLightning.Margin = new Padding(0);
        btnNoLightning.Name = "btnNoLightning";
        btnNoLightning.TabIndex = 15;
        btnNoLightning.Text = "Disable Lightning   [OFF]";
        btnNoLightning.TextAlign = ContentAlignment.MiddleLeft;
        btnNoLightning.UseVisualStyleBackColor = false;
        btnNoLightning.Click += btnNoLightning_Click;

        //
        // hotkeyHome
        //
        hotkeyHome.Dock = DockStyle.Fill;
        hotkeyHome.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        hotkeyHome.ForeColor = Color.FromArgb(235, 182, 43);
        hotkeyHome.Margin = new Padding(0, 10, 0, 0);
        hotkeyHome.Name = "hotkeyHome";
        hotkeyHome.Text = "HOME";
        hotkeyHome.TextAlign = ContentAlignment.MiddleLeft;

        //
        // btnReset
        //
        btnReset.BackColor = Color.FromArgb(17, 17, 17);
        features.SetColumnSpan(btnReset, 3);
        btnReset.Cursor = Cursors.Hand;
        btnReset.Dock = DockStyle.Fill;
        btnReset.FlatAppearance.BorderSize = 0;
        btnReset.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnReset.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnReset.FlatStyle = FlatStyle.Flat;
        btnReset.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnReset.ForeColor = Color.Gainsboro;
        btnReset.Margin = new Padding(0, 10, 0, 0);
        btnReset.Name = "btnReset";
        btnReset.TabIndex = 16;
        btnReset.Text = "Disable All";
        btnReset.TextAlign = ContentAlignment.MiddleLeft;
        btnReset.UseVisualStyleBackColor = false;
        btnReset.Click += btnReset_Click;

        //
        // footer
        //
        footer.BackColor = Color.FromArgb(26, 26, 26);
        footer.ContextMenuStrip = advancedMenu;

        footer.Controls.Add(state);
        footer.Controls.Add(bar);
        footer.Controls.Add(footerCredit);

        footer.Dock = DockStyle.Fill;
        footer.Location = new Point(1, 761);
        footer.Margin = new Padding(0);
        footer.Name = "footer";
        footer.Size = new Size(998, 50);
        footer.TabIndex = 2;

        //
        // advancedMenu
        //
        advancedMenu.ImageScalingSize = new Size(20, 20);
        advancedMenu.Items.AddRange(new ToolStripItem[]
        {
            menuAttach,
            menuAddresses
        });

        advancedMenu.Name = "advancedMenu";
        advancedMenu.Size = new Size(184, 52);

        //
        // menuAttach
        //
        menuAttach.Name = "menuAttach";
        menuAttach.Size = new Size(183, 24);
        menuAttach.Text = "Attach / resolve";
        menuAttach.Click += btnResolve_Click;

        //
        // menuAddresses
        //
        menuAddresses.Name = "menuAddresses";
        menuAddresses.Size = new Size(183, 24);
        menuAddresses.Text = "Show addresses";
        menuAddresses.Click += btnAddresses_Click;

        //
        // state
        //
        state.AutoEllipsis = true;
        state.ContextMenuStrip = advancedMenu;
        state.Dock = DockStyle.Fill;
        state.Font = new Font("Segoe UI", 9F);
        state.ForeColor = Color.FromArgb(235, 182, 43);
        state.Location = new Point(0, 0);
        state.Margin = new Padding(4, 0, 4, 0);
        state.Name = "state";
        state.Padding = new Padding(11, 0, 0, 0);
        state.TabIndex = 0;
        state.Text = "Waiting for game...";
        state.TextAlign = ContentAlignment.MiddleLeft;

        //
        // bar
        //
        bar.BackColor = Color.FromArgb(26, 26, 26);
        bar.Controls.Add(chkAlwaysOnTop);
        bar.Controls.Add(chkSound);

        bar.Dock = DockStyle.Right;
        bar.Margin = new Padding(0);
        bar.Name = "bar";
        bar.Padding = new Padding(5, 6, 0, 0);
        bar.Size = new Size(108, 50);
        bar.TabIndex = 1;
        bar.WrapContents = false;

        //
        // chkAlwaysOnTop
        //
        chkAlwaysOnTop.AutoSize = true;
        chkAlwaysOnTop.ForeColor = Color.Gainsboro;
        chkAlwaysOnTop.Margin = new Padding(5, 8, 6, 0);
        chkAlwaysOnTop.Name = "chkAlwaysOnTop";
        chkAlwaysOnTop.TabIndex = 0;
        chkAlwaysOnTop.Text = "Topmost";
        chkAlwaysOnTop.Visible = false;
        chkAlwaysOnTop.CheckedChanged += chkAlwaysOnTop_CheckedChanged;

        //
        // chkSound
        //
        chkSound.AutoSize = true;
        chkSound.Checked = true;
        chkSound.CheckState = CheckState.Checked;
        chkSound.ForeColor = Color.Gainsboro;
        chkSound.Margin = new Padding(5, 8, 0, 0);
        chkSound.Name = "chkSound";
        chkSound.TabIndex = 1;
        chkSound.Text = "Sound";

        //
        // footerCredit
        //
        footerCredit.Dock = DockStyle.Right;
        footerCredit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        footerCredit.ForeColor = Color.FromArgb(170, 170, 170);
        footerCredit.Margin = new Padding(4, 0, 4, 0);
        footerCredit.Name = "footerCredit";
        footerCredit.Size = new Size(134, 50);
        footerCredit.TabIndex = 2;
        footerCredit.Text = "xDPx  |  v6";
        footerCredit.TextAlign = ContentAlignment.MiddleCenter;
        footerCredit.Visible = false;

        //
        // btnResolve
        //
        btnResolve.BackColor = Color.FromArgb(17, 17, 17);
        btnResolve.Cursor = Cursors.Hand;
        btnResolve.FlatAppearance.BorderSize = 0;
        btnResolve.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnResolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnResolve.FlatStyle = FlatStyle.Flat;
        btnResolve.ForeColor = Color.Gainsboro;
        btnResolve.Location = new Point(0, 0);
        btnResolve.Margin = new Padding(4, 1, 4, 1);
        btnResolve.Name = "btnResolve";
        btnResolve.Size = new Size(150, 36);
        btnResolve.TabIndex = 2;
        btnResolve.TabStop = false;
        btnResolve.Text = "Attach / resolve";
        btnResolve.UseVisualStyleBackColor = false;
        btnResolve.Visible = false;
        btnResolve.Click += btnResolve_Click;

        //
        // btnAddresses
        //
        btnAddresses.BackColor = Color.FromArgb(17, 17, 17);
        btnAddresses.Cursor = Cursors.Hand;
        btnAddresses.FlatAppearance.BorderSize = 0;
        btnAddresses.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 50, 24);
        btnAddresses.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 39, 39);
        btnAddresses.FlatStyle = FlatStyle.Flat;
        btnAddresses.ForeColor = Color.Gainsboro;
        btnAddresses.Location = new Point(0, 0);
        btnAddresses.Margin = new Padding(4, 1, 4, 1);
        btnAddresses.Name = "btnAddresses";
        btnAddresses.Size = new Size(150, 36);
        btnAddresses.TabIndex = 3;
        btnAddresses.TabStop = false;
        btnAddresses.Text = "Show addresses";
        btnAddresses.UseVisualStyleBackColor = false;
        btnAddresses.Visible = false;
        btnAddresses.Click += btnAddresses_Click;

        //
        // diagnosticsLabel
        //
        diagnosticsLabel.BackColor = Color.FromArgb(26, 26, 26);
        diagnosticsLabel.Dock = DockStyle.Top;
        diagnosticsLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        diagnosticsLabel.ForeColor = Color.FromArgb(170, 170, 170);
        diagnosticsLabel.Location = new Point(0, 0);
        diagnosticsLabel.Margin = new Padding(0);
        diagnosticsLabel.Name = "diagnosticsLabel";
        diagnosticsLabel.Size = new Size(998, 28);
        diagnosticsLabel.TabIndex = 1;
        diagnosticsLabel.Text = "  DIAGNOSTICS  •  Offline single-player only";
        diagnosticsLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // log
        //
        log.BackColor = Color.FromArgb(17, 17, 17);
        log.BorderStyle = BorderStyle.None;
        log.Dock = DockStyle.Fill;
        log.Font = new Font("Consolas", 8F);
        log.ForeColor = Color.Gainsboro;
        log.Location = new Point(0, 28);
        log.Margin = new Padding(0, 0, 0, 1);
        log.Multiline = true;
        log.Name = "log";
        log.ReadOnly = true;
        log.ScrollBars = ScrollBars.Vertical;
        log.Size = new Size(998, 134);
        log.TabIndex = 0;

        //
        // diagnosticsPanel
        //
        diagnosticsPanel.Controls.Add(log);
        diagnosticsPanel.Controls.Add(diagnosticsLabel);
        diagnosticsPanel.Controls.Add(btnResolve);
        diagnosticsPanel.Controls.Add(btnAddresses);

        diagnosticsPanel.Location = new Point(0, 850);
        diagnosticsPanel.Margin = new Padding(4);
        diagnosticsPanel.Name = "diagnosticsPanel";
        diagnosticsPanel.Size = new Size(998, 162);
        diagnosticsPanel.TabIndex = 2;
        diagnosticsPanel.Visible = false;

        //
        // fallback
        //
        fallback.Interval = 15;
        fallback.Tick += fallback_Tick;

        //
        // timer1
        //
        timer1.Tick += timer1_Tick_1;

        //
        // TrainerForm
        //
        AutoScaleDimensions = new SizeF(120F, 120F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(17, 17, 17);
        ClientSize = new Size(1000, 812);

        Controls.Add(layout);
        Controls.Add(diagnosticsPanel);

        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.Gainsboro;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Margin = new Padding(4);
        MaximizeBox = false;

        Name = "TrainerForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "The Last Caretaker | Offline Trainer v6";

        FormClosing += OnClosing;
        Shown += TrainerForm_Shown;

        layout.ResumeLayout(false);
        header.ResumeLayout(false);
        body.ResumeLayout(false);
        leftPanel.ResumeLayout(false);

        ((ISupportInitialize)coverArt).EndInit();

        features.ResumeLayout(false);

        ((ISupportInitialize)trkWalk).EndInit();
        ((ISupportInitialize)trkSwim).EndInit();
        ((ISupportInitialize)trkJump).EndInit();
        ((ISupportInitialize)trkGravity).EndInit();
        ((ISupportInitialize)trkGameSpeed).EndInit();
        ((ISupportInitialize)trkFov).EndInit();

        footer.ResumeLayout(false);
        advancedMenu.ResumeLayout(false);

        bar.ResumeLayout(false);
        bar.PerformLayout();

        diagnosticsPanel.ResumeLayout(false);
        diagnosticsPanel.PerformLayout();

        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel layout = null!;
    private Panel header = null!;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private Label languageLabel = null!;

    private TableLayoutPanel body = null!;
    private Panel leftPanel = null!;
    private PictureBox coverArt = null!;
    private Label lblProcess = null!;
    private Label lblPid = null!;
    private Label lblProcessStatus = null!;
    private Label lblCredit = null!;

    private TableLayoutPanel features = null!;

    private Label hotkeyHeader = null!;
    private Label cheatHeader = null!;
    private Label valueHeader = null!;

    private Label hotkey1 = null!;
    private Label hotkey2 = null!;
    private Label hotkey3 = null!;
    private Label hotkey4 = null!;
    private Label hotkey5 = null!;
    private Label hotkey6 = null!;
    private Label hotkey7 = null!;
    private Label hotkey8 = null!;
    private Label hotkey9 = null!;
    private Label hotkey10 = null!;
    private Label hotkey11 = null!;
    private Label hotkey12 = null!;
    private Label hotkey13 = null!;
    private Label hotkey14 = null!;
    private Label hotkey15 = null!;
    private Label hotkey16 = null!;
    private Label hotkeyHome = null!;

    private TrainerFeatureButton btnRefillHealth = null!;
    private TrainerFeatureButton btnRefillBattery = null!;
    private TrainerFeatureButton btnUnlimitedHealth = null!;
    private TrainerFeatureButton btnUnlimitedBattery = null!;
    private TrainerFeatureButton btnCarryCapacity = null!;
    private TrainerFeatureButton btnWalkingSpeed = null!;
    private TrainerFeatureButton btnSwimmingSpeed = null!;
    private TrainerFeatureButton btnJumpVelocity = null!;
    private TrainerFeatureButton btnLowGravity = null!;
    private TrainerFeatureButton btnTimeSpeed = null!;
    private TrainerFeatureButton btnCameraFov = null!;
    private TrainerFeatureButton btnFreeCrafting = null!;

    private TrainerFeatureButton btnClearSkies = null!;
    private TrainerFeatureButton btnDaytime = null!;
    private TrainerFeatureButton btnCalmOcean = null!;
    private TrainerFeatureButton btnNoLightning = null!;

    private TrackBar trkWalk = null!;
    private Label lblWalkValue = null!;
    private TrackBar trkSwim = null!;
    private Label lblSwimValue = null!;
    private TrackBar trkJump = null!;
    private Label lblJumpValue = null!;
    private TrackBar trkGravity = null!;
    private Label lblGravityValue = null!;
    private TrackBar trkGameSpeed = null!;
    private Label lblGameSpeedValue = null!;
    private TrackBar trkFov = null!;
    private Label lblFovValue = null!;

    private FlowLayoutPanel bar = null!;
    private Button btnResolve = null!;
    private Button btnAddresses = null!;
    private TrainerFeatureButton btnReset = null!;
    private CheckBox chkAlwaysOnTop = null!;
    private CheckBox chkSound = null!;

    private Label diagnosticsLabel = null!;
    private TextBox log = null!;
    private Panel footer = null!;
    private Label state = null!;
    private Label footerCredit = null!;
    private Panel diagnosticsPanel = null!;

    private ContextMenuStrip advancedMenu = null!;
    private ToolStripMenuItem menuAttach = null!;
    private ToolStripMenuItem menuAddresses = null!;

    private System.Windows.Forms.Timer fallback = null!;
    private System.Windows.Forms.Timer timer1 = null!;
}

// Native button subclass.
// Keep text readable on dark backgrounds, including disabled states.
[ToolboxItem(true)]
public class TrainerFeatureButton : Button
{
    protected override void OnPaint(PaintEventArgs e)
    {
        Color background = BackColor;

        if (Enabled &&
            ClientRectangle.Contains(
                PointToClient(System.Windows.Forms.Cursor.Position)))
        {
            background = ControlPaint.Light(background, 0.12f);
        }

        e.Graphics.Clear(background);

        Color foreground = ForeColor.GetBrightness() < 0.35f
            ? Color.FromArgb(242, 242, 242)
            : ForeColor;

        if (!Enabled)
        {
            foreground = Color.FromArgb(210, 210, 210);
        }
        else if (Text.EndsWith(
            "[ON]",
            System.StringComparison.OrdinalIgnoreCase))
        {
            foreground = Color.FromArgb(132, 239, 164);
        }

        Rectangle bounds = new Rectangle(
            Padding.Left,
            Padding.Top,
            System.Math.Max(
                0,
                ClientSize.Width - Padding.Horizontal),
            System.Math.Max(
                0,
                ClientSize.Height - Padding.Vertical));

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            bounds,
            foreground,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoPadding |
            TextFormatFlags.EndEllipsis);

        if (Enabled && Focused && ShowFocusCues)
        {
            ControlPaint.DrawFocusRectangle(
                e.Graphics,
                ClientRectangle,
                foreground,
                background);
        }
    }

    protected override void OnMouseEnter(System.EventArgs e)
    {
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(System.EventArgs e)
    {
        base.OnMouseLeave(e);
        Invalidate();
    }
}