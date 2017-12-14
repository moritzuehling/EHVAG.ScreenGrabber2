using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MonoGame.ScreenGrabber2
{
    class SwayScreenshooter : IScreenshooter
    {
        Dictionary<string, Rectangle> SwayScreens = new Dictionary<string, Rectangle>();


        public byte[] GetScreenshotAsBGRA(int windowX, int windowY, out int width, out int height)
        {
            GenerateSwayScreens();

            var screenDump = GetStdOutOfAsByte("swaygrab", "--raw");
            var targetSize = screenDump.Length / 4;
            var screenRect = SwayScreens.Values.FirstOrDefault(a => a.Width * a.Height == targetSize);

            width = screenRect.Width;
            height = screenRect.Height;

            return screenDump;
        }
        
        private void GenerateSwayScreens()
        {
            var rawJson = GetStdOutOf("swaymsg", "-t get_outputs -r");

            var swayMsg = JArray.Parse(rawJson);

            foreach (JObject screen in swayMsg)
            {
                var rect = screen["rect"];
                var srect = new Rectangle(rect["x"].Value<int>(), rect["y"].Value<int>(), rect["width"].Value<int>(), rect["height"].Value<int>());

                // Because sway doesn't support
                // [title="^EHVAG_GLOBAL$"] fullscreen toggle global
                // or anything similar, we can only capture the active
                // screen, and that's where we're also fullscreen'd.

                SwayScreens[screen["name"].Value<string>()] = srect;
            }
        }

        private Rectangle GetCompleteBounds()
        {
            int top = int.MaxValue;
            int left = int.MaxValue;
            int right = int.MinValue;
            int bottom = int.MinValue;

            foreach (var screen in SwayScreens)
            {
                Console.WriteLine("Screen {0}:", screen.Key);
                Console.WriteLine("  Rect: {0}", screen.Value.ToString());

                if (top > screen.Value.Left)
                    top = screen.Value.Left;

                if (left > screen.Value.Left)
                    left = screen.Value.Left;

                if (bottom < screen.Value.Bottom)
                    bottom = screen.Value.Bottom;

                if (right < screen.Value.Right)
                    right = screen.Value.Right;

            }

            var res = new Rectangle(top, left, right - left, bottom - top);

            Console.WriteLine("--------");
            Console.WriteLine("Final Bounds: {0}:", res);

            return res;
        }

        private string GetStdOutOf(string process, string args)
        {
            var psi = new ProcessStartInfo(process, args);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            var proc = Process.Start(psi);

            return proc.StandardOutput.ReadToEnd();
        }

        private byte[] GetStdOutOfAsByte(string process, string args)
        {
            var psi = new ProcessStartInfo(process, args);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            var proc = Process.Start(psi);

            using (var ms = new MemoryStream())
            {
                proc.StandardOutput.BaseStream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
