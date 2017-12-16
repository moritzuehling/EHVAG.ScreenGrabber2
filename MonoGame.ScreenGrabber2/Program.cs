using System;
using System.Diagnostics;
using System.Linq;

namespace MonoGame.ScreenGrabber2
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static Stopwatch StartWatch;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            StartWatch = new Stopwatch();
            StartWatch.Start();


            IScreenshooter shooter = null;
            IHotkeyInterface hotkeyInterface = null;

            if (args.Contains("--sway"))
                shooter = new SwayScreenshooter();
            else
                shooter = new WinFormsScreenshooter();

            if (args.Contains("--udp"))
                hotkeyInterface = new UdpHotkeyInterface();
            else
                hotkeyInterface = new HotkeyForm();

            using (var game = new ScreenshotGame(shooter, hotkeyInterface))
                game.Run();
        }
    }
}
