using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class TrainerForm : Form
{
    private readonly Button[] buttons;
    private readonly string[] buttonLabels;
    private readonly Color[] buttonOffColors;

    private const int FeatureCount = 21;
    private const int ResetHotkeyId = FeatureCount + 1;

    private readonly bool[] registered = new bool[ResetHotkeyId + 1];
    private readonly bool[] wasDown = new bool[ResetHotkeyId + 1];
    private readonly bool[] previousEnabled = new bool[FeatureCount + 1];

    private Button btnRefillBoatFuel = null!;
    private Button btnUnlimitedBoatFuel = null!;
    private Button btnUnlimitedBoatElectricity = null!;
    private Button btnMaximumBoatThrottle = null!;

    private Button btnFreezeHumanSeedDecay = null!;
    private static readonly Keys[] hotkeys =
  {
    Keys.None,

    Keys.NumPad1,
    Keys.NumPad2,
    Keys.NumPad3,
    Keys.NumPad4,
    Keys.NumPad5,
    Keys.NumPad6,
    Keys.NumPad7,
    Keys.NumPad8,
    Keys.NumPad9,
    Keys.NumPad0,

    Keys.Add,
    Keys.Subtract,
    Keys.Multiply,
    Keys.Divide,

    Keys.NumPad1,
    Keys.NumPad2,
    Keys.NumPad3,
    Keys.NumPad4,
    Keys.NumPad5,
    Keys.NumPad6,
    Keys.NumPad7,

    Keys.Home
};
    private volatile Game? game;
    private Task? worker;
    private volatile bool closing;
    private bool readyToClose;

    private bool playerReady;
    private bool activationPlayed;
    private bool resolvePending;

    public TrainerForm()
    {
        InitializeComponent();

        InitializeBoatFeatures();


        btnFreeCrafting.Text = "Free & Instant Crafting   [OFF]";
        subtitleLabel.Text = $"v7  •  Plus {FeatureCount} Trainer";
        footerCredit.Text = "xDPx  |  v7";
        Text = "The Last Caretaker | Offline Trainer v7";

        buttons = new Button[]
        {
            btnRefillHealth,
            btnRefillHealth,
            btnRefillBattery,
            btnUnlimitedHealth,
            btnUnlimitedBattery,
            btnCarryCapacity,
            btnWalkingSpeed,
            btnSwimmingSpeed,
            btnJumpVelocity,
            btnLowGravity,
            btnTimeSpeed,
            btnCameraFov,
            btnFreeCrafting,
            btnClearSkies,
            btnDaytime,
            btnCalmOcean,
            btnNoLightning,
            btnRefillBoatFuel,
            btnUnlimitedBoatFuel,
            btnUnlimitedBoatElectricity,
            btnMaximumBoatThrottle,
            btnFreezeHumanSeedDecay
        };




        buttonLabels = Array.ConvertAll(
            buttons,
            button => button.Text.EndsWith(
                "   [OFF]", StringComparison.Ordinal)
                ? button.Text[..^8]
                : button.Text);

        buttonOffColors = Array.ConvertAll(
            buttons, button => button.BackColor);


        for (int i = 1; i < buttons.Length; i++)
        {
            buttons[i].Enabled = false;
            buttons[i].Text = LabelFor(i, false);
        }

        trkWalk.ValueChanged += (_, _) => UpdateSliderValues();
        trkSwim.ValueChanged += (_, _) => UpdateSliderValues();
        trkJump.ValueChanged += (_, _) => UpdateSliderValues();
        trkGravity.ValueChanged += (_, _) => UpdateSliderValues();
        trkGameSpeed.ValueChanged += (_, _) => UpdateSliderValues();
        trkFov.ValueChanged += (_, _) => UpdateSliderValues();

        UpdateSliderValues();
    }

    private void InitializeBoatFeatures()
    {
        body.SuspendLayout();
        features.SuspendLayout();

        try
        {
            float rowHeight = features.RowStyles[16].Height;
            float resetHeight = features.RowStyles[17].Height;

            features.RowCount = 23;

            while (features.RowStyles.Count > 17)
            {
                features.RowStyles.RemoveAt(
                    features.RowStyles.Count - 1);
            }

            for (int i = 17; i <= FeatureCount; i++)
            {
                features.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, rowHeight));
            }

            features.RowStyles.Add(
                new RowStyle(SizeType.Absolute, resetHeight));

            features.SetRow(hotkeyHome, ResetHotkeyId);
            features.SetRow(btnReset, ResetHotkeyId);

            btnRefillBoatFuel = AddBoatFeature(
                17,
                nameof(btnRefillBoatFuel),
                "Refill Boat Fuel",
                "CTRL + NUM 3",
                17);

            btnUnlimitedBoatFuel = AddBoatFeature(
                18,
                nameof(btnUnlimitedBoatFuel),
                "Unlimited Boat Fuel",
                "CTRL + NUM 4",
                18);

            btnUnlimitedBoatElectricity = AddBoatFeature(
                19,
                nameof(btnUnlimitedBoatElectricity),
                "Unlimited Boat Electricity",
                "CTRL + NUM 5",
                19);

            btnMaximumBoatThrottle = AddBoatFeature(
                20,
                nameof(btnMaximumBoatThrottle),
                "Maximum Boat Throttle",
                "CTRL + NUM 6",
                20);

            btnFreezeHumanSeedDecay = AddBoatFeature(
                21,
                nameof(btnFreezeHumanSeedDecay),
                "Freeze Human Seed Decay",
                "CTRL + NUM 7",
                21);

            // Use a dedicated scrolling container so all
            // feature rows retain their original height.

            var scrollPanel = new Panel
            {
                Name = "featureScrollPanel",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = features.BackColor,
                Margin = new Padding(0)
            };

            body.Controls.Remove(features);

            features.AutoScroll = false;
            features.AutoSize = true;
            features.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            features.Dock = DockStyle.Top;

            scrollPanel.Controls.Add(features);
            body.Controls.Add(scrollPanel, 1, 0);

            ClientSize = new Size(
                ClientSize.Width,
                Math.Min(
                    980,
                    Screen.FromControl(this).WorkingArea.Height - 60));
        }
        finally
        {
            features.ResumeLayout(true);
            body.ResumeLayout(true);
        }
    }
    private Button AddBoatFeature(
        int row,
        string name,
        string caption,
        string hotkey,
        int id)
    {
        var label = new Label
        {
            Name = $"hotkey{id}",
            Text = hotkey.ToUpperInvariant(),

            AutoSize = false,

            Dock = hotkey16.Dock,
            TextAlign = hotkey16.TextAlign,

            ForeColor = hotkey16.ForeColor,
            BackColor = hotkey16.BackColor,

            Font = hotkey16.Font,
            Margin = hotkey16.Margin,
            Padding = hotkey16.Padding
        };

        bool isToggle = !IsActionFeature(id);

        var button = new TrainerFeatureButton
        {
            Name = name,

            Text = caption +
                (isToggle ? "   [OFF]" : ""),

            AutoSize = false,

            Dock = btnNoLightning.Dock,

            BackColor = btnNoLightning.BackColor,
            ForeColor = btnNoLightning.ForeColor,

            FlatStyle = btnNoLightning.FlatStyle,

            Font = btnNoLightning.Font,
            Margin = btnNoLightning.Margin,
            Padding = btnNoLightning.Padding,

            TextAlign = btnNoLightning.TextAlign,
            Cursor = btnNoLightning.Cursor,

            TabIndex = id - 1,
            TabStop = btnNoLightning.TabStop,

            UseVisualStyleBackColor = false,
            Enabled = false
        };

        button.FlatAppearance.BorderColor =
            btnNoLightning.FlatAppearance.BorderColor;

        button.FlatAppearance.BorderSize =
            btnNoLightning.FlatAppearance.BorderSize;

        button.FlatAppearance.MouseDownBackColor =
            btnNoLightning.FlatAppearance.MouseDownBackColor;

        button.FlatAppearance.MouseOverBackColor =
            btnNoLightning.FlatAppearance.MouseOverBackColor;

        button.Click += (_, _) => Send(id);

        features.Controls.Add(label, 0, row);
        features.Controls.Add(button, 1, row);

        features.SetColumnSpan(button, 3);

        return button;
    }

    private void TrainerForm_Shown(
        object? sender,
        EventArgs e)
    {
        Console.SetOut(new LogWriter(Append));

        chkAlwaysOnTop.Visible = false;

        BindHotkeys();

        timer1.Start();
        timer1_Tick_1(this, EventArgs.Empty);
    }

    private void btnRefillHealth_Click(object? sender, EventArgs e)
        => Send(1);

    private void btnRefillBattery_Click(object? sender, EventArgs e)
        => Send(2);

    private void btnUnlimitedHealth_Click(object? sender, EventArgs e)
        => Send(3);

    private void btnUnlimitedBattery_Click(object? sender, EventArgs e)
        => Send(4);

    private void btnCarryCapacity_Click(object? sender, EventArgs e)
        => Send(5);

    private void btnWalkingSpeed_Click(object? sender, EventArgs e)
        => Send(6);

    private void btnSwimmingSpeed_Click(object? sender, EventArgs e)
        => Send(7);

    private void btnJumpVelocity_Click(object? sender, EventArgs e)
        => Send(8);

    private void btnLowGravity_Click(object? sender, EventArgs e)
        => Send(9);

    private void btnTimeSpeed_Click(object? sender, EventArgs e)
        => Send(10);

    private void btnCameraFov_Click(object? sender, EventArgs e)
        => Send(11);

    private void btnFreeCrafting_Click(object? sender, EventArgs e)
        => Send(12);

    private void btnClearSkies_Click(object? sender, EventArgs e)
        => Send(13);

    private void btnDaytime_Click(object? sender, EventArgs e)
        => Send(14);

    private void btnCalmOcean_Click(object? sender, EventArgs e)
        => Send(15);

    private void btnNoLightning_Click(object? sender, EventArgs e)
        => Send(16);

    private void btnResolve_Click(object? sender, EventArgs e)
    {
        if (worker == null || worker.IsCompleted)
            Attach();
        else
            Send(-1);
    }

    private void btnAddresses_Click(object? sender, EventArgs e)
        => Send(-2);

    private void btnReset_Click(object? sender, EventArgs e)
        => Send(-3);

    private void chkAlwaysOnTop_CheckedChanged(
        object? sender, EventArgs e)
    {
        TopMost = chkAlwaysOnTop.Checked;
    }

    private void fallback_Tick(object? sender, EventArgs e)
        => PollUnregistered();

    private string LabelFor(int id, bool on)
    {
        return buttonLabels[id] +
            (IsActionFeature(id)
                ? ""
                : on ? "   [ON]" : "   [OFF]");
    }

    private void OnUi(System.Action a)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            BeginInvoke(a);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void Append(string s)
    {
        OnUi(() =>
        {
            if (log.TextLength > 50000)
                log.Clear();

            log.AppendText(s);

            if (s.Contains("Health refilled.") ||
                s.Contains("Battery refill requested successfully.") ||
                s.Contains("Boat fuel refilled."))
            {
                PlaySoundFromResource(
                    VoyageTrainer.Properties.Resources
                        ._02_Option_Enabled);
            }
        });
    }

    private void Display(string info, bool[] enabled)
    {
        bool[] snapshot = (bool[])enabled.Clone();

        OnUi(() =>
        {
            if (closing)
                return;

            resolvePending = false;

            playerReady = info.StartsWith("Player 0x");

            state.Text = playerReady
                ? "Trainer Activated"
                : info;

            state.ForeColor = playerReady
                ? Color.LimeGreen
                : Color.Orange;

            if (playerReady && !activationPlayed)
            {
                activationPlayed = true;

                PlaySoundFromResource(
                    VoyageTrainer.Properties.Resources
                        ._01_Trainer_Activated);
            }

            for (int i = 1; i < buttons.Length; i++)
            {
                bool isEnabled =
                    i < snapshot.Length && snapshot[i];

                if (playerReady &&
                    previousEnabled[i] != isEnabled)
                {
                    Debug.WriteLine(
                        $"Feature {i}: " +
                        $"{(isEnabled ? "ON" : "OFF")}");

                    PlaySoundFromResource(
                        isEnabled
                            ? VoyageTrainer.Properties.Resources
                                ._05_Extra_Sound
                            : VoyageTrainer.Properties.Resources
                                ._06_Extra_Sound);
                }

                previousEnabled[i] = isEnabled;

                // Enabled toggles can still be turned off
                // during a temporary player transition.
                buttons[i].Enabled =
                    playerReady || isEnabled;

                buttons[i].Text = LabelFor(i, isEnabled);

                buttons[i].ForeColor = isEnabled
                    ? Color.FromArgb(132, 239, 164)
                    : Color.FromArgb(242, 242, 242);

                buttons[i].BackColor =
                    buttonOffColors[i];
            }
        });
    }

    private void UpdateSliderValues()
    {
        lblWalkValue.Text =
            $"{trkWalk.Value / 10f:0.0}x";

        lblSwimValue.Text =
            $"{trkSwim.Value / 10f:0.0}x";

        lblJumpValue.Text =
            $"{trkJump.Value / 10f:0.0}x";

        lblGravityValue.Text =
            $"{trkGravity.Value / 10f:0.0}x";

        lblGameSpeedValue.Text =
            $"{trkGameSpeed.Value / 10f:0.0}x";

        lblFovValue.Text = $"{trkFov.Value}°";

        var g = game;

        if (g == null)
            return;

        g.SetMultiplier(6, trkWalk.Value / 10f);
        g.SetMultiplier(7, trkSwim.Value / 10f);
        g.SetMultiplier(8, trkJump.Value / 10f);
        g.SetMultiplier(9, trkGravity.Value / 10f);
        g.SetMultiplier(10, trkGameSpeed.Value / 10f);
        g.SetMultiplier(11, trkFov.Value);
    }

    private void Attach()
    {
        if (closing ||
            (worker != null && !worker.IsCompleted))
        {
            return;
        }

        state.Text = "Attaching / resolving player...";

        worker = Task.Run(() =>
        {
            try
            {
                using var g = new Game();

                game = g;

                OnUi(UpdateSliderValues);

                if (closing)
                    g.Stop();

                g.Run(Display);
            }
            catch (Exception ex)
            {
                Append(ex.Message + Environment.NewLine);
            }
            finally
            {
                game = null;

                Display(
                    "Game not found, trainer is waiting.",
                    new bool[FeatureCount + 1]);
            }
        });
    }

    private void Send(int id)
    {
        if (closing)
            return;

        Debug.WriteLine(
            $"Trainer command received: {id}");

        var g = game;

        if (g == null)
        {
            Append(
                "Trainer not attached." +
                Environment.NewLine);

            return;
        }

        g.Request(id);
    }

    private void BindHotkeys()
    {
        for (int i = 1; i < hotkeys.Length; i++)
        {
            wasDown[i] = KeyDown(i);

            uint modifiers = 0x4000;

            if (NeedsCtrl(i))
                modifiers |= 0x0002;

            registered[i] = RegisterHotKey(
                Handle,
                i,
                modifiers,
                (uint)hotkeys[i]);

            if (!registered[i])
            {
                Append(
                    $"{hotkeys[i]}: " +
                    "using global key-state polling." +
                    Environment.NewLine);
            }
        }

        fallback.Start();
    }


    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312)
        {
            int id = m.WParam.ToInt32();

            if (id >= 1 && id < hotkeys.Length)
            {
                ExecuteHotkey(id);
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void PollUnregistered()
    {
        PollOverlayHotkey();
        PollMinimapHotkeys();
        PollOverlayEditHotkey();

        for (int i = 1; i < hotkeys.Length; i++)
        {
            if (registered[i])
                continue;

            bool down = KeyDown(i);

            if (down && !wasDown[i])
                ExecuteHotkey(i);

            wasDown[i] = down;
        }
    }

    private async void OnClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        if (readyToClose)
            return;

        e.Cancel = true;

        if (closing)
            return;

        closing = true;

        CloseGameOverlay();

        fallback.Stop();

        for (int i = 1; i < hotkeys.Length; i++)
        {
            if (registered[i])
                UnregisterHotKey(Handle, i);
        }

        timer1.Stop();

        game?.Stop();

        state.Text = "Restoring toggles and closing...";

        if (worker != null)
            await worker;

        readyToClose = true;

        Close();
    }

    private int? GetGamePid()
    {
        var processes = Process.GetProcessesByName(
            "VoyageSteam-Win64-Shipping");

        try
        {
            return processes.Length == 1
                ? processes[0].Id
                : null;
        }
        finally
        {
            foreach (var process in processes)
                process.Dispose();
        }
    }

    private static bool NeedsCtrl(int id)
    {
        return id >= 15 && id <= FeatureCount;
    }

    private void ExecuteHotkey(int id)
    {
        Send(id == ResetHotkeyId ? -3 : id);
    }

    private static bool KeyDown(int id)
    {
        bool ctrl =
            (GetAsyncKeyState(
                (int)Keys.ControlKey) & 0x8000) != 0;

        return ctrl == NeedsCtrl(id) &&
               (GetAsyncKeyState(
                   (int)hotkeys[id]) & 0x8000) != 0;
    }

    private sealed class LogWriter : TextWriter
    {
        private readonly System.Action<string> write;

        public LogWriter(System.Action<string> write)
        {
            this.write = write;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string? value)
        {
            if (value != null)
                write(value);
        }

        public override void WriteLine(string? value)
        {
            write(value + Environment.NewLine);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(
        IntPtr hwnd, int id, uint modifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(
        IntPtr hwnd, int id);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);

    private void timer1_Tick_1(object sender, EventArgs e)
    {
        if (closing)
            return;

        int? pid = GetGamePid();

        lblPid.Text =
            $"Process ID: {pid?.ToString() ?? "—"}";

        if (!pid.HasValue)
        {
            playerReady = false;
            activationPlayed = false;
            resolvePending = false;

            lblProcessStatus.Text =
                "Status: Game not running";

            lblProcessStatus.ForeColor = Color.Red;

            state.Text =
                "Game not found, trainer is waiting.";

            state.ForeColor = Color.Orange;

            return;
        }

        lblProcessStatus.Text = playerReady
            ? "Status: Connected"
            : "Status: Waiting for player";

        lblProcessStatus.ForeColor = playerReady
            ? Color.LimeGreen
            : Color.Orange;

        if (worker == null || worker.IsCompleted)
        {
            resolvePending = true;
            Attach();
        }

        // The Game worker handles player recovery.
        // Do not queue continuous world scans from the UI.
    }

    private void PlaySoundFromResource(Stream resourceStream)
    {
        if (!chkSound.Checked)
        {
            resourceStream.Dispose();
            return;
        }

        try
        {
            var reader = new WaveFileReader(resourceStream);
            var output = new WaveOutEvent();

            output.Init(reader);

            output.PlaybackStopped += (_, _) =>
            {
                output.Dispose();
                reader.Dispose();
                resourceStream.Dispose();
            };

            output.Play();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            resourceStream.Dispose();
        }
    }
}