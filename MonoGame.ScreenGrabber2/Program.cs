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
        static void Main()
        {
            StartWatch = new Stopwatch();
            StartWatch.Start();

            var winforms = new WinFormsScreenshooter();

            using (var game = new ScreenshotGame(winforms))
                game.Run();
        }
    }
}
