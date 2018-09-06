using System.Diagnostics;
using System.Net.Sockets;

namespace amqp_sharp_common
{
    public class Upstream
    {
        private readonly string _upstreamHost;
        private readonly int _upstreamPort;
        private readonly bool _upstreamTls;
        private readonly TraceLevel _logLevel;
        private Socket _bridgeSocket;

        public Upstream(string upstreamHost, int upstreamPort, bool upstreamTls, TraceLevel logLevel)
        {
            _upstreamHost = upstreamHost;
            _upstreamPort = upstreamPort;
            _upstreamTls = upstreamTls;
            _logLevel = logLevel;

            _bridgeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
//            _toClient = new Frame(); // ignoring Channels for now... something to do with concurrency.
        }

        public void Connect(string user, string password, string vHost)
        {

        }

        public bool IsClosed()
        {
            return false;
        }
    }
}