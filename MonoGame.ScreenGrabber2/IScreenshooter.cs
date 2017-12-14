using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.ScreenGrabber2
{
    public interface IScreenshooter
    {
        byte[] GetScreenshotAsBGRA(int windowX, int windowY, out int width, out int height);
    }
}
