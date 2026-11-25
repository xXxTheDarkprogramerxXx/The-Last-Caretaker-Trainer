using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

public partial class TrainerForm
{
    private MinimapForm? minimap;
    private MinimapForm? atlas;

    private bool minimapWanted = true;
    private bool atlasWanted = false;

    private bool minimapKeyDown;
    private bool minimapZoomKeyDown;
    private bool atlasKeyDown;

    private long nextAutomaticMapBind;

    private readonly int[] mapRanges =
    {
        500,
        1000,
        2500,
        5000,
        10000,
        20000
    };

    private int mapRangeIndex = 3;

    private void PollMinimapHotkeys()
    {

        // Automatically bind the minimap when the game
        // becomes the foreground application.

        if (!closing &&
            game != null &&
            minimapWanted &&
            (overlayTimer?.Enabled != true ||
             !IsWindow(overlayGameWindow)))
        {
            long now = Environment.TickCount64;

            if (now >= nextAutomaticMapBind)
            {
                nextAutomaticMapBind = now + 500;

                if (TryBindOverlayGame())
                    RefreshOverlayWindows();
            }
        }

        bool m = OverlayKeyPressed(Keys.M);
        bool n = OverlayKeyPressed(Keys.N);
        bool g = OverlayKeyPressed(Keys.G);

        if (m && !minimapKeyDown)
            ToggleMinimap();

        if (g && !atlasKeyDown)
            ToggleAtlas();

        if (n && !minimapZoomKeyDown &&
            TryBindOverlayGame())
        {
            if (atlasWanted && atlas != null)
            {
                atlas.ZoomMap(1);
            }
            else if (minimapWanted)
            {
                mapRangeIndex =
                    (mapRangeIndex + 1) % mapRanges.Length;

                minimap?.Invalidate();
            }
        }

        minimapKeyDown = m;
        minimapZoomKeyDown = n;
        atlasKeyDown = g;
    }

    private void ToggleMinimap()
    {
        if (closing || !TryBindOverlayGame())
            return;

        minimapWanted = !minimapWanted;

        RefreshOverlayWindows();
    }

    private void ToggleAtlas()
    {
        if (closing || !TryBindOverlayGame())
            return;

        atlasWanted = !atlasWanted;

        if (atlasWanted)
            atlas?.ResetMap();

        SetOverlayEditing(atlasWanted);
    }

    private void RefreshMapWindows(
        Rectangle gameBounds)
    {
        bool mapVisible =
            minimapWanted || atlasWanted;

        game?.SetMinimapEnabled(mapVisible);

        if (atlasWanted)
        {
            minimap?.Hide();

            atlas ??= new MinimapForm(
                this,
                fullScreen: true);

            atlas.SetInteractive(overlayEditing);

            if (atlas.Bounds != gameBounds)
                atlas.Bounds = gameBounds;

            if (!atlas.Visible)
                atlas.Show();

            atlas.Invalidate();

            return;
        }

        atlas?.Hide();

        if (minimapWanted)
        {
            minimap ??= new MinimapForm(
                this,
                fullScreen: false);

            minimap.SetInteractive(overlayEditing);

            minimap.PlaceWithinGame(
                gameBounds,
                new Point(
                    20,
                    gameBounds.Height -
                    minimap.Height - 20));

            if (!minimap.Visible)
                minimap.Show();

            minimap.Invalidate();
        }
        else
        {
            minimap?.Hide();
        }
    }

    private void HideMapWindows()
    {
        minimap?.Hide();
        atlas?.Hide();

        game?.SetMinimapEnabled(false);
    }

    private void CloseMinimap()
    {
        game?.SetMinimapEnabled(false);

        minimap?.Dispose();
        atlas?.Dispose();

        minimap = null;
        atlas = null;

        //minimapWanted = false;
        atlasWanted = false;
    }

