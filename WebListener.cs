using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace VK_Unicorn
{
    class WebListener
    {
        readonly TcpListener tcpListener;

        public WebListener(int port)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            Task.Factory.StartNew(() => ListenLoop());
        }

        async void ListenLoop()
        {
            while (true)
            {
                var socket = await tcpListener.AcceptSocketAsync();
                if (socket == null)
                {
                    break;
                }

                var client = new WebClient(socket);

#pragma warning disable 4014
                Task.Factory.StartNew(client.Do);
#pragma warning restore 4014
            }
        }
    }
}