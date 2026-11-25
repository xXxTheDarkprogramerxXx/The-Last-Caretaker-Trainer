using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public partial class TrainerForm
{
    private GameOverlayForm? playerOverlay;
    private GameOverlayForm? worldOverlay;
    private GameOverlayForm? vesselOverlay;

    private System.Windows.Forms.Timer? overlayTimer;

    private IntPtr overlayGameWindow;
    private int overlayGamePid;

    private bool overlayKeyDown;
    private bool editKeyDown;
    private bool overlayWanted;
    private bool overlayEditing;

    private static readonly Color OverlayBackground =
        Color.FromArgb(16, 18, 22);

    private static readonly Color OverlaySurface =
        Color.FromArgb(24, 27, 33);

    private static readonly Color OverlayBorder =
        Color.FromArgb(41, 45, 53);

    private static readonly Color OverlayAccent =
        Color.FromArgb(38, 156, 220);

    private static readonly Color OverlayGold =
        Color.FromArgb(235, 182, 43);

    private static readonly Color OverlayMuted =
        Color.FromArgb(155, 161, 172);

    private static readonly Color OverlayGreen =
        Color.FromArgb(132, 239, 164);

    private static readonly string[] OverlayKeyLabels =
    {
        "",
        "NUM 1",
        "NUM 2",
        "NUM 3",
        "NUM 4",
        "NUM 5",
        "NUM 6",
        "NUM 7",
        "NUM 8",
        "NUM 9",
        "NUM 0",
        "NUM +",
        "NUM -",
        "NUM *",
        "NUM /",
        "CTRL 1",
        "CTRL 2",
        "CTRL 3",
        "CTRL 4",
        "CTRL 5",
        "CTRL 6",
        "CTRL 7"
    };

    private static bool OverlayKeyPressed(Keys key)
    {
        return (GetAsyncKeyState((int)Keys.Menu) & 0x8000) != 0 &&
               (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    private void PollOverlayHotkey()
    {
        bool down = OverlayKeyPressed(Keys.W);

        if (down && !overlayKeyDown)
            ToggleGameOverlay();

        overlayKeyDown = down;
    }

    private void PollOverlayEditHotkey()
    {
        bool down = OverlayKeyPressed(Keys.E);

        if (down && !editKeyDown &&
            TryBindOverlayGame())
        {
            if (overlayWanted || minimapWanted || atlasWanted)
                SetOverlayEditing(!overlayEditing);
        }

        editKeyDown = down;
    }

    private void ToggleGameOverlay()
    {
        if (closing || !TryBindOverlayGame())
            return;

        overlayWanted = !overlayWanted;

        if (overlayWanted)
            atlasWanted = false;

        SetOverlayEditing(overlayWanted || atlasWanted);
    }

    private bool TryBindOverlayGame()
    {
        if (closing)
            return false;

        IntPtr foreground = GetForegroundWindow();

        if (overlayGameWindow != IntPtr.Zero &&
            IsWindow(overlayGameWindow) &&
            (foreground == overlayGameWindow ||
             IsOverlayFocused()))
        {
            EnsureOverlayTimer();
            return true;
        }

        int? pid = GetGamePid();

        if (!pid.HasValue ||
            foreground == IntPtr.Zero)
            return false;

        GetWindowThreadProcessId(
            foreground, out uint foregroundPid);

        if (foregroundPid != (uint)pid.Value)
            return false;

        if (overlayGameWindow != IntPtr.Zero &&
            overlayGameWindow != foreground)
        {
            CloseGameOverlay();
        }

        overlayGameWindow = foreground;
        overlayGamePid = pid.Value;

        EnsureOverlayTimer();

        return true;
    }

    private void EnsureOverlayTimer()
    {
        if (overlayTimer == null)
        {
            overlayTimer = new System.Windows.Forms.Timer
            {
                Interval = 50
            };

            overlayTimer.Tick += (_, _) =>
                RefreshOverlayWindows();
        }

        if (!overlayTimer.Enabled)
            overlayTimer.Start();
    }

    private bool IsOverlayFocused()
    {
        IntPtr foreground = GetForegroundWindow();

        return IsFormFocused(playerOverlay, foreground) ||
               IsFormFocused(worldOverlay, foreground) ||
               IsFormFocused(vesselOverlay, foreground) ||
               IsFormFocused(minimap, foreground) ||
               IsFormFocused(atlas, foreground);
    }

    private static bool IsFormFocused(
        Form? form,
        IntPtr foreground)
    {
        return form?.IsHandleCreated == true &&
               foreground == form.Handle;
    }

    private void SetOverlayEditing(bool enabled)
    {
        bool overlayHadFocus = IsOverlayFocused();

        overlayEditing = enabled;

        playerOverlay?.SetInteractive(enabled);
        worldOverlay?.SetInteractive(enabled);
        vesselOverlay?.SetInteractive(enabled);

        minimap?.SetInteractive(enabled);
        atlas?.SetInteractive(enabled);

        if (!enabled &&
            overlayHadFocus &&
            overlayGameWindow != IntPtr.Zero &&
            IsWindow(overlayGameWindow))
        {
            SetForegroundWindow(overlayGameWindow);
        }

        RefreshOverlayWindows();

        if (enabled)
        {
            if (atlas?.Visible == true)
                atlas.Activate();
            else if (playerOverlay?.Visible == true)
                playerOverlay.Activate();
            else if (minimap?.Visible == true)
                minimap.Activate();
        }
    }

    private bool TryGetGameBounds(out Rectangle bounds)
    {
        bounds = Rectangle.Empty;

        if (!GetClientRect(
                overlayGameWindow,
                out OverlayRect rect))
            return false;

        var origin = new OverlayPoint();

        if (!ClientToScreen(
                overlayGameWindow,
                ref origin))
            return false;

        bounds = new Rectangle(
            origin.X,
            origin.Y,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top);

        return bounds.Width >= 360 &&
               bounds.Height >= 300;
    }

    private void RefreshOverlayWindows()
    {
        if (closing)
            return;

        if (!IsWindow(overlayGameWindow))
        {
            CloseGameOverlay();
            return;
        }

        GetWindowThreadProcessId(
            overlayGameWindow, out uint pid);

        if (pid != (uint)overlayGamePid)
        {
            CloseGameOverlay();
            return;
        }

        // Hide overlays when another application has focus.
        if ((GetForegroundWindow() != overlayGameWindow &&
             !IsOverlayFocused()) ||
            IsIconic(overlayGameWindow))
        {
            HideTrainerPanels();
            HideMapWindows();

            return;
        }

        if (!TryGetGameBounds(out Rectangle bounds))
        {
            HideTrainerPanels();
            HideMapWindows();

            return;
        }

        if (overlayWanted && !atlasWanted)
        {
            playerOverlay ??= new GameOverlayForm(
                 this,
                 "PLAYER",
                 "Survival & movement",
                 1, 2, 3, 4, 5, 6,
                 7, 8, 9, 10, 11, 12);

            worldOverlay ??= new GameOverlayForm(
                this,
                "WORLD",
                "Environment & survival",
                13, 14, 15, 16, 21);

            vesselOverlay ??= new GameOverlayForm(
                this,
                "VESSEL",
                "Boat systems",
                17, 18, 19, 20);

            ShowTrainerPanel(
                playerOverlay,
                bounds,
                new Point(
                    bounds.Width - 390,
                    20));

            ShowTrainerPanel(
                worldOverlay,
                bounds,
                new Point(
                    bounds.Width - 780,
                    20));

            ShowTrainerPanel(
                vesselOverlay,
                bounds,
                new Point(
                    bounds.Width - 780,
                    worldOverlay.Height + 32));
        }
        else
        {
            HideTrainerPanels();
        }

        RefreshMapWindows(bounds);

        if (!overlayWanted &&
            !minimapWanted &&
            !atlasWanted)
        {
            overlayTimer?.Stop();
        }
    }

    private void ShowTrainerPanel(
        GameOverlayForm panel,
        Rectangle gameBounds,
        Point defaultOffset)
    {
        panel.SetInteractive(overlayEditing);

        panel.PlaceWithinGame(gameBounds, defaultOffset);

        if (!panel.Visible)
            panel.Show();

        panel.Invalidate();
    }

    private void HideTrainerPanels()
    {
        playerOverlay?.Hide();
        worldOverlay?.Hide();
        vesselOverlay?.Hide();
    }

    private void CloseGameOverlay()
    {
        overlayTimer?.Stop();
        overlayTimer?.Dispose();
        overlayTimer = null;

        playerOverlay?.Dispose();
        worldOverlay?.Dispose();
        vesselOverlay?.Dispose();

        playerOverlay = null;
        worldOverlay = null;
        vesselOverlay = null;

        CloseMinimap();

        overlayWanted = false;
        overlayEditing = false;

        overlayGameWindow = IntPtr.Zero;
        overlayGamePid = 0;
    }

    private string GetOverlayValue(int featureId)
    {
        return featureId switch
        {
            6 => lblWalkValue.Text,
            7 => lblSwimValue.Text,
            8 => lblJumpValue.Text,
            9 => lblGravityValue.Text,
            10 => lblGameSpeedValue.Text,
            11 => lblFovValue.Text,
            _ => ""
        };
    }

    private TrackBar? GetOverlaySlider(int featureId)
    {
        return featureId switch
        {
            6 => trkWalk,
            7 => trkSwim,
            8 => trkJump,
            9 => trkGravity,
            10 => trkGameSpeed,
            11 => trkFov,
            _ => null
        };
    }

    private static bool IsActionFeature(int id)
    {
        return id == 1 ||
               id == 2 ||
               id == 17;
    }

    private sealed class GameOverlayForm
        : FloatingOverlayForm
    {
        private readonly TrainerForm trainer;

        private readonly string title;
        private readonly string subtitle;

        private readonly int[] featureIds;

        private const int RowStart = 70;
        private const int RowHeight = 33;

        protected override int DragHeaderHeight => 64;



        public GameOverlayForm(
     TrainerForm trainer,
     string title,
     string subtitle,
     params int[] featureIds)
        {
            this.trainer = trainer;
            this.title = title;
            this.subtitle = subtitle;
            this.featureIds = featureIds;

            BackColor = OverlayBackground;
            ForeColor = Color.White;

            ClientSize = new Size(
                370,
                RowStart +
                (featureIds.Length * RowHeight) +
                39);

            Opacity = 0.96;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(OverlayBackground);

            g.SmoothingMode =
                SmoothingMode.AntiAlias;

            g.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint
                    .AntiAliasGridFit;

            using var titleFont = new Font(
                "Segoe UI", 19f,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            using var normalFont = new Font(
                "Segoe UI", 13f,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            using var smallFont = new Font(
                "Segoe UI", 10f,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            using var border = new Pen(OverlayBorder);

            g.DrawRectangle(
                border,
                0, 0,
                ClientSize.Width - 1,
                ClientSize.Height - 1);

            // Header.
            FillRounded(
                g,
                new RectangleF(
                    10, 10,
                    ClientSize.Width - 20, 49),
                9,
                OverlaySurface);

            DrawOverlayText(
                g,
                "⠿",
                normalFont,
                OverlayMuted,
                20, 15, 20, 18);

            DrawOverlayText(
                g,
                title,
                titleFont,
                Color.White,
                40, 12, 225, 25);

            DrawOverlayText(
                g,
                subtitle,
                smallFont,
                OverlayMuted,
                41, 36, 240, 18);

            DrawOverlayText(
                g,
                trainer.playerReady ? "LIVE" : "WAITING",
                smallFont,
                trainer.playerReady
                    ? OverlayGreen
                    : OverlayGold,
                280, 17, 70, 20,
                true);

            for (int index = 0; index < featureIds.Length; index++)
            {
                int id = featureIds[index];

                if (id >= trainer.buttons.Length)
                    continue;

                int rowY = RowStart + (index * RowHeight);

                DrawFeature(
                    g,
                    id,
                    rowY,
                    normalFont,
                    smallFont);
            }

            int footerY = ClientSize.Height - 32;

            g.DrawLine(
                border,
                12, footerY,
                ClientSize.Width - 12, footerY);

            DrawOverlayText(
                g,
                Interactive
                    ? "DRAG HEADER   |   ALT + E LOCK"
                    : "ALT + E EDIT   |   ALT + W HIDE",
                smallFont,
                OverlayMuted,
                15, footerY + 2,
                ClientSize.Width - 30,
                23);
        }

        private void DrawFeature(
            Graphics g,
            int id,
            int y,
            Font normalFont,
            Font smallFont)
        {
            bool active =
                id < trainer.previousEnabled.Length &&
                trainer.previousEnabled[id];

            bool action = IsActionFeature(id);

            bool available =
                trainer.buttons[id].Enabled;

            string name = trainer.buttonLabels[id];

            string value =
                trainer.GetOverlayValue(id);

            FillRounded(
                g,
                new RectangleF(
                    10, y + 2,
                    ClientSize.Width - 20,
                    RowHeight - 4),
                6,
                OverlaySurface);

            DrawOverlayText(
                g,
                OverlayKeyLabels[id],
                smallFont,
                OverlayMuted,
                16, y, 62, RowHeight);

            DrawOverlayText(
                g,
                name,
                normalFont,
                Color.White,
                82, y,
                string.IsNullOrEmpty(value)
                    ? 206
                    : 163,
                RowHeight);

            if (!string.IsNullOrEmpty(value))
            {
                DrawOverlayText(
                    g,
                    value,
                    smallFont,
                    OverlayGold,
                    245, y, 49,
                    RowHeight,
                    true);
            }

            const float switchX = 306;

            if (action)
            {
                FillRounded(
                    g,
                    new RectangleF(
                        switchX, y + 7,
                        47, 19),
                    8,
                    available
                        ? OverlayAccent
                        : OverlayBorder);

                DrawOverlayText(
                    g,
                    "RUN",
                    smallFont,
                    Color.White,
                    switchX + 5,
                    y + 7,
                    37, 19);
            }
            else
            {
                FillRounded(
                    g,
                    new RectangleF(
                        switchX, y + 7,
                        47, 19),
                    10,
                    active
                        ? OverlayAccent
                        : Color.FromArgb(59, 63, 73));

                float knobX = active
                    ? switchX + 30
                    : switchX + 3;

                using var knob = new SolidBrush(
                    Color.White);

                g.FillEllipse(
                    knob,
                    knobX, y + 10,
                    13, 13);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!Interactive ||
                e.Button != MouseButtons.Left ||
                e.Y < RowStart)
                return;

            int index = (e.Y - RowStart) / RowHeight;

            if (index < 0 || index >= featureIds.Length)
                return;

            int id = featureIds[index];

            if (id >= trainer.buttons.Length)
                return;

            if (trainer.buttons[id].Enabled)
                trainer.Send(id);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!Interactive || e.Y < RowStart)
                return;

            int index = (e.Y - RowStart) / RowHeight;

            if (index < 0 || index >= featureIds.Length)
                return;

            int id = featureIds[index];

            TrackBar? slider = trainer.GetOverlaySlider(id);

            if (slider == null)
                return;

            int step = Math.Max(1, slider.SmallChange);

            slider.Value = Math.Clamp(
                slider.Value + (Math.Sign(e.Delta) * step),
                slider.Minimum,
                slider.Maximum);

            Invalidate();
        }
    }

    private static void FillRounded(
        Graphics g,
        RectangleF bounds,
        float radius,
        Color color)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        float d = Math.Min(
            radius * 2,
            Math.Min(bounds.Width, bounds.Height));

        using var path = new GraphicsPath();

        path.AddArc(
            bounds.X, bounds.Y,
            d, d, 180, 90);

        path.AddArc(
            bounds.Right - d, bounds.Y,
            d, d, 270, 90);

        path.AddArc(
            bounds.Right - d, bounds.Bottom - d,
            d, d, 0, 90);

        path.AddArc(
            bounds.X, bounds.Bottom - d,
            d, d, 90, 90);

        path.CloseFigure();

        using var brush = new SolidBrush(color);

        g.FillPath(brush, path);
    }

    private static void DrawOverlayText(
        Graphics g,
        string text,
        Font font,
        Color color,
        float x,
        float y,
        float width,
        float height,
        bool rightAlign = false)
    {
        if (width <= 0 || height <= 0)
            return;

        using var brush = new SolidBrush(color);

        using var format = new StringFormat
        {
            Alignment = rightAlign
                ? StringAlignment.Far
                : StringAlignment.Near,

            LineAlignment = StringAlignment.Center,

            Trimming = StringTrimming.EllipsisCharacter,

            FormatFlags = StringFormatFlags.NoWrap
        };

        g.DrawString(
            text,
            font,
            brush,
            new RectangleF(
                x, y, width, height),
            format);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OverlayRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OverlayPoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(
        IntPtr hWnd,
        out OverlayRect rect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(
        IntPtr hWnd,
        ref OverlayPoint point);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(
        IntPtr hWnd);
}