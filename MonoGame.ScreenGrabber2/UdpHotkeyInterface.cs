using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.ScreenGrabber2
{
    class UdpHotkeyInterface : IHotkeyInterface
    {
        UdpClient Client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 33223));

        public void WaitFor()
        {
            IPEndPoint dontCare;

            while (Client.Available > 0)
            {
                dontCare = new IPEndPoint(IPAddress.Any, 0);
                Client.Receive(ref dontCare);
            }

            dontCare = new IPEndPoint(IPAddress.Any, 0);
            Client.Receive(ref dontCare);
        }
    }
}
