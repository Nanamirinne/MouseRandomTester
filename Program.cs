using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NethardMusic
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : Form
    {
        private const int MinDelaySeconds = 20;
        private const int MaxDelaySeconds = 40;
        private const int MinMovePixels = 10;
        private const int MaxMovePixels = 25;
        private const int BrowserChromeTopPixels = 120;
        private const int BrowserSideMarginPixels = 12;
        private const int BrowserBottomMarginPixels = 24;
        private const int MinWheelClicks = 3;
        private const int MaxWheelClicks = 7;
        private const int ScrollEstimateLimit = 100;
        private const int ScrollEstimateMiddleBand = 45;
        private const int ScrollEstimateEdgeBand = 75;
        private const int WheelDelta = 120;
        private const uint MouseEventWheel = 0x0800;

        private readonly Random random = new Random();
        private readonly Timer timer = new Timer();
        private readonly Button toggleButton = new Button();
        private readonly Label statusLabel = new Label();
        private readonly Label nextRunLabel = new Label();
        private readonly Label lastActionLabel = new Label();
        private bool running;
        private int estimatedScrollPosition;

        public MainForm()
        {
            Text = "Nethard Music";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(420, 230);

            Label titleLabel = new Label();
            titleLabel.Text = "Nethard Music";
            titleLabel.Font = new Font(Font.FontFamily, 14, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(22, 20);

            Label settingsLabel = new Label();
            settingsLabel.Text = "Interval: 20-40 sec   Move: 10-25 px   Scroll: 3-7 clicks";
            settingsLabel.AutoSize = true;
            settingsLabel.Location = new Point(24, 58);

            toggleButton.Text = "Start";
            toggleButton.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
            toggleButton.Size = new Size(140, 44);
            toggleButton.Location = new Point(24, 94);
            toggleButton.Click += ToggleButton_Click;

            statusLabel.Text = "Status: stopped";
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(184, 100);

            nextRunLabel.Text = "Next action: none";
            nextRunLabel.AutoSize = true;
            nextRunLabel.Location = new Point(184, 124);

            lastActionLabel.Text = "Last action: none";
            lastActionLabel.AutoSize = true;
            lastActionLabel.Location = new Point(24, 162);

            Label noteLabel = new Label();
            noteLabel.Text = "Close this window to exit.";
            noteLabel.AutoSize = true;
            noteLabel.ForeColor = SystemColors.GrayText;
            noteLabel.Location = new Point(24, 194);

            Controls.Add(titleLabel);
            Controls.Add(settingsLabel);
            Controls.Add(toggleButton);
            Controls.Add(statusLabel);
            Controls.Add(nextRunLabel);
            Controls.Add(lastActionLabel);
            Controls.Add(noteLabel);

            timer.Tick += Timer_Tick;
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            if (running)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }

        private void Start()
        {
            running = true;
            estimatedScrollPosition = 0;
            toggleButton.Text = "Stop";
            statusLabel.Text = "Status: running";
            ScheduleNextAction();
        }

        private void Stop()
        {
            running = false;
            timer.Stop();
            toggleButton.Text = "Start";
            statusLabel.Text = "Status: stopped";
            nextRunLabel.Text = "Next action: none";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            if (!running)
            {
                return;
            }

            RunMouseStep();
            ScheduleNextAction();
        }

        private void ScheduleNextAction()
        {
            int delaySeconds = random.Next(MinDelaySeconds, MaxDelaySeconds + 1);
            timer.Interval = delaySeconds * 1000;
            timer.Start();
            nextRunLabel.Text = "Next action: about " + delaySeconds + " sec";
        }

        private void RunMouseStep()
        {
            Rectangle pageArea;
            if (!TryGetEstimatedBrowserPageArea(out pageArea))
            {
                lastActionLabel.Text = "Last action: could not estimate active browser page area";
                return;
            }

            Point point;
            if (!GetCursorPos(out point))
            {
                lastActionLabel.Text = "Last action: could not read cursor position";
                return;
            }

            int dx = RandomSignedDistance();
            int dy = RandomSignedDistance();
            int targetX = Clamp(point.X + dx, pageArea.Left, pageArea.Right);
            int targetY = Clamp(point.Y + dy, pageArea.Top, pageArea.Bottom);

            SetCursorPos(targetX, targetY);

            int wheelClicks = GetNextWheelClicks();

            mouse_event(MouseEventWheel, 0, 0, wheelClicks * WheelDelta, UIntPtr.Zero);
            UpdateEstimatedScrollPosition(wheelClicks);

            string direction = wheelClicks > 0 ? "up" : "down";
            lastActionLabel.Text = "Last action: moved to (" + targetX + ", " + targetY + "), scrolled " + direction + " " + Math.Abs(wheelClicks) + ", estimate " + estimatedScrollPosition;
        }

        private int GetNextWheelClicks()
        {
            int clicks = random.Next(MinWheelClicks, MaxWheelClicks + 1);

            if (estimatedScrollPosition <= -ScrollEstimateEdgeBand)
            {
                return -clicks;
            }

            if (estimatedScrollPosition >= ScrollEstimateEdgeBand)
            {
                return clicks;
            }

            if (estimatedScrollPosition < -ScrollEstimateMiddleBand)
            {
                return random.Next(0, 100) < 75 ? -clicks : clicks;
            }

            if (estimatedScrollPosition > ScrollEstimateMiddleBand)
            {
                return random.Next(0, 100) < 75 ? clicks : -clicks;
            }

            return random.Next(0, 2) == 0 ? -clicks : clicks;
        }

        private void UpdateEstimatedScrollPosition(int wheelClicks)
        {
            // Positive wheel data scrolls up, so the estimated page position moves toward the top.
            estimatedScrollPosition = Clamp(estimatedScrollPosition - wheelClicks * 4, -ScrollEstimateLimit, ScrollEstimateLimit);
        }

        private bool TryGetEstimatedBrowserPageArea(out Rectangle pageArea)
        {
            pageArea = Rectangle.Empty;

            IntPtr window = GetForegroundWindow();
            if (window == IntPtr.Zero || window == Handle)
            {
                return false;
            }

            NativeRect rect;
            if (!GetWindowRect(window, out rect))
            {
                return false;
            }

            int left = rect.Left + BrowserSideMarginPixels;
            int top = rect.Top + BrowserChromeTopPixels;
            int right = rect.Right - BrowserSideMarginPixels;
            int bottom = rect.Bottom - BrowserBottomMarginPixels;

            if (right - left < 100 || bottom - top < 100)
            {
                return false;
            }

            pageArea = Rectangle.FromLTRB(left, top, right, bottom);
            return true;
        }

        private int RandomSignedDistance()
        {
            int distance = random.Next(MinMovePixels, MaxMovePixels + 1);
            return random.Next(0, 2) == 0 ? -distance : distance;
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
