using System;
using System.Diagnostics;

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

            if (args.Length == 1 && args[0] == "--sway")
                shooter = new SwayScreenshooter();
            else
                shooter = new WinFormsScreenshooter();


            using (var game = new ScreenshotGame(shooter))
                game.Run();
        }
    }
}
