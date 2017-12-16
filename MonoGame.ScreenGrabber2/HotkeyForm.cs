using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoGame.ScreenGrabber2
{
    class HotkeyForm : Form, IHotkeyInterface
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        private static int WM_HOTKEY = 0x0312;

        TaskCompletionSource<bool> WaitForThread;

        bool HasExited = false;

        public HotkeyForm()
        {
            var a = Handle;
            // FormThread = Thread.CurrentThread;

            var res = RegisterHotKey(Handle, 42, (uint)(ModifierKeys.Control | ModifierKeys.Shift), (uint)Keys.F);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                HasExited = true;
            }

            base.WndProc(ref m);
        }

        public void WaitFor()
        {
            HasExited = false;
            while (!HasExited)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
        }

        [Flags]
        public new enum ModifierKeys : uint
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }
    }
}
