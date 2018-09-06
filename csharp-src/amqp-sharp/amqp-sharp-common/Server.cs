using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace amqp_sharp_common
{
    public class Server
    {
        private TraceLevel _logLevel;
        private int _clientConnections;
        private Pool _pool;
        private bool _running;

        public Server(string upstream_host, int upstream_port, bool upstream_tls, TraceLevel log_level = TraceLevel.Info)
        {
            _running = true;
            _logLevel = log_level;
            _clientConnections = 0;
            _pool = new Pool(upstream_host, upstream_port, upstream_tls, _logLevel);
            var tlsDisplay = upstream_tls ? "TLS" : ""; 
            Trace.TraceInformation($"Proxy upstream: {upstream_host}:{upstream_port} {tlsDisplay}");
        }

        void Listen(string address, int port)
        {
            
        }

        void ListenTls(string address, int port, string certPath, string keyPath)
        {
            
        }

        void HandleConnection(Socket socket, string remoteAddress)
        {
            
        }
    }
}