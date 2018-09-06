using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using amqp_sharp_common;

namespace amqp_sharp_bridge
{
    public enum ConnectionType { ClientBridge, BridgeSQL };

    public class BridgedConnection
    {
        public BridgeAcceptor BridgeAcceptor { get; protected set; }
        public SocketCouple SocketCouple { get; protected set; }

        public BridgedConnection(BridgeAcceptor BridgeAcceptor, SocketCouple SocketCouple)
        {
            this.BridgeAcceptor = BridgeAcceptor;
            this.SocketCouple = SocketCouple;
        }

        public void Start()
        {
            Thread tIn = new Thread(new ThreadStart(ClientBridgeThread));
            tIn.IsBackground = true;
            tIn.Start();

            Thread tOut = new Thread(new ThreadStart(BridgeSQLThread));
            tOut.IsBackground = true;
            tOut.Start();
        }

        protected virtual void ClientBridgeThread()
        {
            try
            {
//                byte[] bBuffer = null;
                byte[] bBuffer = new byte[SocketCouple.ClientBridgeSocket.ReceiveBufferSize];
//                var headerSize = Header.TDSHeader.HEADER_SIZE;
                var headerSize = 8;
                byte[] bHeader = new byte[headerSize];
                int iReceived = 0;

//                Message.TDSMessage tdsMessage = null;
                

//                while ((iReceived = SocketCouple.ClientBridgeSocket.Receive(bHeader, headerSize, SocketFlags.None)) > 0)
                while ((iReceived = SocketCouple.ClientBridgeSocket.Receive(bBuffer, SocketFlags.None)) > 0)
                {
                    Console.WriteLine($"[FROM CLIENT][{iReceived} bytes received]");
                    var amqpHeader = new byte[8];
                    Array.Copy(bBuffer, 0, amqpHeader, 0, 8);
                    var isProtocolHeader = isProtocolHeaderPacket(amqpHeader);
                    if (isProtocolHeader)
                    {
                        byte asciiDifference = 48; 
                        amqpHeader[4] = amqpHeader[4].Add(asciiDifference);
                        amqpHeader[5] = amqpHeader[5].Add(asciiDifference);
                        amqpHeader[6] = amqpHeader[6].Add(asciiDifference);
                        amqpHeader[7] = amqpHeader[7].Add(asciiDifference);
                        Console.WriteLine($"    [FROM CLIENT][PROTOCOL HEADER -> {string.Join("-",amqpHeader.Select(p=>Encoding.UTF8.GetString(new [] {p})))}]");
                    }
                    else
                    {
                        var frameType = (FrameType) amqpHeader[0];
                        var frameChannel = BitConverter.ToUInt16(new [] {amqpHeader[1], amqpHeader[2]},0);
                        var frameSize = BitConverter.ToUInt32(amqpHeader, 3);
                    
                        Console.WriteLine($"    [FROM CLIENT][GENERAL -> {frameType}]");
                        Console.WriteLine($"    [FROM CLIENT][GENERAL -> {frameChannel}]");
                        Console.WriteLine($"    [FROM CLIENT][GENERAL -> {frameSize}]");
                    }
                    
                    SocketCouple.BridgeSQLSocket.Send(bBuffer, iReceived, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                OnBridgeException(ConnectionType.ClientBridge, e);
            }

            OnConnectionDisconnected(ConnectionType.ClientBridge);
        }

        private bool isProtocolHeaderPacket(byte[] amqpHeader)
        {
            return amqpHeader.SequenceEqual(AMQP.PROTOCOL_START);
        }
        
        protected virtual void BridgeSQLThread()
        {
            try
            {
                byte[] bBuffer = new byte[SocketCouple.BridgeSQLSocket.ReceiveBufferSize];
                int iReceived = 0;

                while ((iReceived = SocketCouple.BridgeSQLSocket.Receive(bBuffer, SocketFlags.None)) > 0)
                {
                    Console.WriteLine($"[FROM SERVER][{iReceived} bytes received]");
                    var amqpHeader = new byte[8];
                    Array.Copy(bBuffer, 0, amqpHeader, 0, 8);
//                    var isProtocolHeader = isProtocolHeaderPacket(amqpHeader);
                    var frameType = (FrameType) amqpHeader[0];
                    var frameChannel = BitConverter.ToUInt16(new [] {amqpHeader[1], amqpHeader[2]},0);
                    var frameSize = BitConverter.ToUInt32(amqpHeader, 3);
                    
                    Console.WriteLine($"    [FROM SERVER][GENERAL -> {frameType}]");
                    Console.WriteLine($"    [FROM SERVER][GENERAL -> {frameChannel}]");
                    Console.WriteLine($"    [FROM SERVER][GENERAL -> {frameSize}]");
                    
                    SocketCouple.ClientBridgeSocket.Send(bBuffer, iReceived, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                OnBridgeException(ConnectionType.BridgeSQL, e);
            }

            OnConnectionDisconnected(ConnectionType.BridgeSQL);
        }


        #region Event handlers
//        protected virtual void OnTDSMessageReceived(Message.TDSMessage msg)
//        {
//            BridgeAcceptor.OnTDSMessageReceived(this, msg);
//        }
//
//        protected virtual void OnTDSPacketReceived(Packet.TDSPacket packet)
//        {
//            BridgeAcceptor.OnTDSMessageReceived(this, packet);
//        }

        protected virtual void OnBridgeException(ConnectionType ct, Exception exce)
        {
            BridgeAcceptor.OnBridgeException(this, ct, exce);
        }

        protected virtual void OnConnectionDisconnected(ConnectionType ct)
        {
            BridgeAcceptor.OnConnectionDisconnected(this, ct);

            switch (ct)
            {
                case ConnectionType.ClientBridge:
                    if(SocketCouple.BridgeSQLSocket.Connected)
                        SocketCouple.BridgeSQLSocket.Disconnect(false);
                    break;
                case ConnectionType.BridgeSQL:
                    if (SocketCouple.ClientBridgeSocket.Connected)                        
                        SocketCouple.ClientBridgeSocket.Disconnect(false);
                    break;
            }
        }
        #endregion
    }
}
