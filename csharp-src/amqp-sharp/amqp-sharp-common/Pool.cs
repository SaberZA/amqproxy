using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace amqp_sharp_common
{
    public class Pool
    {
        private readonly string _upstreamHost;
        private readonly int _upstreamPort;
        private readonly bool _upstreamTls;
        private readonly TraceLevel _logLevel;
        private List<Upstream> _pools;
        public int Size { get; }

        public Pool(string upstreamHost, int upstreamPort, bool upstreamTls, TraceLevel logLevel)
        {
            _upstreamHost = upstreamHost;
            _upstreamPort = upstreamPort;
            _upstreamTls = upstreamTls;
            _logLevel = logLevel;
            _pools = new List<Upstream>();
            Size = 0;
        }

        void Borrow(string user, string password, string vHost, Upstream block = null)
        {
            var upstream = new Upstream(_upstreamHost, _upstreamPort, _upstreamTls, _logLevel);
            
            try
            {
                upstream.Connect(user, password, vHost);
                _pools.Add(upstream);
            }
            catch (Exception e)
            {
                Trace.TraceError("Upstream connection could not be established");
                if (upstream.IsClosed())
                {
                    Trace.TraceError("Upstream connection closed when returned");
                }
                throw;
            }
        }
    }
}