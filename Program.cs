using System;
using System.Drawing;
using System.IO;
using System.Media;
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
        private const int DefaultMinDelaySeconds = 10;
        private const int DefaultMaxDelaySeconds = 20;
        private const int MinDelaySeconds = 1;
        private const int MaxDelaySeconds = 3600;
        private const int DefaultRunDurationMinutes = 0;
        private const int MinRunDurationMinutes = 0;
        private const int MaxRunDurationMinutes = 1440;
        private const int MinMovePixels = 10;
        private const int MaxMovePixels = 25;
        private const int BrowserChromeTopPixels = 120;
        private const int BrowserSideMarginPixels = 12;
        private const int BrowserBottomMarginPixels = 24;
        private const int MinWheelClicks = 3;
        private const int MaxWheelClicks = 7;
        private const int MinTypedCharacters = 3;
        private const int MaxTypedCharacters = 10;
        private const int MinTypingDelayMilliseconds = 90;
        private const int MaxTypingDelayMilliseconds = 260;
        private const int HesitationChancePercent = 18;
        private const int MinHesitationMilliseconds = 300;
        private const int MaxHesitationMilliseconds = 750;
        private const int AlarmRepeatCount = 4;
        private const int AlarmSampleRate = 44100;
        private const short AlarmBitsPerSample = 16;
        private const short AlarmChannels = 1;
        private const short AlarmVolume = 26000;
        private const int VisualAlertFlashIntervalMilliseconds = 500;
        private const int VisualAlertFlashDurationSeconds = 10;
        private const int ToggleHotKeyId = 100;
        private const int WmHotKey = 0x0312;
        private const uint HotKeyModifierAlt = 0x0001;
        private const uint HotKeyModifierControl = 0x0002;
        private const uint HotKeyModifierShift = 0x0004;
        private const uint HotKeyModifierNoRepeat = 0x4000;
        private const int ScrollEstimateLimit = 100;
        private const int ScrollEstimateMiddleBand = 45;
        private const int ScrollEstimateEdgeBand = 75;
        private const int WheelDelta = 120;
        private const uint MouseEventWheel = 0x0800;
        private const uint FlashWindowStop = 0x00000000;
        private const uint FlashWindowAll = 0x00000003;
        private const uint FlashWindowTimer = 0x00000004;
        private const string DefaultHotKeyKeyName = "M";
        private const string TypingCharacters = "abcdefghijklmnopqrstuvwxyz0123456789     ";

        private readonly Random random = new Random();
        private readonly Timer timer = new Timer();
        private readonly Button toggleButton = new Button();
        private readonly Button testAlarmButton = new Button();
        private readonly NumericUpDown minDelayInput = new NumericUpDown();
        private readonly NumericUpDown maxDelayInput = new NumericUpDown();
        private readonly NumericUpDown runDurationMinutesInput = new NumericUpDown();
        private readonly CheckBox keyboardActionsCheckBox = new CheckBox();
        private readonly CheckBox hotKeyCtrlCheckBox = new CheckBox();
        private readonly CheckBox hotKeyAltCheckBox = new CheckBox();
        private readonly CheckBox hotKeyShiftCheckBox = new CheckBox();
        private readonly ComboBox hotKeyKeyComboBox = new ComboBox();
        private readonly Label statusLabel = new Label();
        private readonly Label nextRunLabel = new Label();
        private readonly Label lastActionLabel = new Label();
        private readonly Label hotKeyLabel = new Label();
        private bool running;
        private bool hasRunDuration;
        private bool hotKeyRegistered;
        private DateTime runEndsAt;
        private SoundPlayer completionAlarmPlayer;
        private MemoryStream completionAlarmStream;
        private int estimatedScrollPosition;

        public MainForm()
        {
            Text = "Nethard Music";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(520, 430);

            Label titleLabel = new Label();
            titleLabel.Text = "Nethard Music";
            titleLabel.Font = new Font(Font.FontFamily, 14, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(22, 20);

            Label intervalLabel = new Label();
            intervalLabel.Text = "Interval between actions";
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new Point(24, 58);

            Label minDelayLabel = new Label();
            minDelayLabel.Text = "Min seconds";
            minDelayLabel.AutoSize = true;
            minDelayLabel.Location = new Point(24, 88);

            ConfigureDelayInput(minDelayInput, DefaultMinDelaySeconds, new Point(108, 84));

            Label maxDelayLabel = new Label();
            maxDelayLabel.Text = "Max seconds";
            maxDelayLabel.AutoSize = true;
            maxDelayLabel.Location = new Point(210, 88);

            ConfigureDelayInput(maxDelayInput, DefaultMaxDelaySeconds, new Point(296, 84));

            Label settingsLabel = new Label();
            settingsLabel.Text = "Move: 10-25 px   Scroll: 3-7 clicks";
            settingsLabel.AutoSize = true;
            settingsLabel.Location = new Point(24, 156);

            Label durationLabel = new Label();
            durationLabel.Text = "Run minutes";
            durationLabel.AutoSize = true;
            durationLabel.Location = new Point(24, 122);

            ConfigureRunDurationInput(runDurationMinutesInput, DefaultRunDurationMinutes, new Point(108, 118));

            Label durationHintLabel = new Label();
            durationHintLabel.Text = "0 = until stopped";
            durationHintLabel.AutoSize = true;
            durationHintLabel.ForeColor = SystemColors.GrayText;
            durationHintLabel.Location = new Point(210, 122);

            keyboardActionsCheckBox.Text = "Keyboard actions: random typing";
            keyboardActionsCheckBox.AutoSize = true;
            keyboardActionsCheckBox.Checked = true;
            keyboardActionsCheckBox.Location = new Point(24, 184);

            toggleButton.Text = "Start";
            toggleButton.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
            toggleButton.Size = new Size(140, 44);
            toggleButton.Location = new Point(24, 296);
            toggleButton.Click += ToggleButton_Click;

            testAlarmButton.Text = "Test alert";
            testAlarmButton.Size = new Size(110, 30);
            testAlarmButton.Location = new Point(386, 183);
            testAlarmButton.Click += TestAlarmButton_Click;

            Label hotKeyConfigLabel = new Label();
            hotKeyConfigLabel.Text = "Hotkey";
            hotKeyConfigLabel.AutoSize = true;
            hotKeyConfigLabel.Location = new Point(24, 218);

            ConfigureHotKeyCheckBox(hotKeyCtrlCheckBox, "Ctrl", true, new Point(78, 214));
            ConfigureHotKeyCheckBox(hotKeyAltCheckBox, "Alt", true, new Point(130, 214));
            ConfigureHotKeyCheckBox(hotKeyShiftCheckBox, "Shift", false, new Point(176, 214));
            ConfigureHotKeyKeyComboBox();

            hotKeyLabel.Text = "Hotkey: registering";
            hotKeyLabel.AutoSize = true;
            hotKeyLabel.ForeColor = SystemColors.GrayText;
            hotKeyLabel.Location = new Point(24, 246);

            statusLabel.Text = "Status: stopped";
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(184, 302);

            nextRunLabel.Text = "Next action: none";
            nextRunLabel.AutoSize = true;
            nextRunLabel.Location = new Point(184, 326);

            lastActionLabel.Text = "Last action: none";
            lastActionLabel.AutoSize = false;
            lastActionLabel.Size = new Size(470, 36);
            lastActionLabel.Location = new Point(24, 356);

            Label noteLabel = new Label();
            noteLabel.Text = "Close this window to exit.";
            noteLabel.AutoSize = true;
            noteLabel.ForeColor = SystemColors.GrayText;
            noteLabel.Location = new Point(24, 404);

            Controls.Add(titleLabel);
            Controls.Add(intervalLabel);
            Controls.Add(minDelayLabel);
            Controls.Add(minDelayInput);
            Controls.Add(maxDelayLabel);
            Controls.Add(maxDelayInput);
            Controls.Add(durationLabel);
            Controls.Add(runDurationMinutesInput);
            Controls.Add(durationHintLabel);
            Controls.Add(settingsLabel);
            Controls.Add(keyboardActionsCheckBox);
            Controls.Add(toggleButton);
            Controls.Add(testAlarmButton);
            Controls.Add(hotKeyConfigLabel);
            Controls.Add(hotKeyCtrlCheckBox);
            Controls.Add(hotKeyAltCheckBox);
            Controls.Add(hotKeyShiftCheckBox);
            Controls.Add(hotKeyKeyComboBox);
            Controls.Add(hotKeyLabel);
            Controls.Add(statusLabel);
            Controls.Add(nextRunLabel);
            Controls.Add(lastActionLabel);
            Controls.Add(noteLabel);

            timer.Tick += Timer_Tick;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterToggleHotKey();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterToggleHotKey();
            base.OnHandleDestroyed(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopCompletionAlarm();
            base.OnFormClosed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotKey && m.WParam.ToInt32() == ToggleHotKeyId)
            {
                ToggleRunning();
                return;
            }

            base.WndProc(ref m);
        }

        private void RegisterToggleHotKey()
        {
            UnregisterToggleHotKey();

            uint modifiers = GetSelectedHotKeyModifiers();
            Keys key = GetSelectedHotKeyKey();
            string hotKeyText = GetSelectedHotKeyText();

            if (modifiers == 0)
            {
                hotKeyLabel.Text = "Hotkey unavailable: choose Ctrl, Alt, or Shift";
                hotKeyLabel.ForeColor = Color.Firebrick;
                return;
            }

            if (key == Keys.None)
            {
                hotKeyLabel.Text = "Hotkey unavailable: choose a key";
                hotKeyLabel.ForeColor = Color.Firebrick;
                return;
            }

            hotKeyRegistered = RegisterHotKey(Handle, ToggleHotKeyId, modifiers | HotKeyModifierNoRepeat, (uint)key);

            if (hotKeyRegistered)
            {
                hotKeyLabel.Text = "Hotkey: " + hotKeyText + " toggles Start/Stop";
                hotKeyLabel.ForeColor = SystemColors.GrayText;
            }
            else
            {
                hotKeyLabel.Text = "Hotkey unavailable: " + hotKeyText;
                hotKeyLabel.ForeColor = Color.Firebrick;
            }
        }

        private uint GetSelectedHotKeyModifiers()
        {
            uint modifiers = 0;

            if (hotKeyCtrlCheckBox.Checked)
            {
                modifiers |= HotKeyModifierControl;
            }

            if (hotKeyAltCheckBox.Checked)
            {
                modifiers |= HotKeyModifierAlt;
            }

            if (hotKeyShiftCheckBox.Checked)
            {
                modifiers |= HotKeyModifierShift;
            }

            return modifiers;
        }

        private Keys GetSelectedHotKeyKey()
        {
            if (hotKeyKeyComboBox.SelectedItem == null)
            {
                return Keys.None;
            }

            string keyName = hotKeyKeyComboBox.SelectedItem.ToString();

            if (keyName.Length == 1)
            {
                return (Keys)Enum.Parse(typeof(Keys), keyName);
            }

            if (keyName.Length > 1 && keyName[0] == 'F')
            {
                return (Keys)Enum.Parse(typeof(Keys), keyName);
            }

            return Keys.None;
        }

        private string GetSelectedHotKeyText()
        {
            string text = "";

            if (hotKeyCtrlCheckBox.Checked)
            {
                text = AppendHotKeyPart(text, "Ctrl");
            }

            if (hotKeyAltCheckBox.Checked)
            {
                text = AppendHotKeyPart(text, "Alt");
            }

            if (hotKeyShiftCheckBox.Checked)
            {
                text = AppendHotKeyPart(text, "Shift");
            }

            if (hotKeyKeyComboBox.SelectedItem != null)
            {
                text = AppendHotKeyPart(text, hotKeyKeyComboBox.SelectedItem.ToString());
            }

            return text.Length == 0 ? "(none)" : text;
        }

        private static string AppendHotKeyPart(string text, string part)
        {
            if (text.Length == 0)
            {
                return part;
            }

            return text + "+" + part;
        }

        private void UnregisterToggleHotKey()
        {
            if (!hotKeyRegistered)
            {
                return;
            }

            UnregisterHotKey(Handle, ToggleHotKeyId);
            hotKeyRegistered = false;
        }

        private void ToggleRunning()
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

        private void ConfigureDelayInput(NumericUpDown input, int value, Point location)
        {
            input.Minimum = MinDelaySeconds;
            input.Maximum = MaxDelaySeconds;
            input.Value = value;
            input.Size = new Size(78, 22);
            input.Location = location;
            input.ValueChanged += DelayInput_ValueChanged;
        }

        private void ConfigureRunDurationInput(NumericUpDown input, int value, Point location)
        {
            input.Minimum = MinRunDurationMinutes;
            input.Maximum = MaxRunDurationMinutes;
            input.Value = value;
            input.Size = new Size(78, 22);
            input.Location = location;
            input.ValueChanged += RunDurationInput_ValueChanged;
        }

        private void ConfigureHotKeyCheckBox(CheckBox checkBox, string text, bool isChecked, Point location)
        {
            checkBox.Text = text;
            checkBox.AutoSize = true;
            checkBox.Checked = isChecked;
            checkBox.Location = location;
            checkBox.CheckedChanged += HotKeyInput_Changed;
        }

        private void ConfigureHotKeyKeyComboBox()
        {
            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                hotKeyKeyComboBox.Items.Add(letter.ToString());
            }

            for (int i = 1; i <= 12; i++)
            {
                hotKeyKeyComboBox.Items.Add("F" + i);
            }

            hotKeyKeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            hotKeyKeyComboBox.Size = new Size(72, 22);
            hotKeyKeyComboBox.Location = new Point(242, 212);
            hotKeyKeyComboBox.SelectedItem = DefaultHotKeyKeyName;
            hotKeyKeyComboBox.SelectedIndexChanged += HotKeyInput_Changed;
        }

        private void DelayInput_ValueChanged(object sender, EventArgs e)
        {
            NormalizeDelayInputs(sender);

            if (running)
            {
                ScheduleNextAction();
            }
        }

        private void RunDurationInput_ValueChanged(object sender, EventArgs e)
        {
            if (running)
            {
                ApplyRunDurationFromInput();
                ScheduleNextAction();
            }
        }

        private void HotKeyInput_Changed(object sender, EventArgs e)
        {
            if (IsHandleCreated)
            {
                RegisterToggleHotKey();
            }
        }

        private void NormalizeDelayInputs(object changedInput)
        {
            if (minDelayInput.Value <= maxDelayInput.Value)
            {
                return;
            }

            if (changedInput == minDelayInput)
            {
                maxDelayInput.Value = minDelayInput.Value;
            }
            else
            {
                minDelayInput.Value = maxDelayInput.Value;
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            ToggleRunning();
        }

        private void TestAlarmButton_Click(object sender, EventArgs e)
        {
            PlayCompletionAlarm();
            ShowCompletionAlert("This is a visual alert test.");
            StopCompletionAlarm();
        }

        private void Start()
        {
            NormalizeDelayInputs(null);
            running = true;
            estimatedScrollPosition = 0;
            ApplyRunDurationFromInput();
            toggleButton.Text = "Stop";
            UpdateRunningStatus();
            ScheduleNextAction();
        }

        private void Stop()
        {
            Stop("Status: stopped");
        }

        private void Stop(string statusText)
        {
            running = false;
            hasRunDuration = false;
            timer.Stop();
            toggleButton.Text = "Start";
            statusLabel.Text = statusText;
            nextRunLabel.Text = "Next action: none";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            if (!running)
            {
                return;
            }

            if (IsRunDurationExpired())
            {
                StopAfterDurationEnded();
                lastActionLabel.Text = "Last action: stopped after configured duration";
                return;
            }

            RunMouseStep();

            if (!running)
            {
                return;
            }

            ScheduleNextAction();
        }

        private void ScheduleNextAction()
        {
            // SendKeys.SendWait can process a stop hotkey while an action is still
            // unwinding. Never let that stale action restart the timer or status.
            if (!running)
            {
                timer.Stop();
                return;
            }

            if (IsRunDurationExpired())
            {
                StopAfterDurationEnded();
                lastActionLabel.Text = "Last action: stopped after configured duration";
                return;
            }

            int minDelaySeconds = (int)minDelayInput.Value;
            int maxDelaySeconds = (int)maxDelayInput.Value;
            int delaySeconds = random.Next(minDelaySeconds, maxDelaySeconds + 1);

            if (hasRunDuration)
            {
                int remainingSeconds = GetRemainingRunSeconds();

                if (delaySeconds > remainingSeconds)
                {
                    timer.Interval = Math.Max(1, remainingSeconds) * 1000;
                    timer.Start();
                    nextRunLabel.Text = "Auto stop: about " + FormatDuration(remainingSeconds);
                    UpdateRunningStatus();
                    return;
                }
            }

            timer.Interval = delaySeconds * 1000;
            timer.Start();
            nextRunLabel.Text = "Next action: about " + delaySeconds + " sec";
            UpdateRunningStatus();
        }

        private void StopAfterDurationEnded()
        {
            Stop("Status: stopped (duration ended)");
            PlayCompletionAlarm();
            ShowCompletionAlert("Configured run duration ended.");
            StopCompletionAlarm();
        }

        private void PlayCompletionAlarm()
        {
            StopCompletionAlarm();

            completionAlarmStream = CreateCompletionAlarmStream();
            completionAlarmPlayer = new SoundPlayer(completionAlarmStream);
            completionAlarmPlayer.Load();
            completionAlarmPlayer.Play();
        }

        private void StopCompletionAlarm()
        {
            if (completionAlarmPlayer != null)
            {
                completionAlarmPlayer.Stop();
                completionAlarmPlayer.Dispose();
                completionAlarmPlayer = null;
            }

            if (completionAlarmStream != null)
            {
                completionAlarmStream.Dispose();
                completionAlarmStream = null;
            }
        }

        private MemoryStream CreateCompletionAlarmStream()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            int dataSize = GetAlarmDataSize();
            int byteRate = AlarmSampleRate * AlarmChannels * AlarmBitsPerSample / 8;
            short blockAlign = (short)(AlarmChannels * AlarmBitsPerSample / 8);

            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(AlarmChannels);
            writer.Write(AlarmSampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(AlarmBitsPerSample);
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            for (int i = 0; i < AlarmRepeatCount; i++)
            {
                WriteTone(writer, 880, 220);
                WriteTone(writer, 660, 220);
                WriteTone(writer, 990, 360);
                WriteSilence(writer, 180);
            }

            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static int GetAlarmDataSize()
        {
            int totalMilliseconds = AlarmRepeatCount * (220 + 220 + 360 + 180);
            return AlarmSampleRate * totalMilliseconds / 1000 * AlarmChannels * AlarmBitsPerSample / 8;
        }

        private static void WriteTone(BinaryWriter writer, int frequency, int milliseconds)
        {
            int samples = AlarmSampleRate * milliseconds / 1000;

            for (int i = 0; i < samples; i++)
            {
                double angle = 2.0 * Math.PI * frequency * i / AlarmSampleRate;
                short sample = (short)(Math.Sin(angle) * AlarmVolume);
                writer.Write(sample);
            }
        }

        private static void WriteSilence(BinaryWriter writer, int milliseconds)
        {
            int samples = AlarmSampleRate * milliseconds / 1000;

            for (int i = 0; i < samples; i++)
            {
                writer.Write((short)0);
            }
        }

        private void ShowCompletionAlert(string message)
        {
            bool wasTopMost = TopMost;
            Form alert = new Form();
            Timer flashTimer = new Timer();
            Label alertLabel = new Label();
            Button acknowledgeButton = new Button();
            int flashTicks = 0;

            alert.Text = "Nethard Music - Alert";
            alert.StartPosition = FormStartPosition.CenterScreen;
            alert.FormBorderStyle = FormBorderStyle.FixedDialog;
            alert.MaximizeBox = false;
            alert.MinimizeBox = false;
            alert.ShowInTaskbar = false;
            alert.TopMost = true;
            alert.ClientSize = new Size(620, 240);
            alert.BackColor = Color.Firebrick;

            alertLabel.Text = message;
            alertLabel.Font = new Font(Font.FontFamily, 20, FontStyle.Bold);
            alertLabel.ForeColor = Color.White;
            alertLabel.TextAlign = ContentAlignment.MiddleCenter;
            alertLabel.Location = new Point(30, 35);
            alertLabel.Size = new Size(560, 100);

            acknowledgeButton.Text = "Acknowledge";
            acknowledgeButton.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
            acknowledgeButton.DialogResult = DialogResult.OK;
            acknowledgeButton.Location = new Point(230, 165);
            acknowledgeButton.Size = new Size(160, 44);

            alert.Controls.Add(alertLabel);
            alert.Controls.Add(acknowledgeButton);
            alert.AcceptButton = acknowledgeButton;
            alert.CancelButton = acknowledgeButton;

            flashTimer.Interval = VisualAlertFlashIntervalMilliseconds;
            flashTimer.Tick += delegate
            {
                flashTicks++;

                if (flashTicks >= VisualAlertFlashDurationSeconds * 1000 / VisualAlertFlashIntervalMilliseconds)
                {
                    flashTimer.Stop();
                    alert.BackColor = Color.Firebrick;
                    alertLabel.ForeColor = Color.White;
                    return;
                }

                bool showRed = flashTicks % 2 == 0;
                alert.BackColor = showRed ? Color.Firebrick : Color.White;
                alertLabel.ForeColor = showRed ? Color.White : Color.Firebrick;
            };

            try
            {
                TopMost = true;
                Show();
                Activate();
                FlashTaskbar(FlashWindowAll | FlashWindowTimer);
                flashTimer.Start();
                alert.ShowDialog(this);
            }
            finally
            {
                flashTimer.Stop();
                FlashTaskbar(FlashWindowStop);
                TopMost = wasTopMost;
                flashTimer.Dispose();
                alert.Dispose();
            }
        }

        private void FlashTaskbar(uint flags)
        {
            FlashWindowInfo info = new FlashWindowInfo();
            info.cbSize = (uint)Marshal.SizeOf(typeof(FlashWindowInfo));
            info.hwnd = Handle;
            info.dwFlags = flags;
            info.uCount = 0;
            info.dwTimeout = 0;
            FlashWindowEx(ref info);
        }

        private void ApplyRunDurationFromInput()
        {
            hasRunDuration = runDurationMinutesInput.Value > 0;

            if (hasRunDuration)
            {
                runEndsAt = DateTime.Now.AddMinutes((double)runDurationMinutesInput.Value);
            }
        }

        private bool IsRunDurationExpired()
        {
            return running && hasRunDuration && DateTime.Now >= runEndsAt;
        }

        private int GetRemainingRunSeconds()
        {
            return Math.Max(1, (int)Math.Ceiling((runEndsAt - DateTime.Now).TotalSeconds));
        }

        private void UpdateRunningStatus()
        {
            if (!running)
            {
                return;
            }

            if (hasRunDuration)
            {
                statusLabel.Text = "Status: running, " + FormatDuration(GetRemainingRunSeconds()) + " left";
            }
            else
            {
                statusLabel.Text = "Status: running";
            }
        }

        private static string FormatDuration(int totalSeconds)
        {
            if (totalSeconds < 60)
            {
                return totalSeconds + " sec";
            }

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            if (seconds == 0)
            {
                return minutes + " min";
            }

            return minutes + " min " + seconds + " sec";
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
            string keyAction = "";

            if (keyboardActionsCheckBox.Checked)
            {
                keyAction = ", typed \"" + TypeRandomTextAction() + "\"";
            }

            lastActionLabel.Text = "Last action: moved to (" + targetX + ", " + targetY + "), scrolled " + direction + " " + Math.Abs(wheelClicks) + keyAction + ", estimate " + estimatedScrollPosition;
        }

        private string TypeRandomTextAction()
        {
            int length = random.Next(MinTypedCharacters, MaxTypedCharacters + 1);
            string text = "";

            for (int i = 0; i < length; i++)
            {
                if (!running)
                {
                    break;
                }

                char character = TypingCharacters[random.Next(0, TypingCharacters.Length)];
                text += character;

                SendKeys.SendWait(character.ToString());

                if (!running)
                {
                    break;
                }

                if (i < length - 1)
                {
                    System.Threading.Thread.Sleep(GetNextTypingDelayMilliseconds());
                }
            }

            return text;
        }

        private int GetNextTypingDelayMilliseconds()
        {
            if (random.Next(0, 100) < HesitationChancePercent)
            {
                return random.Next(MinHesitationMilliseconds, MaxHesitationMilliseconds + 1);
            }

            return random.Next(MinTypingDelayMilliseconds, MaxTypingDelayMilliseconds + 1);
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
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FlashWindowInfo pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FlashWindowInfo
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

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