    private sealed class MinimapForm
        : FloatingOverlayForm
    {
        private readonly TrainerForm trainer;

        private readonly bool fullScreen;

        private readonly TextBox? searchBox;

        private bool fitPending = true;
        private bool panning;

        private Point lastMouse;
        private Point mousePosition;

        private double centerX;
        private double centerY;

        // World centimetres represented by one pixel.
        private double unitsPerPixel = 1000;

        private RectangleF plotBounds;

        protected override int DragHeaderHeight =>
            fullScreen ? -1 : 48;

        private static readonly Color Ocean =
            Color.FromArgb(18, 31, 43);

        private static readonly Color GridColor =
            Color.FromArgb(37, 54, 66);

        private static readonly Color VirtualMarkerColor =
            Color.FromArgb(101, 184, 242);

        public MinimapForm(
            TrainerForm trainer,
            bool fullScreen)
        {
            this.trainer = trainer;
            this.fullScreen = fullScreen;

            BackColor = OverlayBackground;
            ForeColor = Color.White;

            ClientSize = new Size(340, 380);

            Opacity = fullScreen
                ? 0.98
                : 0.96;

            if (fullScreen)
            {
                searchBox = new TextBox
                {
                    BackColor = OverlaySurface,
                    ForeColor = Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    Font = new Font(
                        "Segoe UI",
                        11f),

                    PlaceholderText =
                        "Search locations..."
                };

                searchBox.TextChanged += (_, _) =>
                {
                    fitPending = true;
                    Invalidate();
                };

                Controls.Add(searchBox);

                PositionSearchBox();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            PositionSearchBox();
        }

        private void PositionSearchBox()
        {
            if (searchBox == null)
                return;

            searchBox.Width = Math.Min(
                350,
                Math.Max(
                    120,
                    ClientSize.Width / 3));

            searchBox.Location = new Point(
                Math.Max(
                    12,
                    ClientSize.Width -
                    searchBox.Width - 20),
                15);
        }

        public void ResetMap()
        {
            fitPending = true;

            Invalidate();
        }

        public void ZoomMap(int direction)
        {
            if (fullScreen)
            {
                if (fitPending)
                    return;

                unitsPerPixel = Math.Clamp(
                    unitsPerPixel *
                    Math.Pow(0.8, direction),
                    10,
                    10000000);

                Invalidate();

                return;
            }

            int count = trainer.mapRanges.Length;

            trainer.mapRangeIndex =
                (trainer.mapRangeIndex +
                 direction + count) % count;

            Invalidate();
        }

        private Game.MapMarker[] GetVisibleMarkers(
            Game.MapSnapshot snapshot)
        {
            string search =
                searchBox?.Text.Trim() ?? "";

            if (string.IsNullOrEmpty(search))
                return snapshot.Markers;

            return snapshot.Markers
                .Where(marker =>
                    marker.Name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        private void FitMap(
            Game.MapSnapshot snapshot,
            Game.MapMarker[] markers,
            RectangleF bounds)
        {
            double minX = snapshot.X;
            double maxX = snapshot.X;

            double minY = snapshot.Y;
            double maxY = snapshot.Y;

            foreach (var marker in markers)
            {
                minX = Math.Min(minX, marker.X);
                maxX = Math.Max(maxX, marker.X);

                minY = Math.Min(minY, marker.Y);
                maxY = Math.Max(maxY, marker.Y);
            }

            centerX = (minX + maxX) / 2.0;
            centerY = (minY + maxY) / 2.0;

            double worldWidth = Math.Max(
                100000,
                maxY - minY);

            double worldHeight = Math.Max(
                100000,
                maxX - minX);

            unitsPerPixel = Math.Clamp(
                Math.Max(
                    worldWidth /
                    Math.Max(1, bounds.Width - 80),

                    worldHeight /
                    Math.Max(1, bounds.Height - 80))
                * 1.2,
                10,
                10000000);

            if (markers.Length > 0 ||
                !string.IsNullOrEmpty(searchBox?.Text))
            {
                fitPending = false;
            }
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
                "Segoe UI",
                fullScreen ? 23f : 17f,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            using var normalFont = new Font(
                "Segoe UI",
                12f,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            using var smallFont = new Font(
                "Segoe UI",
                11f,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            using var border = new Pen(OverlayBorder);

            g.DrawRectangle(
                border,
                0, 0,
                ClientSize.Width - 1,
                ClientSize.Height - 1);

            DrawOverlayText(
                g,
                fullScreen
                    ? "WORLD ATLAS"
                    : "⠿  NAVIGATION",
                titleFont,
                Color.White,
                16, 7,
                ClientSize.Width / 2,
                35);

            if (fullScreen)
            {
                DrawOverlayText(
                    g,
                    "Registered world locations",
                    smallFont,
                    OverlayMuted,
                    18, 34,
                    ClientSize.Width - 40,
                    21);
            }

            Game.MapSnapshot? snapshot =
                trainer.game?.LatestMinimap;

            if (fullScreen)
            {
                plotBounds = new RectangleF(
                    15, 65,
                    Math.Max(
                        1,
                        ClientSize.Width - 30),
                    Math.Max(
                        1,
                        ClientSize.Height - 125));
            }
            else
            {
                float diameter = Math.Max(
                    1,
                    Math.Min(
                        ClientSize.Width - 66,
                        ClientSize.Height - 115));

                plotBounds = new RectangleF(
                    (ClientSize.Width - diameter) / 2,
                    50,
                    diameter,
                    diameter);

                DrawOverlayText(
                    g,
                    snapshot == null
                        ? "WAITING"
                        : "LIVE",
                    smallFont,
                    snapshot == null
                        ? OverlayGold
                        : OverlayGreen,
                    ClientSize.Width - 90,
                    10,
                    70, 30,
                    true);
            }

            using var mapBrush =
                new SolidBrush(Ocean);

            using var clip = new GraphicsPath();

            if (fullScreen)
            {
                clip.AddRectangle(plotBounds);

                g.FillRectangle(
                    mapBrush,
                    plotBounds);
            }
            else
            {
                clip.AddEllipse(plotBounds);

                g.FillEllipse(
                    mapBrush,
                    plotBounds);
            }

            GraphicsState saved = g.Save();

            g.SetClip(clip);

            DrawGrid(g);

            Game.MapMarker? nearest = null;
            Game.MapMarker? hovered = null;

            double nearestDistance = double.MaxValue;
            double hoverDistance = double.MaxValue;

            int markerCount = 0;

            if (snapshot != null)
            {
                Game.MapMarker[] markers =
                    GetVisibleMarkers(snapshot);

                markerCount = markers.Length;

                if (fullScreen)
                {
                    if (fitPending)
                        FitMap(
                            snapshot,
                            markers,
                            plotBounds);
                }
                else
                {
                    centerX = snapshot.X;
                    centerY = snapshot.Y;

                    double range =
                        trainer.mapRanges[
                            trainer.mapRangeIndex];

                    unitsPerPixel =
                        (range * 100.0) /
                        (plotBounds.Width / 2.0);
                }

                var occupied =
                    new List<RectangleF>();

                foreach (var marker in markers)
                {
                    double dx =
                        (marker.X - snapshot.X) / 100.0;

                    double dy =
                        (marker.Y - snapshot.Y) / 100.0;

                    double distance = Math.Sqrt(
                        dx * dx + dy * dy);

                    if (distance >= 1 &&
                        distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = marker;
                    }

                    PointF point = ToScreen(
                        marker.X,
                        marker.Y);

                    if (!plotBounds.Contains(point))
                        continue;

                    if (!fullScreen)
                    {
                        float radius =
                            plotBounds.Width / 2f;

                        float offsetX =
                            point.X -
                            (plotBounds.Left + radius);

                        float offsetY =
                            point.Y -
                            (plotBounds.Top + radius);

                        if (offsetX * offsetX +
                            offsetY * offsetY >
                            radius * radius)
                        {
                            continue;
                        }
                    }

                    Color color = marker.Virtual
                        ? VirtualMarkerColor
                        : OverlayGold;

                    using var markerBrush =
                        new SolidBrush(color);

                    g.FillEllipse(
                        markerBrush,
                        point.X - 4,
                        point.Y - 4,
                        8, 8);

                    if (fullScreen)
                    {
                        DrawMarkerLabel(
                            g,
                            marker.Name,
                            point,
                            smallFont,
                            occupied);
                    }

                    if (Interactive)
                    {
                        double mx =
                            point.X - mousePosition.X;

                        double my =
                            point.Y - mousePosition.Y;

                        double pixelDistance =
                            Math.Sqrt(mx * mx + my * my);

                        if (pixelDistance < 15 &&
                            pixelDistance < hoverDistance)
                        {
                            hovered = marker;
                            hoverDistance = pixelDistance;
                        }
                    }
                }

                DrawPlayer(g, snapshot);

                if (hovered != null)
                {
                    DrawHoveredMarker(
                        g,
                        hovered,
                        snapshot,
                        normalFont);
                }
            }

            g.Restore(saved);

            using var mapBorder =
                new Pen(OverlayAccent, 1.3f);

            if (fullScreen)
                g.DrawRectangle(
                    mapBorder,
                    plotBounds.X,
                    plotBounds.Y,
                    plotBounds.Width,
                    plotBounds.Height);
            else
                g.DrawEllipse(
                    mapBorder,
                    plotBounds);

            if (fullScreen)
            {
                DrawAtlasFooter(
                    g,
                    snapshot,
                    markerCount,
                    normalFont,
                    smallFont);
            }
            else
            {
                DrawMinimapFooter(
                    g,
                    snapshot,
                    nearest,
                    nearestDistance,
                    normalFont,
                    smallFont);
            }
        }

        private void DrawGrid(Graphics g)
        {
            using var pen =
                new Pen(GridColor);

            int spacing = fullScreen ? 60 : 30;

            for (float x = plotBounds.Left;
                 x < plotBounds.Right;
                 x += spacing)
            {
                g.DrawLine(
                    pen,
                    x, plotBounds.Top,
                    x, plotBounds.Bottom);
            }

            for (float y = plotBounds.Top;
                 y < plotBounds.Bottom;
                 y += spacing)
            {
                g.DrawLine(
                    pen,
                    plotBounds.Left, y,
                    plotBounds.Right, y);
            }

            if (!fullScreen)
            {
                pen.DashStyle =
                    DashStyle.Dash;

                float cx =
                    plotBounds.Left +
                    plotBounds.Width / 2;

                float cy =
                    plotBounds.Top +
                    plotBounds.Height / 2;

                for (int i = 1; i <= 3; i++)
                {
                    float r =
                        plotBounds.Width / 6f * i;

                    g.DrawEllipse(
                        pen,
                        cx - r,
                        cy - r,
                        r * 2,
                        r * 2);
                }
            }
        }

        private PointF ToScreen(
            double worldX,
            double worldY)
        {
            float cx =
                plotBounds.Left +
                plotBounds.Width / 2;

            float cy =
                plotBounds.Top +
                plotBounds.Height / 2;

            return new PointF(
                cx + (float)(
                    (worldY - centerY) /
                    unitsPerPixel),

                cy - (float)(
                    (worldX - centerX) /
                    unitsPerPixel));
        }

        private void DrawPlayer(
            Graphics g,
            Game.MapSnapshot snapshot)
        {
            PointF point = ToScreen(
                snapshot.X,
                snapshot.Y);

            if (!plotBounds.Contains(point))
                return;

            using var glow = new SolidBrush(
                Color.FromArgb(65, OverlayGreen));

            g.FillEllipse(
                glow,
                point.X - 17,
                point.Y - 17,
                34, 34);

            GraphicsState saved = g.Save();

            g.TranslateTransform(
                point.X, point.Y);

            g.RotateTransform(
                (float)snapshot.Heading);

            using var brush =
                new SolidBrush(OverlayGreen);

            g.FillPolygon(
                brush,
                new[]
                {
                    new PointF(0, -13),
                    new PointF(9, 10),
                    new PointF(0, 6),
                    new PointF(-9, 10)
                });

            g.Restore(saved);
        }

        private void DrawMarkerLabel(
            Graphics g,
            string name,
            PointF point,
            Font font,
            List<RectangleF> occupied)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            SizeF measured =
                g.MeasureString(name, font);

            float width = Math.Min(
                220,
                measured.Width + 12);

            var bounds = new RectangleF(
                point.X + 8,
                point.Y - 10,
                width,
                20);

            if (bounds.Right > plotBounds.Right)
                bounds.X = point.X - width - 8;

            if (!plotBounds.Contains(bounds))
                return;

            // Avoid overlapping labels at wide zoom.
            if (occupied.Any(
                existing =>
                    existing.IntersectsWith(bounds)))
            {
                return;
            }

            occupied.Add(bounds);

            FillRounded(
                g,
                bounds,
                5,
                Color.FromArgb(225, 16, 18, 22));

            DrawOverlayText(
                g,
                name,
                font,
                Color.White,
                bounds.X + 5,
                bounds.Y,
                bounds.Width - 10,
                bounds.Height);
        }

        private void DrawHoveredMarker(
            Graphics g,
            Game.MapMarker marker,
            Game.MapSnapshot snapshot,
            Font font)
        {
            double dx =
                (marker.X - snapshot.X) / 100;

            double dy =
                (marker.Y - snapshot.Y) / 100;

            double distance =
                Math.Sqrt(dx * dx + dy * dy);

            string text =
                $"{marker.Name}  |  {FormatDistance(distance)}";

            SizeF size = g.MeasureString(text, font);

            float width = Math.Min(
                plotBounds.Width - 10,
                size.Width + 20);

            if (width <= 0)
                return;

            var bounds = new RectangleF(
                Math.Clamp(
                    mousePosition.X + 12f,
                    plotBounds.Left,
                    Math.Max(
                        plotBounds.Left,
                        plotBounds.Right - width)),

                Math.Clamp(
                    mousePosition.Y + 15f,
                    plotBounds.Top,
                    Math.Max(
                        plotBounds.Top,
                        plotBounds.Bottom - 28)),

                width,
                28);

            FillRounded(
                g,
                bounds,
                6,
                OverlayBackground);

            DrawOverlayText(
                g,
                text,
                font,
                Color.White,
                bounds.X + 8,
                bounds.Y,
                bounds.Width - 16,
                bounds.Height);
        }

        private void DrawMinimapFooter(
            Graphics g,
            Game.MapSnapshot? snapshot,
            Game.MapMarker? nearest,
            double nearestDistance,
            Font normalFont,
            Font smallFont)
        {
            int y = ClientSize.Height - 54;

            int range = trainer.mapRanges[
                trainer.mapRangeIndex];

            DrawOverlayText(
                g,
                $"RANGE  {FormatDistance(range)}",
                normalFont,
                OverlayGold,
                16, y, 180, 20);

            if (snapshot != null)
            {
                DrawOverlayText(
                    g,
                    $"{snapshot.Heading:000}°",
                    normalFont,
                    Color.White,
                    ClientSize.Width - 85,
                    y, 65, 20,
                    true);
            }

            string nearestText =
                nearest != null
                    ? $"{nearest.Name} • " +
                      FormatDistance(nearestDistance)
                    : "Waiting for registered locations";

            DrawOverlayText(
                g,
                nearestText,
                smallFont,
                Color.White,
                16, y + 19,
                ClientSize.Width - 32,
                17);

            DrawOverlayText(
                g,
                "ALT + G WORLD MAP   |   ALT + N ZOOM",
                smallFont,
                OverlayMuted,
                16, y + 36,
                ClientSize.Width - 32,
                15);
        }

        private void DrawAtlasFooter(
            Graphics g,
            Game.MapSnapshot? snapshot,
            int markerCount,
            Font normalFont,
            Font smallFont)
        {
            int y = ClientSize.Height - 52;

            string status = snapshot == null
                ? "Waiting for world data..."
                : $"{markerCount} registered locations";

            DrawOverlayText(
                g,
                status,
                normalFont,
                OverlayGreen,
                18, y,
                ClientSize.Width - 36,
                20);

            DrawOverlayText(
                g,
                "DRAG: PAN   |   WHEEL: ZOOM   |   " +
                "DOUBLE CLICK: RESET   |   ALT + G: CLOSE",
                smallFont,
                OverlayMuted,
                18, y + 22,
                ClientSize.Width - 36,
                20);
        }

        private static string FormatDistance(
            double metres)
        {
            return metres >= 1000
                ? $"{metres / 1000.0:0.##} km"
                : $"{metres:0} m";
        }

        protected override void OnMouseDown(
            MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!fullScreen ||
                !Interactive ||
                e.Button != MouseButtons.Left ||
                !plotBounds.Contains(e.Location))
                return;

            panning = true;

            lastMouse = e.Location;

            Capture = true;

            Cursor = Cursors.SizeAll;
        }

        protected override void OnMouseMove(
            MouseEventArgs e)
        {
            base.OnMouseMove(e);

            mousePosition = e.Location;

            if (panning && Interactive)
            {
                int dx = e.X - lastMouse.X;
                int dy = e.Y - lastMouse.Y;

                centerY -= dx * unitsPerPixel;

                centerX += dy * unitsPerPixel;

                lastMouse = e.Location;

                fitPending = false;
            }

            if (Interactive)
                Invalidate();
        }

        protected override void OnMouseUp(
            MouseEventArgs e)
        {
            base.OnMouseUp(e);

            panning = false;

            Capture = false;

            Cursor = Cursors.Default;
        }

        protected override void OnMouseCaptureChanged(
            EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (!Capture)
            {
                panning = false;
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseWheel(
            MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!Interactive)
                return;

            if (fullScreen)
                ZoomMap(Math.Sign(e.Delta));
            else
                ZoomMap(-Math.Sign(e.Delta));
        }

        protected override void OnMouseDoubleClick(
            MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (fullScreen && Interactive)
                ResetMap();
        }
    }
}