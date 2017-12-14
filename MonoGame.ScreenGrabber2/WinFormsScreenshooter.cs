using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MonoGame.ScreenGrabber2
{
    class WinFormsScreenshooter : Screenshooter
    {
        public byte[] GetScreenshotAsBGRA(int windowX, int windowY, out int width, out int height)
        {
            var screen = Screen.FromPoint(new Point(windowX, windowY));

            width = screen.Bounds.Width;
            height = screen.Bounds.Height;

            byte[] res = new byte[width * height * 4];

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                    g.CopyFromScreen(screen.Bounds.Location, Point.Empty, screen.Bounds.Size);

                var bmpHandle = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    fixed (byte* resPtr = res)
                    {
                        byte* bmpPtr = (byte*)bmpHandle.Scan0;

                        for (int i = 0; i < res.Length; i += 4)
                        {
                            resPtr[i + 0] = bmpPtr[i + 2]; //B
                            resPtr[i + 1] = bmpPtr[i + 1]; //G
                            resPtr[i + 2] = bmpPtr[i + 0]; //R
                            resPtr[i + 3] = bmpPtr[i + 3]; //A
                        }
                    }
                }
                bmp.UnlockBits(bmpHandle);
            }
            return res;
        }
    }
}
