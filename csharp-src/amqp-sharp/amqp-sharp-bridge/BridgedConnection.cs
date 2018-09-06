using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

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
//                    Header.TDSHeader header = new Header.TDSHeader(bHeader);
//                    var incomingBuffer = bHeader;

//                    int iMinBufferSize = Math.Max(0x1000, header.LengthIncludingHeader + 1);
//                    if ((bBuffer == null) || (bBuffer.Length < iMinBufferSize))
//                    {
//                        bBuffer = new byte[iMinBufferSize];
//                    }

                    //Console.WriteLine(header.Type);

//                    if (header.Type == (HeaderType)23)
//                    {
//                        iReceived = SocketCouple.ClientBridgeSocket.Receive(bBuffer, 0, 0x1000 - headerSize, SocketFlags.None);
//                    }
//                    else if(header.PayloadSize > 0)
//                    {
//                        //Console.WriteLine("\t{0:N0} bytes package", header.LengthIncludingHeader);
//                        SocketCouple.ClientBridgeSocket.Receive(bBuffer, 0, header.PayloadSize, SocketFlags.None);
//                    }
//                    TDSPacket tdsPacket = new TDSPacket(bHeader, bBuffer, header.PayloadSize);
//                    OnTDSPacketReceived(tdsPacket);

//                    if (tdsMessage == null)
//                        tdsMessage = Message.TDSMessage.CreateFromFirstPacket(tdsPacket);
//                    else
//                        tdsMessage.Packets.Add(tdsPacket);
//
//                    if ((header.StatusBitMask & StatusBitMask.END_OF_MESSAGE) == StatusBitMask.END_OF_MESSAGE)
//                    {
//                        OnTDSMessageReceived(tdsMessage);
//                        tdsMessage = null;
//                    }

//                    SocketCouple.BridgeSQLSocket.Send(bHeader, bHeader.Length, SocketFlags.None);

//                    if (header.Type == (HeaderType)23)
//                    {
//                        SocketCouple.BridgeSQLSocket.Send(bBuffer, iReceived, SocketFlags.None);
//                    }
//                    else
//                    {
//                        SocketCouple.BridgeSQLSocket.Send(bBuffer, header.PayloadSize, SocketFlags.None);
//                    }

//                    SocketCouple.BridgeSQLSocket.Send(bBuffer, header.LengthIncludingHeader, SocketFlags.None);
                    SocketCouple.BridgeSQLSocket.Send(bBuffer, iReceived, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                OnBridgeException(ConnectionType.ClientBridge, e);
            }

            OnConnectionDisconnected(ConnectionType.ClientBridge);
            //Console.WriteLine("Closing InputThread");
        }

        protected virtual void BridgeSQLThread()
        {
            try
            {
                byte[] bBuffer = new byte[SocketCouple.BridgeSQLSocket.ReceiveBufferSize];
                int iReceived = 0;

                while ((iReceived = SocketCouple.BridgeSQLSocket.Receive(bBuffer, SocketFlags.None)) > 0)
                {
//                    Header.TDSHeader header = new Header.TDSHeader(bBuffer);
//                    var tcpHeader = new TcpHeader(bBuffer, iReceived);
                    
//                    Console.WriteLine("[OUT][" + header.Type.ToString() + "]{" + iReceived + "}");
//                    Console.WriteLine("[RAW TCP HEADER]");
//                    Console.WriteLine($"{tcpHeader.SourcePort}");
//                    Console.WriteLine($"{tcpHeader.DestinationPort}");
//                    Console.WriteLine($"{tcpHeader.SequenceNumber}");
//                    Console.WriteLine($"{tcpHeader.AcknowledgementNumber}");
//                    Console.WriteLine($"{tcpHeader.HeaderLength}");
//                    Console.WriteLine($"{tcpHeader.WindowSize}");
//                    Console.WriteLine($"{tcpHeader.UrgentPointer}");
//                    Console.WriteLine($"{tcpHeader.Flags}");
//                    Console.WriteLine($"{tcpHeader.Checksum}");
//                    Console.WriteLine($"{tcpHeader.Data}");
//                    Console.WriteLine($"{tcpHeader.GetBatchText(iReceived)}");
//                    Console.WriteLine($"{tcpHeader.MessageLength}");
//                    Console.WriteLine("[END OF RAW TCP HEADER]");
            
                    SocketCouple.ClientBridgeSocket.Send(bBuffer, iReceived, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                OnBridgeException(ConnectionType.BridgeSQL, e);
            }

            OnConnectionDisconnected(ConnectionType.BridgeSQL);
            //Console.WriteLine("Closing OutputThread");
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
