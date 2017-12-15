using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoGame.ScreenGrabber2
{
    class SocketWaiter
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        static UdpClient Client = null;

        public static void Wait()
        {
            if (Client == null)
                Client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 22342));

            IPEndPoint ep;
            while (Client.Available > 0)
            {
                ep = new IPEndPoint(IPAddress.Loopback, 0);
                Client.Receive(ref ep);
            }

            ep = new IPEndPoint(IPAddress.Loopback, 0);
            Client.Receive(ref ep);
        }
    }
}
