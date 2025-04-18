using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;
namespace AutoClickerPro
{
    public partial class MainForm : Form
    {
        // Импорт функций WinAPI
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Константы для работы с мышью
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        // Настройки горячих клавиш
        private const int HOTKEY_ID = 1;
        private const int MOD_NONE = 0x0000;

        // Состояние программы
        private int _clicksCount;
        private bool _isClicking;
        private readonly Timer _clickTimer;
        private MouseButton _currentButton = MouseButton.Left;

        public MainForm()
        {
            InitializeComponent();

            // Инициализация таймера
            _clickTimer = new Timer
            {
                Interval = 100 // Начальное значение
            };
            _clickTimer.Tick += ClickTimer_Tick;

            // Регистрация горячей клавиши
            RegisterHotKey(Handle, HOTKEY_ID, MOD_NONE, (int)Keys.F6);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                ToggleClicker();
            }
            base.WndProc(ref m);
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            var pos = Cursor.Position;

            switch (_currentButton)
            {
                case MouseButton.Left:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)pos.X, (uint)pos.Y, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, (uint)pos.X, (uint)pos.Y, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)pos.X, (uint)pos.Y, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTUP, (uint)pos.X, (uint)pos.Y, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, (uint)pos.X, (uint)pos.Y, 0, 0);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, (uint)pos.X, (uint)pos.Y, 0, 0);
                    break;
            }

            _clicksCount++;
            lblCounter.Text = _clicksCount.ToString();
        }

        private void ToggleClicker()
        {
            _isClicking = !_isClicking;

            if (_isClicking)
            {
                _clickTimer.Start();
                btnStartStop.Text = "Стоп (F6)";
            }
            else
            {
                _clickTimer.Stop();
                btnStartStop.Text = "Старт (F6)";
            }
        }

        private void UpdateSpeed()
        {
            int cps = trackBarSpeed.Value;
            _clickTimer.Interval = cps > 0 ? 1000 / cps : 1;
            lblSpeed.Text = $"{cps} кликов/сек";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }

        // Обработчики событий
        private void BtnStartStop_Click(object sender, EventArgs e) => ToggleClicker();
        private void BtnReset_Click(object sender, EventArgs e) { _clicksCount = 0; lblCounter.Text = "0"; }
        private void TrackBarSpeed_Scroll(object sender, EventArgs e) => UpdateSpeed();
        private void RbLeft_CheckedChanged(object sender, EventArgs e) => _currentButton = MouseButton.Left;
        private void RbRight_CheckedChanged(object sender, EventArgs e) => _currentButton = MouseButton.Right;
        private void RbMiddle_CheckedChanged(object sender, EventArgs e) => _currentButton = MouseButton.Middle;
    }

    public enum MouseButton { Left, Right, Middle }
}