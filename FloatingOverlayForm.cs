using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class FloatingOverlayForm : Form
{
    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    private bool dragging;
    private Point dragStart;
    private Point windowStart;

    public bool Interactive { get; private set; }

    public Point? SavedOffset { get; private set; }

    public Rectangle GameBounds { get; private set; }

    protected virtual int DragHeaderHeight => 40;

    public FloatingOverlayForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;

        TopMost = true;
        ShowInTaskbar = false;

        BackColor = Color.FromArgb(16, 18, 22);
        ForeColor = Color.White;

        DoubleBuffered = true;
        Opacity = 0.96;

        AutoScaleMode = AutoScaleMode.None;
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;

            cp.ExStyle |= WS_EX_TOOLWINDOW;

            if (!Interactive)
            {
                cp.ExStyle |= WS_EX_TRANSPARENT;
                cp.ExStyle |= WS_EX_NOACTIVATE;
            }

            return cp;
        }
    }

    public void SetInteractive(bool enabled)
    {
        if (Interactive == enabled)
            return;

        Interactive = enabled;

        if (!IsHandleCreated)
            return;

        int style = GetWindowLong(Handle, GWL_EXSTYLE);

        if (enabled)
        {
            style &= ~WS_EX_TRANSPARENT;
            style &= ~WS_EX_NOACTIVATE;
        }
        else
        {
            style |= WS_EX_TRANSPARENT;
            style |= WS_EX_NOACTIVATE;

            Capture = false;
            dragging = false;
        }

        SetWindowLong(Handle, GWL_EXSTYLE, style);

        SetWindowPos(
            Handle,
            IntPtr.Zero,
            0, 0, 0, 0,
            0x0037);

        Invalidate();
    }

    public void PlaceWithinGame(
        Rectangle gameBounds,
        Point defaultOffset)
    {
        GameBounds = gameBounds;

        if (dragging)
            return;

        Point offset = SavedOffset ?? defaultOffset;

        offset.X = Math.Clamp(
            offset.X,
            0,
            Math.Max(0, gameBounds.Width - Width));

        offset.Y = Math.Clamp(
            offset.Y,
            0,
            Math.Max(0, gameBounds.Height - Height));

        Location = new Point(
            gameBounds.X + offset.X,
            gameBounds.Y + offset.Y);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!Interactive ||
            e.Button != MouseButtons.Left ||
            e.Y > DragHeaderHeight)
            return;

        dragging = true;

        dragStart = Cursor.Position;
        windowStart = Location;

        Capture = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!dragging)
            return;

        Point mouse = Cursor.Position;

        int x = windowStart.X + mouse.X - dragStart.X;
        int y = windowStart.Y + mouse.Y - dragStart.Y;

        x = Math.Clamp(
            x,
            GameBounds.Left,
            Math.Max(
                GameBounds.Left,
                GameBounds.Right - Width));

        y = Math.Clamp(
            y,
            GameBounds.Top,
            Math.Max(
                GameBounds.Top,
                GameBounds.Bottom - Height));

        Location = new Point(x, y);

        SavedOffset = new Point(
            x - GameBounds.X,
            y - GameBounds.Y);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        dragging = false;
        Capture = false;
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);

        if (!Capture)
            dragging = false;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(
        IntPtr hWnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong(
        IntPtr hWnd, int index, int value);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr insertAfter,
        int x, int y,
        int width, int height,
        uint flags);
}